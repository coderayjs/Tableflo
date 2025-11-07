using System.Collections.ObjectModel;
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

    #endregion

    #region Commands

    public ICommand AddTableCommand { get; }
    public ICommand EditTableCommand { get; }
    public ICommand DeleteTableCommand { get; }
    public ICommand OpenTableCommand { get; }
    public ICommand CloseTableCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadTablesAsync()
    {
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

    private void AddTable()
    {
        MessageBox.Show("Add Table functionality - Coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void EditTable()
    {
        MessageBox.Show("Edit Table functionality - Coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DeleteTable()
    {
        MessageBox.Show("Delete Table functionality - Coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OpenTable()
    {
        if (SelectedTable == null) return;

        try
        {
            var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable.Id);
            if (table != null)
            {
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
    }

    private async void CloseTable()
    {
        if (SelectedTable == null) return;

        try
        {
            var table = await _unitOfWork.Tables.GetByIdAsync(SelectedTable.Id);
            if (table != null)
            {
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
