using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Services.Interfaces;

namespace TableFlo.UI.ViewModels;

/// <summary>
/// ViewModel for managing rotation strings
/// </summary>
public class StringManagementViewModel : ViewModelBase
{
    private readonly IRotationStringService _stringService;
    private readonly TableFloDbContext _context;
    private readonly IAuditService _auditService;

    public StringManagementViewModel(
        IRotationStringService stringService,
        TableFloDbContext context,
        IAuditService auditService)
    {
        _stringService = stringService;
        _context = context;
        _auditService = auditService;

        CreateStringCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CreateString);
        EditStringCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(EditString, () => SelectedString != null);
        DeleteStringCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(DeleteStringAsync, () => SelectedString != null);
        AddDealerToStringCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(AddDealerToStringAsync, () => SelectedString != null && SelectedDealer != null);
        RemoveDealerFromStringCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(RemoveDealerFromStringAsync, () => SelectedStringDealer != null);
        SaveStringCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(SaveStringAsync);
        CancelEditCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CancelEdit);
        RefreshCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadDataAsync);

        _ = LoadDataAsync();
    }

    #region Properties

    private ObservableCollection<RotationStringViewModel> _strings = new();
    public ObservableCollection<RotationStringViewModel> Strings
    {
        get => _strings;
        set => SetProperty(ref _strings, value);
    }

    private RotationStringViewModel? _selectedString;
    public RotationStringViewModel? SelectedString
    {
        get => _selectedString;
        set
        {
            SetProperty(ref _selectedString, value);
            (EditStringCommand as CommunityToolkit.Mvvm.Input.RelayCommand)?.NotifyCanExecuteChanged();
            (DeleteStringCommand as CommunityToolkit.Mvvm.Input.AsyncRelayCommand)?.NotifyCanExecuteChanged();
            (AddDealerToStringCommand as CommunityToolkit.Mvvm.Input.AsyncRelayCommand)?.NotifyCanExecuteChanged();
            if (value != null)
            {
                _ = LoadStringDealersAsync(value.Id);
            }
        }
    }

    private ObservableCollection<DealerSelectionViewModel> _allDealers = new();
    public ObservableCollection<DealerSelectionViewModel> AllDealers
    {
        get => _allDealers;
        set => SetProperty(ref _allDealers, value);
    }

    private DealerSelectionViewModel? _selectedDealer;
    public DealerSelectionViewModel? SelectedDealer
    {
        get => _selectedDealer;
        set
        {
            SetProperty(ref _selectedDealer, value);
            (AddDealerToStringCommand as CommunityToolkit.Mvvm.Input.AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }
    }

    private ObservableCollection<StringDealerViewModel> _stringDealers = new();
    public ObservableCollection<StringDealerViewModel> StringDealers
    {
        get => _stringDealers;
        set => SetProperty(ref _stringDealers, value);
    }

    private StringDealerViewModel? _selectedStringDealer;
    public StringDealerViewModel? SelectedStringDealer
    {
        get => _selectedStringDealer;
        set
        {
            SetProperty(ref _selectedStringDealer, value);
            (RemoveDealerFromStringCommand as CommunityToolkit.Mvvm.Input.AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    private bool _isNewString;
    public bool IsNewString
    {
        get => _isNewString;
        set => SetProperty(ref _isNewString, value);
    }

    private string _stringName = string.Empty;
    public string StringName
    {
        get => _stringName;
        set => SetProperty(ref _stringName, value);
    }

    private string _stringDescription = string.Empty;
    public string StringDescription
    {
        get => _stringDescription;
        set => SetProperty(ref _stringDescription, value);
    }

    #endregion

    #region Commands

    public ICommand CreateStringCommand { get; }
    public ICommand EditStringCommand { get; }
    public ICommand DeleteStringCommand { get; }
    public ICommand AddDealerToStringCommand { get; }
    public ICommand RemoveDealerFromStringCommand { get; }
    public ICommand SaveStringCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadDataAsync()
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

        Strings = stringViewModels;

        // Load all dealers
        var dealers = await _context.Dealers
            .Include(d => d.Employee)
            .ToListAsync();

        var dealerViewModels = new ObservableCollection<DealerSelectionViewModel>();
        foreach (var dealer in dealers)
        {
            if (dealer.Employee != null)
            {
                dealerViewModels.Add(new DealerSelectionViewModel
                {
                    Id = dealer.Id,
                    Name = dealer.Employee.FullName,
                    EmployeeNumber = dealer.Employee.EmployeeNumber
                });
            }
        }

        AllDealers = dealerViewModels;
    }

    private async Task LoadStringDealersAsync(int stringId)
    {
        var dealers = await _stringService.GetDealersInStringAsync(stringId);
        var stringDealers = await _context.DealerStringAssignments
            .Include(dsa => dsa.Dealer)
                .ThenInclude(d => d!.Employee)
            .Where(dsa => dsa.RotationStringId == stringId && dsa.IsActive)
            .OrderBy(dsa => dsa.RotationOrder)
            .ToListAsync();

        var viewModels = new ObservableCollection<StringDealerViewModel>();
        foreach (var assignment in stringDealers)
        {
            if (assignment.Dealer?.Employee != null)
            {
                viewModels.Add(new StringDealerViewModel
                {
                    AssignmentId = assignment.Id,
                    DealerId = assignment.DealerId,
                    DealerName = assignment.Dealer.Employee.FullName,
                    RotationOrder = assignment.RotationOrder
                });
            }
        }

        StringDealers = viewModels;
    }

    private void CreateString()
    {
        IsNewString = true;
        IsEditing = true;
        StringName = string.Empty;
        StringDescription = string.Empty;
    }

    private void EditString()
    {
        if (SelectedString == null) return;

        IsNewString = false;
        IsEditing = true;
        StringName = SelectedString.Name;
        StringDescription = SelectedString.Description;
    }

    private async Task DeleteStringAsync()
    {
        if (SelectedString == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete '{SelectedString.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            var rotationString = await _stringService.GetStringByIdAsync(SelectedString.Id);
            if (rotationString != null)
            {
                rotationString.IsActive = false;
                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    SessionManager.CurrentEmployee?.Id ?? 0,
                    Core.Enums.ActionType.SettingsChanged,
                    $"Deleted rotation string: {SelectedString.Name}"
                );

                await LoadDataAsync();
                MessageBox.Show("String deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting string: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SaveStringAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(StringName))
            {
                MessageBox.Show("String name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsNewString)
            {
                await _stringService.CreateStringAsync(StringName, StringDescription);
                MessageBox.Show("String created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var rotationString = await _stringService.GetStringByIdAsync(SelectedString!.Id);
                if (rotationString != null)
                {
                    rotationString.Name = StringName;
                    rotationString.Description = StringDescription;
                    await _context.SaveChangesAsync();

                    await _auditService.LogActionAsync(
                        SessionManager.CurrentEmployee?.Id ?? 0,
                        Core.Enums.ActionType.SettingsChanged,
                        $"Updated rotation string: {StringName}"
                    );

                    MessageBox.Show("String updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            IsEditing = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving string: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        StringName = string.Empty;
        StringDescription = string.Empty;
    }

    private async Task AddDealerToStringAsync()
    {
        if (SelectedString == null || SelectedDealer == null) return;

        try
        {
            var currentOrder = StringDealers.Count > 0 ? StringDealers.Max(sd => sd.RotationOrder) + 1 : 1;
            var success = await _stringService.AddDealerToStringAsync(SelectedDealer.Id, SelectedString.Id, currentOrder);

            if (success)
            {
                await LoadStringDealersAsync(SelectedString.Id);
                await LoadDataAsync();
                MessageBox.Show("Dealer added to string successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Dealer is already in this string.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding dealer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RemoveDealerFromStringAsync()
    {
        if (SelectedStringDealer == null || SelectedString == null) return;

        try
        {
            var success = await _stringService.RemoveDealerFromStringAsync(SelectedStringDealer.DealerId, SelectedString.Id);

            if (success)
            {
                await LoadStringDealersAsync(SelectedString.Id);
                await LoadDataAsync();
                MessageBox.Show("Dealer removed from string successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error removing dealer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}

public class DealerSelectionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
}

public class StringDealerViewModel
{
    public int AssignmentId { get; set; }
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public int RotationOrder { get; set; }
}

