using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Enums;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Data.Interfaces;
using TableFlo.Services.Interfaces;

namespace TableFlo.UI.ViewModels;

/// <summary>
/// Main Dashboard ViewModel - Handles all dashboard operations
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRotationService _rotationService;
    private readonly ISchedulingService _schedulingService;
    private readonly IAuditService _auditService;
    private readonly TableFloDbContext _context;

    public MainViewModel(
        IUnitOfWork unitOfWork,
        IRotationService rotationService,
        ISchedulingService schedulingService,
        IAuditService auditService,
        TableFloDbContext context)
    {
        _unitOfWork = unitOfWork;
        _rotationService = rotationService;
        _schedulingService = schedulingService;
        _auditService = auditService;
        _context = context;

        // Initialize commands
        PushAllTablesCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(PushAllTablesAsync);
        GenerateScheduleCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(GenerateScheduleAsync);
        SendToBreakCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(SendToBreakAsync);
        ReturnFromBreakCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand<int>(ReturnFromBreakAsync);
        RefreshCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadDashboardDataAsync);

        // Load data on startup
        _ = LoadDashboardDataAsync();
    }

    #region Properties

    private ObservableCollection<TableRowViewModel> _currentTables = new();
    public ObservableCollection<TableRowViewModel> CurrentTables
    {
        get => _currentTables;
        set => SetProperty(ref _currentTables, value);
    }

    private ObservableCollection<TableRowViewModel> _nextTables = new();
    public ObservableCollection<TableRowViewModel> NextTables
    {
        get => _nextTables;
        set => SetProperty(ref _nextTables, value);
    }

    private ObservableCollection<DealerBreakViewModel> _dealersOnBreak = new();
    public ObservableCollection<DealerBreakViewModel> DealersOnBreak
    {
        get => _dealersOnBreak;
        set => SetProperty(ref _dealersOnBreak, value);
    }

    private string _newBreakDealerNumber = string.Empty;
    public string NewBreakDealerNumber
    {
        get => _newBreakDealerNumber;
        set => SetProperty(ref _newBreakDealerNumber, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    #endregion

    #region Commands

    public ICommand PushAllTablesCommand { get; }
    public ICommand GenerateScheduleCommand { get; }
    public ICommand SendToBreakCommand { get; }
    public ICommand ReturnFromBreakCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Load all dashboard data
    /// </summary>
    public async Task LoadDashboardDataAsync()
    {
        try
        {
            IsLoading = true;

            // Load open tables with current and next assignments
            var tables = await _context.Tables
                .Include(t => t.CurrentAssignments)
                    .ThenInclude(a => a.Dealer)
                        .ThenInclude(d => d!.Employee)
                .Include(t => t.NextAssignments)
                    .ThenInclude(a => a.Dealer)
                        .ThenInclude(d => d!.Employee)
                .Where(t => t.Status == TableStatus.Open)
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            // Build current tables view
            var currentTablesList = new ObservableCollection<TableRowViewModel>();
            var nextTablesList = new ObservableCollection<TableRowViewModel>();

            foreach (var table in tables)
            {
                // Current dealer info
                var currentAssignment = table.CurrentAssignments.FirstOrDefault();
                var currentDealerName = currentAssignment?.Dealer?.Employee?.FullName ?? "No Dealer";
                var timeInMinutes = currentAssignment != null
                    ? (int)(DateTime.UtcNow - currentAssignment.StartTime).TotalMinutes
                    : 0;

                currentTablesList.Add(new TableRowViewModel
                {
                    TableId = table.Id,
                    TableNumber = table.TableNumber,
                    GameType = table.GameType.ToString(),
                    CurrentDealerName = currentDealerName,
                    TimeInMinutes = $"{timeInMinutes}m"
                });

                // Next dealer info
                var nextAssignment = table.NextAssignments.FirstOrDefault();
                var nextDealerName = nextAssignment?.Dealer?.Employee?.FullName ?? "TBD";

                nextTablesList.Add(new TableRowViewModel
                {
                    TableId = table.Id,
                    TableNumber = table.TableNumber,
                    GameType = table.GameType.ToString(),
                    NextDealerName = nextDealerName
                });
            }

            CurrentTables = currentTablesList;
            NextTables = nextTablesList;

            // Load dealers on break
            var dealersOnBreak = await _rotationService.GetDealersOnBreakAsync();
            var breakList = new ObservableCollection<DealerBreakViewModel>();

            foreach (var dealer in dealersOnBreak)
            {
                if (dealer.Employee != null)
                {
                    breakList.Add(new DealerBreakViewModel
                    {
                        DealerId = dealer.Id,
                        DealerName = dealer.Employee.FullName,
                        EmployeeNumber = dealer.Employee.EmployeeNumber
                    });
                }
            }

            DealersOnBreak = breakList;
            StatusMessage = $"Loaded {tables.Count} tables, {dealersOnBreak.Count()} on break";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Execute push for all tables (swap current and next dealers)
    /// </summary>
    private async Task PushAllTablesAsync()
    {
        try
        {
            var result = MessageBox.Show(
                "Are you sure you want to push all tables? This will rotate all dealers.",
                "Confirm Push",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            StatusMessage = "Executing push...";

            await _rotationService.ExecutePushAsync();

            await _auditService.LogActionAsync(
                SessionManager.CurrentEmployee?.Id ?? 0,
                ActionType.PushExecuted,
                "Push executed for all tables"
            );

            await LoadDashboardDataAsync();

            MessageBox.Show("Push completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error executing push: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    /// <summary>
    /// Generate AI schedule for next rotation
    /// </summary>
    private async Task GenerateScheduleAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Generating AI schedule...";

            var openTables = await _context.Tables
                .Where(t => t.Status == TableStatus.Open)
                .ToListAsync();

            if (!openTables.Any())
            {
                MessageBox.Show("No open tables to schedule.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var schedule = await _schedulingService.GenerateScheduleAsync(DateTime.UtcNow);

            // Clear existing next assignments
            var existingNextAssignments = await _context.Assignments
                .Where(a => !a.IsCurrent)
                .ToListAsync();

            foreach (var assignment in existingNextAssignments)
            {
                _context.Assignments.Remove(assignment);
            }

            // Create new next assignments
            foreach (var assignment in schedule)
            {
                assignment.IsCurrent = false;
                await _context.Assignments.AddAsync(assignment);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                SessionManager.CurrentEmployee?.Id ?? 0,
                ActionType.ScheduleGenerated,
                $"AI schedule generated for {schedule.Count} tables"
            );

            await LoadDashboardDataAsync();

            MessageBox.Show($"AI schedule generated for {schedule.Count} tables!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    /// <summary>
    /// Send dealer to break
    /// </summary>
    private async Task SendToBreakAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewBreakDealerNumber))
            {
                MessageBox.Show("Please enter a dealer employee number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            // Find dealer by employee number
            var employee = await _context.Employees
                .Include(e => e.Dealer)
                .FirstOrDefaultAsync(e => e.EmployeeNumber == NewBreakDealerNumber);

            if (employee?.Dealer == null)
            {
                MessageBox.Show("Dealer not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await _rotationService.SendToBreakAsync(employee.Dealer.Id);

            await _auditService.LogActionAsync(
                SessionManager.CurrentEmployee?.Id ?? 0,
                ActionType.DealerSentToBreak,
                $"Sent {employee.FullName} to break"
            );

            NewBreakDealerNumber = string.Empty;
            await LoadDashboardDataAsync();

            MessageBox.Show($"{employee.FullName} sent to break.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending to break: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Return dealer from break
    /// </summary>
    private async Task ReturnFromBreakAsync(int dealerId)
    {
        try
        {
            IsLoading = true;

            var dealer = await _context.Dealers
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Id == dealerId);

            if (dealer == null)
                return;

            await _rotationService.ReturnFromBreakAsync(dealerId);

            await _auditService.LogActionAsync(
                SessionManager.CurrentEmployee?.Id ?? 0,
                ActionType.DealerReturnedFromBreak,
                $"{dealer.Employee?.FullName} returned from break"
            );

            await LoadDashboardDataAsync();

            MessageBox.Show($"{dealer.Employee?.FullName} returned from break.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error returning from break: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}

/// <summary>
/// ViewModel for table rows in the grid
/// </summary>
public class TableRowViewModel
{
    public int TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public string CurrentDealerName { get; set; } = string.Empty;
    public string TimeInMinutes { get; set; } = string.Empty;
    public string NextDealerName { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for dealers on break
/// </summary>
public class DealerBreakViewModel
{
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
}
