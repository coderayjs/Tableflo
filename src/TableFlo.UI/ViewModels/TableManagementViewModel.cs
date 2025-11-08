using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Enums;
using TableFlo.Core.Models;
using TableFlo.Data.Interfaces;
using TableFlo.Services.Interfaces;

namespace TableFlo.UI.ViewModels;

/// <summary>
/// Table Management ViewModel - Manage casino tables
/// </summary>
public class TableManagementViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public TableManagementViewModel(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;

        // Initialize commands
        AddTableCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(AddTable);
        EditTableCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(EditTable, () => SelectedTable != null);
        DeleteTableCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(DeleteTable, () => SelectedTable != null);
        SaveTableCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(SaveTableAsync);
        CancelEditCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CancelEdit);
        OpenTableCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(OpenTable, () => SelectedTable != null);
        CloseTableCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CloseTable, () => SelectedTable != null);
        RefreshCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadTablesAsync);

        _ = LoadTablesAsync();
    }

    #region Properties

    private ObservableCollection<TableViewModel> _tables = new();
    public ObservableCollection<TableViewModel> Tables
    {
        get => _tables;
        set => SetProperty(ref _tables, value);
    }

    private TableViewModel? _selectedTable;
    public TableViewModel? SelectedTable
    {
        get => _selectedTable;
        set
        {
            SetProperty(ref _selectedTable, value);
            (EditTableCommand as CommunityToolkit.Mvvm.Input.RelayCommand)?.NotifyCanExecuteChanged();
            (DeleteTableCommand as CommunityToolkit.Mvvm.Input.RelayCommand)?.NotifyCanExecuteChanged();
            (OpenTableCommand as CommunityToolkit.Mvvm.Input.RelayCommand)?.NotifyCanExecuteChanged();
            (CloseTableCommand as CommunityToolkit.Mvvm.Input.RelayCommand)?.NotifyCanExecuteChanged();
        }
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    private bool _isNewTable;
    public bool IsNewTable
    {
        get => _isNewTable;
        set => SetProperty(ref _isNewTable, value);
    }

    // Edit Form Properties
    private string _tableNumber = string.Empty;
    public string TableNumber
    {
        get => _tableNumber;
        set => SetProperty(ref _tableNumber, value);
    }

    private GameType _gameType = GameType.Blackjack;
    public GameType GameType
    {
        get => _gameType;
        set => SetProperty(ref _gameType, value);
    }

    private decimal _minBet;
    public decimal MinBet
    {
        get => _minBet;
        set => SetProperty(ref _minBet, value);
    }

    private decimal _maxBet;
    public decimal MaxBet
    {
        get => _maxBet;
        set => SetProperty(ref _maxBet, value);
    }

    private string _pitLocation = string.Empty;
    public string PitLocation
    {
        get => _pitLocation;
        set => SetProperty(ref _pitLocation, value);
    }

    private bool _isHighLimit;
    public bool IsHighLimit
    {
        get => _isHighLimit;
        set => SetProperty(ref _isHighLimit, value);
    }

    private int _requiredDealerCount = 1;
    public int RequiredDealerCount
    {
        get => _requiredDealerCount;
        set => SetProperty(ref _requiredDealerCount, value);
    }

    private int _pushIntervalMinutes = 20;
    public int PushIntervalMinutes
    {
        get => _pushIntervalMinutes;
        set => SetProperty(ref _pushIntervalMinutes, value);
    }

    public Array GameTypes => Enum.GetValues(typeof(GameType));

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _validationError = string.Empty;
    public string ValidationError
    {
        get => _validationError;
        set => SetProperty(ref _validationError, value);
    }

    #endregion

    #region Commands

    public ICommand AddTableCommand { get; }
    public ICommand EditTableCommand { get; }
    public ICommand DeleteTableCommand { get; }
    public ICommand SaveTableCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand OpenTableCommand { get; }
    public ICommand CloseTableCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadTablesAsync()
    {
        try
        {
            IsLoading = true;
            var tables = await _unitOfWork.Tables.GetAllAsync();
            var tableViewModels = new ObservableCollection<TableViewModel>();

            foreach (var table in tables)
            {
                tableViewModels.Add(new TableViewModel
                {
                    Id = table.Id,
                    TableNumber = table.TableNumber,
                    GameType = table.GameType.ToString(),
                    Status = table.Status.ToString(),
                    MinBet = table.MinBet,
                    MaxBet = table.MaxBet,
                    PitLocation = table.Pit
                });
            }

            Tables = tableViewModels;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tables: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AddTable()
    {
        IsNewTable = true;
        IsEditing = true;
        TableNumber = string.Empty;
        GameType = GameType.Blackjack;
        MinBet = 5;
        MaxBet = 500;
        PitLocation = string.Empty;
        IsHighLimit = false;
        RequiredDealerCount = 1;
        PushIntervalMinutes = 20;
    }

    private async void EditTable()
    {
        if (SelectedTable == null) return;

        try
        {
            IsLoading = true;
            ValidationError = string.Empty;

            // Check if table has active assignments
            var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable.Id);
            if (table != null)
            {
                var hasActiveAssignments = table.CurrentAssignments.Any(a => a.IsActive) || 
                                          table.NextAssignments.Any(a => a.IsActive);

                if (hasActiveAssignments)
                {
                    var result = MessageBox.Show(
                        $"Table '{table.TableNumber}' has active dealer assignments.\n\n" +
                        "Editing this table may affect current rotations.\n\n" +
                        "Do you want to continue?",
                        "Active Assignments Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        IsLoading = false;
                        return;
                    }
                }

                IsNewTable = false;
                IsEditing = true;
                TableNumber = SelectedTable.TableNumber;
                GameType = Enum.Parse<GameType>(SelectedTable.GameType);
                MinBet = SelectedTable.MinBet;
                MaxBet = SelectedTable.MaxBet;
                PitLocation = SelectedTable.PitLocation;
                
                // Load full table to get additional properties
                await LoadTableDetailsAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTableDetailsAsync()
    {
        if (SelectedTable == null) return;
        
        try
        {
            var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable.Id);
            if (table != null)
            {
                IsHighLimit = table.IsHighLimit;
                RequiredDealerCount = table.RequiredDealerCount;
                PushIntervalMinutes = table.PushIntervalMinutes;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading table details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SaveTableAsync()
    {
        try
        {
            IsSaving = true;
            ValidationError = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(TableNumber))
            {
                ValidationError = "Table number is required.";
                MessageBox.Show(ValidationError, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsSaving = false;
                return;
            }

            // Validate numeric inputs
            if (MinBet < 0)
            {
                ValidationError = "Minimum bet must be 0 or greater.";
                MessageBox.Show(ValidationError, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsSaving = false;
                return;
            }

            if (MaxBet < 0)
            {
                ValidationError = "Maximum bet must be 0 or greater.";
                MessageBox.Show(ValidationError, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsSaving = false;
                return;
            }

            if (MaxBet < MinBet)
            {
                ValidationError = "Maximum bet must be greater than or equal to minimum bet.";
                MessageBox.Show(ValidationError, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsSaving = false;
                return;
            }

            if (RequiredDealerCount < 1)
            {
                ValidationError = "Required dealer count must be at least 1.";
                MessageBox.Show(ValidationError, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsSaving = false;
                return;
            }

            if (PushIntervalMinutes < 1)
            {
                ValidationError = "Push interval must be at least 1 minute.";
                MessageBox.Show(ValidationError, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsSaving = false;
                return;
            }

            var employeeId = SessionManager.CurrentEmployee?.Id ?? 0;

            if (IsNewTable)
            {
                // Check if table number already exists
                var existing = await _unitOfWork.Tables.GetAllAsync();
                if (existing.Any(t => t.TableNumber.Equals(TableNumber, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A table with this number already exists.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newTable = new Table
                {
                    TableNumber = TableNumber,
                    GameType = GameType,
                    Status = TableStatus.Closed,
                    MinBet = MinBet,
                    MaxBet = MaxBet,
                    Pit = PitLocation,
                    IsHighLimit = IsHighLimit,
                    RequiredDealerCount = RequiredDealerCount,
                    PushIntervalMinutes = PushIntervalMinutes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Tables.AddAsync(newTable);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    employeeId,
                    ActionType.SettingsChanged,
                    $"Created table {TableNumber} ({GameType})"
                );

                MessageBox.Show("Table created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable!.Id);
                if (table == null)
                {
                    MessageBox.Show("Table not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                table.TableNumber = TableNumber;
                table.GameType = GameType;
                table.MinBet = MinBet;
                table.MaxBet = MaxBet;
                table.Pit = PitLocation;
                table.IsHighLimit = IsHighLimit;
                table.RequiredDealerCount = RequiredDealerCount;
                table.PushIntervalMinutes = PushIntervalMinutes;
                table.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Tables.UpdateAsync(table);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    employeeId,
                    ActionType.SettingsChanged,
                    $"Updated table {TableNumber} ({GameType})"
                );

                MessageBox.Show("Table updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            IsEditing = false;
            ValidationError = string.Empty;
            await LoadTablesAsync();
        }
        catch (Exception ex)
        {
            ValidationError = $"Error saving table: {ex.Message}";
            MessageBox.Show(ValidationError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        IsNewTable = false;
        TableNumber = string.Empty;
        GameType = GameType.Blackjack;
        MinBet = 0;
        MaxBet = 0;
        PitLocation = string.Empty;
        IsHighLimit = false;
        RequiredDealerCount = 1;
        PushIntervalMinutes = 20;
    }

    private async void DeleteTable()
    {
        if (SelectedTable == null) return;

        try
        {
            IsLoading = true;

            // Check if table has active assignments
            var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable.Id);
            if (table == null)
            {
                MessageBox.Show("Table not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var hasActiveAssignments = table.CurrentAssignments.Any(a => a.IsActive) || 
                                      table.NextAssignments.Any(a => a.IsActive);

            if (hasActiveAssignments)
            {
                MessageBox.Show(
                    $"Cannot delete table '{table.TableNumber}' because it has active dealer assignments.\n\n" +
                    "Please remove all assignments or close the table first.",
                    "Cannot Delete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (table.Status == TableStatus.Open)
            {
                var result = MessageBox.Show(
                    $"Table '{table.TableNumber}' is currently open.\n\n" +
                    "Are you sure you want to delete it?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }
            else
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete table '{table.TableNumber}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }

            await _unitOfWork.Tables.DeleteAsync(table);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActionAsync(
                SessionManager.CurrentEmployee?.Id ?? 0,
                ActionType.SettingsChanged,
                $"Deleted table {table.TableNumber}"
            );

            await LoadTablesAsync();
            MessageBox.Show("Table deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OpenTable()
    {
        if (SelectedTable == null) return;

        try
        {
            IsLoading = true;
            var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable.Id);
            if (table != null)
            {
                if (table.Status == TableStatus.Open)
                {
                    MessageBox.Show("Table is already open.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                table.Status = TableStatus.Open;
                await _unitOfWork.Tables.UpdateAsync(table);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    SessionManager.CurrentEmployee?.Id ?? 0,
                    ActionType.TableOpened,
                    $"Opened table {table.TableNumber}"
                );

                await LoadTablesAsync();
                MessageBox.Show("Table opened successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void CloseTable()
    {
        if (SelectedTable == null) return;

        try
        {
            IsLoading = true;
            var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable.Id);
            if (table != null)
            {
                if (table.Status == TableStatus.Closed)
                {
                    MessageBox.Show("Table is already closed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var hasActiveAssignments = table.CurrentAssignments.Any(a => a.IsActive);

                if (hasActiveAssignments)
                {
                    var result = MessageBox.Show(
                        $"Table '{table.TableNumber}' has active dealer assignments.\n\n" +
                        "Closing this table will remove all active assignments.\n\n" +
                        "Do you want to continue?",
                        "Active Assignments Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                    {
                        IsLoading = false;
                        return;
                    }
                }

                table.Status = TableStatus.Closed;
                await _unitOfWork.Tables.UpdateAsync(table);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    SessionManager.CurrentEmployee?.Id ?? 0,
                    ActionType.TableClosed,
                    $"Closed table {table.TableNumber}"
                );

                await LoadTablesAsync();
                MessageBox.Show("Table closed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error closing table: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}

/// <summary>
/// ViewModel for displaying table in grid
/// </summary>
public class TableViewModel
{
    public int Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal MinBet { get; set; }
    public decimal MaxBet { get; set; }
    public string PitLocation { get; set; } = string.Empty;
}
