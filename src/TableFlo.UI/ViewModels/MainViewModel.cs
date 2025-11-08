using System.Collections.ObjectModel;
using System.IO;
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
    private readonly IRotationStringService _stringService;
    private readonly TableFloDbContext _context;

    public MainViewModel(
        IUnitOfWork unitOfWork,
        IRotationService rotationService,
        ISchedulingService schedulingService,
        IAuditService auditService,
        IRotationStringService stringService,
        TableFloDbContext context)
    {
        _unitOfWork = unitOfWork;
        _rotationService = rotationService;
        _schedulingService = schedulingService;
        _auditService = auditService;
        _stringService = stringService;
        _context = context;

        // Initialize commands
        PushAllTablesCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(PushAllTablesAsync);
        PushSingleTableCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand<int>(PushSingleTableAsync);
        SwapDealersCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand<int>(SwapDealersAsync);
        SendDealerHomeCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand<int>(SendDealerHomeAsync);
        GenerateScheduleCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(GenerateScheduleAsync);
        SendToBreakCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(SendToBreakAsync);
        ReturnFromBreakCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand<int>(ReturnFromBreakAsync);
        AssignDealerToTableCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand<string?>(AssignDealerToTableAsync);
        ExecuteStringRotationCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand<int>(ExecuteStringRotationAsync);
        ExportScheduleCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(ExportScheduleAsync);
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

    private int _availableDealers;
    public int AvailableDealers
    {
        get => _availableDealers;
        set => SetProperty(ref _availableDealers, value);
    }

    private int _dealingCount;
    public int DealingCount
    {
        get => _dealingCount;
        set => SetProperty(ref _dealingCount, value);
    }

    private int _reliefNeeded;
    public int ReliefNeeded
    {
        get => _reliefNeeded;
        set => SetProperty(ref _reliefNeeded, value);
    }

    private TableRowViewModel? _selectedCurrentTable;
    public TableRowViewModel? SelectedCurrentTable
    {
        get => _selectedCurrentTable;
        set => SetProperty(ref _selectedCurrentTable, value);
    }

    private ObservableCollection<RotationStringViewModel> _rotationStrings = new();
    public ObservableCollection<RotationStringViewModel> RotationStrings
    {
        get => _rotationStrings;
        set => SetProperty(ref _rotationStrings, value);
    }

    #endregion

    #region Commands

    public ICommand PushAllTablesCommand { get; }
    public ICommand PushSingleTableCommand { get; }
    public ICommand SwapDealersCommand { get; }
    public ICommand SendDealerHomeCommand { get; }
    public ICommand GenerateScheduleCommand { get; }
    public ICommand SendToBreakCommand { get; }
    public ICommand ReturnFromBreakCommand { get; }
    public ICommand AssignDealerToTableCommand { get; }
    public ICommand ExecuteStringRotationCommand { get; }
    public ICommand ExportScheduleCommand { get; }
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
                var currentDealerId = currentAssignment?.DealerId ?? 0;
                var timeInMinutes = currentAssignment != null
                    ? (int)(DateTime.UtcNow - currentAssignment.StartTime).TotalMinutes
                    : 0;

                // Format time display (hours and minutes, or just minutes if < 60)
                string timeDisplay;
                if (timeInMinutes >= 60)
                {
                    int hours = timeInMinutes / 60;
                    int minutes = timeInMinutes % 60;
                    timeDisplay = $"{hours}h {minutes}m";
                }
                else
                {
                    timeDisplay = $"{timeInMinutes}m";
                }

                currentTablesList.Add(new TableRowViewModel
                {
                    TableId = table.Id,
                    TableNumber = table.TableNumber,
                    GameType = table.GameType.ToString(),
                    CurrentDealerName = currentDealerName,
                    CurrentDealerId = currentDealerId,
                    TimeInMinutes = timeDisplay,
                    ActualMinutes = timeInMinutes
                });

                // Next dealer info (replacement)
                var nextAssignment = table.NextAssignments.FirstOrDefault();
                var nextDealerName = nextAssignment?.Dealer?.Employee?.FullName ?? "—";
                var nextDealerId = nextAssignment?.DealerId ?? 0;

                nextTablesList.Add(new TableRowViewModel
                {
                    TableId = table.Id,
                    TableNumber = table.TableNumber,
                    GameType = table.GameType.ToString(),
                    NextDealerName = nextDealerName,
                    NextDealerId = nextDealerId,
                    ReplacingDealerName = currentDealerName // Show who they're replacing
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
            
            // Calculate relief pool stats
            var allDealers = await _context.Dealers.ToListAsync();
            AvailableDealers = allDealers.Count(d => d.Status == DealerStatus.Available);
            DealingCount = allDealers.Count(d => d.Status == DealerStatus.Dealing);
            ReliefNeeded = tables.Count - DealingCount; // Tables without dealers
            
            StatusMessage = $"Loaded {tables.Count} tables | {AvailableDealers} available | {DealingCount} dealing | {dealersOnBreak.Count()} on break";
            
            // Load rotation strings
            await LoadRotationStringsAsync();
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
    /// Load rotation strings
    /// </summary>
    private async Task LoadRotationStringsAsync()
    {
        try
        {
            var strings = await _stringService.GetAllStringsAsync();
            var stringViewModels = new ObservableCollection<RotationStringViewModel>();

            foreach (var str in strings)
            {
                var dealerCount = str.DealerAssignments.Count(dsa => dsa.IsActive);
                stringViewModels.Add(new RotationStringViewModel
                {
                    Id = str.Id,
                    Name = str.Name,
                    Description = str.Description,
                    DealerCount = dealerCount
                });
            }

            RotationStrings = stringViewModels;
        }
        catch
        {
            // Silently fail - strings are optional
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

            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;
            
            // Execute push for each open table
            var openTables = await _context.Tables
                .Where(t => t.Status == TableStatus.Open)
                .ToListAsync();
                
            foreach (var table in openTables)
            {
                await _rotationService.ExecutePushAsync(table.Id, employeeId);
            }

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

            var scheduleResult = await _schedulingService.GenerateScheduleAsync(DateTime.UtcNow);

            if (!scheduleResult.Success)
            {
                MessageBox.Show($"Failed to generate schedule: {scheduleResult.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Clear existing next assignments
            var existingNextAssignments = await _context.Assignments
                .Where(a => !a.IsCurrent)
                .ToListAsync();

            foreach (var assignment in existingNextAssignments)
            {
                _context.Assignments.Remove(assignment);
            }

            // Create new next assignments
            foreach (var assignment in scheduleResult.Assignments)
            {
                assignment.IsCurrent = false;
                await _context.Assignments.AddAsync(assignment);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                SessionManager.CurrentEmployee?.Id ?? 0,
                ActionType.ScheduleGenerated,
                $"AI schedule generated for {scheduleResult.Assignments.Count} tables"
            );

            await LoadDashboardDataAsync();

            MessageBox.Show($"AI schedule generated for {scheduleResult.Assignments.Count} tables!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                .FirstOrDefaultAsync(e => e.EmployeeNumber == NewBreakDealerNumber);

            if (employee == null)
            {
                MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get dealer record
            var dealer = await _context.Dealers
                .FirstOrDefaultAsync(d => d.EmployeeId == employee.Id);

            if (dealer == null)
            {
                MessageBox.Show("This employee is not a dealer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;
            await _rotationService.SendToBreakAsync(dealer.Id, "Regular", 20, employeeId);

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

            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;
            await _rotationService.ReturnFromBreakAsync(dealerId, employeeId);

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

    /// <summary>
    /// Push single table - swap current and next dealer for one table
    /// </summary>
    private async Task PushSingleTableAsync(int tableId)
    {
        try
        {
            IsLoading = true;
            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;
            
            await _rotationService.ExecutePushAsync(tableId, employeeId);
            
            await _auditService.LogActionAsync(
                employeeId,
                ActionType.PushExecuted,
                $"Single table push executed for table ID {tableId}"
            );

            await LoadDashboardDataAsync();
            MessageBox.Show("Table pushed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error pushing table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Swap two specific dealers
    /// </summary>
    private async Task SwapDealersAsync(int tableId)
    {
        try
        {
            var table = await _context.Tables
                .Include(t => t.CurrentAssignments)
                    .ThenInclude(a => a.Dealer)
                        .ThenInclude(d => d!.Employee)
                .FirstOrDefaultAsync(t => t.Id == tableId);

            if (table == null || !table.CurrentAssignments.Any())
            {
                MessageBox.Show("No dealer assigned to this table.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var currentDealer = table.CurrentAssignments.First().Dealer;
            if (currentDealer == null)
                return;

            // Get available dealers for swap
            var availableDealers = await _context.Dealers
                .Include(d => d.Employee)
                .Where(d => d.Id != currentDealer.Id && 
                           (d.Status == DealerStatus.Available || d.Status == DealerStatus.Dealing))
                .ToListAsync();

            if (!availableDealers.Any())
            {
                MessageBox.Show("No available dealers to swap with.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Simple swap - get first available dealer (in real app, would show dialog)
            var swapDealer = availableDealers.First();
            
            var result = MessageBox.Show(
                $"Swap {currentDealer.Employee?.FullName} with {swapDealer.Employee?.FullName}?",
                "Confirm Swap",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;

            // End current assignment
            var currentAssignment = table.CurrentAssignments.First();
            currentAssignment.EndTime = DateTime.UtcNow;

            // Assign new dealer
            await _rotationService.AssignDealerAsync(swapDealer.Id, tableId, true, employeeId);

            await _auditService.LogActionAsync(
                employeeId,
                ActionType.ManualOverride,
                $"Swapped {currentDealer.Employee?.FullName} with {swapDealer.Employee?.FullName} at table {table.TableNumber}"
            );

            await LoadDashboardDataAsync();
            MessageBox.Show("Dealers swapped successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error swapping dealers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Send dealer home (remove from shift)
    /// </summary>
    private async Task SendDealerHomeAsync(int dealerId)
    {
        try
        {
            var dealer = await _context.Dealers
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Id == dealerId);

            if (dealer == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to send {dealer.Employee?.FullName} home?",
                "Confirm Send Home",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;

            // Remove from any current assignments
            var currentAssignment = await _context.Assignments
                .Where(a => a.DealerId == dealerId && a.IsCurrent && a.EndTime == null)
                .FirstOrDefaultAsync();

            if (currentAssignment != null)
            {
                currentAssignment.EndTime = DateTime.UtcNow;
            }

            // Mark dealer as sent home
            dealer.Status = DealerStatus.SentHome;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                SessionManager.CurrentEmployee?.Id ?? 0,
                ActionType.DealerSentHome,
                $"Sent {dealer.Employee?.FullName} home"
            );

            await LoadDashboardDataAsync();
            MessageBox.Show($"{dealer.Employee?.FullName} sent home successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending dealer home: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Assign specific dealer to specific table
    /// </summary>
    private async Task AssignDealerToTableAsync(string? parameters)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parameters))
                return;

            // Parameters format: "dealerId,tableId"
            var parts = parameters.Split(',');
            if (parts.Length != 2)
                return;

            var dealerId = int.Parse(parts[0]);
            var tableId = int.Parse(parts[1]);

            IsLoading = true;
            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;

            await _rotationService.AssignDealerAsync(dealerId, tableId, true, employeeId);

            await _auditService.LogActionAsync(
                employeeId,
                ActionType.DealerAssigned,
                $"Assigned dealer ID {dealerId} to table ID {tableId}"
            );

            await LoadDashboardDataAsync();
            MessageBox.Show("Dealer assigned successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error assigning dealer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Execute rotation for a specific string
    /// </summary>
    private async Task ExecuteStringRotationAsync(int stringId)
    {
        try
        {
            var rotationString = await _stringService.GetStringByIdAsync(stringId);
            if (rotationString == null)
                return;

            var result = MessageBox.Show(
                $"Execute rotation for '{rotationString.Name}'?\n\nThis will assign all dealers in this string to their designated tables.",
                "Confirm String Rotation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;

            var success = await _stringService.ExecuteStringRotationAsync(stringId, employeeId);

            if (success)
            {
                await LoadDashboardDataAsync();
                MessageBox.Show($"'{rotationString.Name}' rotation executed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to execute string rotation.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error executing string rotation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Export current schedule to text file
    /// </summary>
    private async Task ExportScheduleAsync()
    {
        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"TableFlo_Schedule_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var tables = await _context.Tables
                    .Include(t => t.CurrentAssignments)
                        .ThenInclude(a => a.Dealer)
                            .ThenInclude(d => d!.Employee)
                    .Include(t => t.NextAssignments)
                        .ThenInclude(a => a.Dealer)
                            .ThenInclude(d => d!.Employee)
                    .Where(t => t.Status == Core.Enums.TableStatus.Open)
                    .OrderBy(t => t.TableNumber)
                    .ToListAsync();

                var lines = new List<string>
                {
                    "TableFlo - Dealer Schedule Export",
                    $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    "",
                    "=".PadRight(80, '='),
                    "CURRENT ASSIGNMENTS",
                    "=".PadRight(80, '='),
                    ""
                };

                foreach (var table in tables)
                {
                    var current = table.CurrentAssignments.FirstOrDefault();
                    var dealerName = current?.Dealer?.Employee?.FullName ?? "No Dealer";
                    var timeIn = current != null ? (int)(DateTime.UtcNow - current.StartTime).TotalMinutes : 0;
                    
                    lines.Add($"Table: {table.TableNumber} ({table.GameType})");
                    lines.Add($"  Current Dealer: {dealerName}");
                    lines.Add($"  Time at Table: {timeIn} minutes");
                    lines.Add("");
                }

                lines.Add("=".PadRight(80, '='));
                lines.Add("NEXT ASSIGNMENTS");
                lines.Add("=".PadRight(80, '='));
                lines.Add("");

                foreach (var table in tables)
                {
                    var next = table.NextAssignments.FirstOrDefault();
                    var nextDealer = next?.Dealer?.Employee?.FullName ?? "TBD";
                    
                    lines.Add($"Table: {table.TableNumber} ({table.GameType})");
                    lines.Add($"  Next Dealer: {nextDealer}");
                    lines.Add("");
                }

                await File.WriteAllLinesAsync(saveDialog.FileName, lines);

                await _auditService.LogActionAsync(
                    SessionManager.CurrentEmployee?.Id ?? 0,
                    ActionType.ReportGenerated,
                    $"Exported schedule to {saveDialog.FileName}"
                );

                MessageBox.Show($"Schedule exported successfully to:\n{saveDialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    public int CurrentDealerId { get; set; }
    public string TimeInMinutes { get; set; } = string.Empty;
    public int ActualMinutes { get; set; } // For calculations/sorting
    public string NextDealerName { get; set; } = string.Empty;
    public int NextDealerId { get; set; }
    public string ReplacingDealerName { get; set; } = string.Empty; // Shows who is being replaced
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

/// <summary>
/// ViewModel for rotation strings
/// </summary>
public class RotationStringViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DealerCount { get; set; }
}
