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
/// Dealer Management ViewModel - Full CRUD operations
/// </summary>
public class DealerManagementViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;

    public DealerManagementViewModel(
        IUnitOfWork unitOfWork,
        IAuthenticationService authService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _auditService = auditService;

        AddDealerCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(AddDealer);
        EditDealerCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(EditDealer, () => SelectedDealer != null);
        DeleteDealerCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(DeleteDealer, () => SelectedDealer != null);
        SaveDealerCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(SaveDealerAsync);
        CancelEditCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CancelEdit);
        RefreshCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadDealersAsync);

        _ = LoadDealersAsync();
    }

    #region Properties

    private ObservableCollection<DealerViewModel> _dealers = new();
    public ObservableCollection<DealerViewModel> Dealers
    {
        get => _dealers;
        set => SetProperty(ref _dealers, value);
    }

    private DealerViewModel? _selectedDealer;
    public DealerViewModel? SelectedDealer
    {
        get => _selectedDealer;
        set
        {
            SetProperty(ref _selectedDealer, value);
            (EditDealerCommand as CommunityToolkit.Mvvm.Input.RelayCommand)?.NotifyCanExecuteChanged();
            (DeleteDealerCommand as CommunityToolkit.Mvvm.Input.RelayCommand)?.NotifyCanExecuteChanged();
        }
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    private bool _isNewDealer;
    public bool IsNewDealer
    {
        get => _isNewDealer;
        set => SetProperty(ref _isNewDealer, value);
    }

    // Edit Form Properties
    private string _employeeNumber = string.Empty;
    public string EmployeeNumber
    {
        get => _employeeNumber;
        set => SetProperty(ref _employeeNumber, value);
    }

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private int _seniorityLevel = 1;
    public int SeniorityLevel
    {
        get => _seniorityLevel;
        set => SetProperty(ref _seniorityLevel, value);
    }

    private string _preferredPit = "Main";
    public string PreferredPit
    {
        get => _preferredPit;
        set => SetProperty(ref _preferredPit, value);
    }

    private TimeSpan _shiftStart = new TimeSpan(6, 0, 0);
    public TimeSpan ShiftStart
    {
        get => _shiftStart;
        set => SetProperty(ref _shiftStart, value);
    }

    private TimeSpan _shiftEnd = new TimeSpan(14, 0, 0);
    public TimeSpan ShiftEnd
    {
        get => _shiftEnd;
        set => SetProperty(ref _shiftEnd, value);
    }

    // Certifications
    private bool _hasBlackjack;
    public bool HasBlackjack
    {
        get => _hasBlackjack;
        set => SetProperty(ref _hasBlackjack, value);
    }

    private bool _hasRoulette;
    public bool HasRoulette
    {
        get => _hasRoulette;
        set => SetProperty(ref _hasRoulette, value);
    }

    private bool _hasCraps;
    public bool HasCraps
    {
        get => _hasCraps;
        set => SetProperty(ref _hasCraps, value);
    }

    private bool _hasPaiGow;
    public bool HasPaiGow
    {
        get => _hasPaiGow;
        set => SetProperty(ref _hasPaiGow, value);
    }

    private bool _hasBaccarat;
    public bool HasBaccarat
    {
        get => _hasBaccarat;
        set => SetProperty(ref _hasBaccarat, value);
    }

    #endregion

    #region Commands

    public ICommand AddDealerCommand { get; }
    public ICommand EditDealerCommand { get; }
    public ICommand DeleteDealerCommand { get; }
    public ICommand SaveDealerCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadDealersAsync()
    {
        var dealers = await _unitOfWork.Dealers.GetAllAsync();
        var dealerViewModels = new ObservableCollection<DealerViewModel>();

        foreach (var dealer in dealers)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(dealer.EmployeeId);
            if (employee != null)
            {
                dealerViewModels.Add(new DealerViewModel
                {
                    Id = dealer.Id,
                    EmployeeId = dealer.EmployeeId,
                    EmployeeNumber = employee.EmployeeNumber,
                    FullName = employee.FullName,
                    Status = dealer.Status.ToString(),
                    SeniorityLevel = dealer.SeniorityLevel,
                    PreferredPit = dealer.PreferredPit,
                    ShiftStart = dealer.ShiftStart.ToString(@"hh\:mm"),
                    ShiftEnd = dealer.ShiftEnd.ToString(@"hh\:mm"),
                    Certifications = string.Join(", ", dealer.Certifications.Select(c => c.GameType.ToString()))
                });
            }
        }

        Dealers = dealerViewModels;
    }

    private void AddDealer()
    {
        IsNewDealer = true;
        IsEditing = true;
        ClearForm();
    }

    private void EditDealer()
    {
        if (SelectedDealer == null) return;

        IsNewDealer = false;
        IsEditing = true;

        // Load dealer data into form
        EmployeeNumber = SelectedDealer.EmployeeNumber;
        FirstName = SelectedDealer.FullName.Split(' ')[0];
        LastName = SelectedDealer.FullName.Split(' ').Length > 1 
            ? SelectedDealer.FullName.Split(' ')[1] 
            : "";
        SeniorityLevel = SelectedDealer.SeniorityLevel;
        PreferredPit = SelectedDealer.PreferredPit;

        // Parse shift times
        if (TimeSpan.TryParse(SelectedDealer.ShiftStart, out var start))
            ShiftStart = start;
        if (TimeSpan.TryParse(SelectedDealer.ShiftEnd, out var end))
            ShiftEnd = end;

        // Load certifications
        var certs = SelectedDealer.Certifications.Split(',').Select(c => c.Trim()).ToList();
        HasBlackjack = certs.Contains("Blackjack");
        HasRoulette = certs.Contains("Roulette");
        HasCraps = certs.Contains("Craps");
        HasPaiGow = certs.Contains("PaiGow");
        HasBaccarat = certs.Contains("Baccarat");
    }

    private async void DeleteDealer()
    {
        if (SelectedDealer == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete {SelectedDealer.FullName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var dealer = await _unitOfWork.Dealers.GetByIdAsync(SelectedDealer.Id);
                if (dealer != null)
                {
                    await _unitOfWork.Dealers.DeleteAsync(dealer);
                    await _unitOfWork.SaveChangesAsync();

                    // Audit log
                    await _auditService.LogActionAsync(
                        SessionManager.CurrentEmployee?.Id ?? 0,
                        ActionType.DealerRemoved,
                        $"Deleted dealer: {SelectedDealer.FullName}"
                    );

                    await LoadDealersAsync();
                    MessageBox.Show("Dealer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting dealer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async Task SaveDealerAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EmployeeNumber) || 
                string.IsNullOrWhiteSpace(FirstName) || 
                string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsNewDealer)
            {
                // Create new employee and dealer
                if (string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("Password is required for new dealers.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var employee = await _authService.RegisterEmployeeAsync(
                    EmployeeNumber, FirstName, LastName, Password, "Dealer");

                var dealer = new Dealer
                {
                    EmployeeId = employee.Id,
                    Status = DealerStatus.Available,
                    SeniorityLevel = SeniorityLevel,
                    ShiftStart = ShiftStart,
                    ShiftEnd = ShiftEnd,
                    PreferredPit = PreferredPit
                };

                // Add certifications
                if (HasBlackjack) dealer.Certifications.Add(new DealerCertification 
                { 
                    GameType = GameType.Blackjack, 
                    ProficiencyLevel = ProficiencyLevel.Intermediate,
                    CertifiedDate = DateTime.UtcNow,
                    IsActive = true
                });
                if (HasRoulette) dealer.Certifications.Add(new DealerCertification 
                { 
                    GameType = GameType.Roulette, 
                    ProficiencyLevel = ProficiencyLevel.Intermediate,
                    CertifiedDate = DateTime.UtcNow,
                    IsActive = true
                });
                if (HasCraps) dealer.Certifications.Add(new DealerCertification 
                { 
                    GameType = GameType.Craps, 
                    ProficiencyLevel = ProficiencyLevel.Intermediate,
                    CertifiedDate = DateTime.UtcNow,
                    IsActive = true
                });
                if (HasPaiGow) dealer.Certifications.Add(new DealerCertification 
                { 
                    GameType = GameType.PaiGow, 
                    ProficiencyLevel = ProficiencyLevel.Intermediate,
                    CertifiedDate = DateTime.UtcNow,
                    IsActive = true
                });
                if (HasBaccarat) dealer.Certifications.Add(new DealerCertification 
                { 
                    GameType = GameType.Baccarat, 
                    ProficiencyLevel = ProficiencyLevel.Intermediate,
                    CertifiedDate = DateTime.UtcNow,
                    IsActive = true
                });

                await _unitOfWork.Dealers.AddAsync(dealer);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    SessionManager.CurrentEmployee?.Id ?? 0,
                    ActionType.DealerAssigned,
                    $"Added new dealer: {FirstName} {LastName}"
                );

                MessageBox.Show("Dealer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Update existing dealer
                if (SelectedDealer == null) return;

                var dealer = await _unitOfWork.Dealers.GetByIdAsync(SelectedDealer.Id);
                if (dealer != null)
                {
                    dealer.SeniorityLevel = SeniorityLevel;
                    dealer.ShiftStart = ShiftStart;
                    dealer.ShiftEnd = ShiftEnd;
                    dealer.PreferredPit = PreferredPit;

                    // Update certifications
                    dealer.Certifications.Clear();
                    if (HasBlackjack) dealer.Certifications.Add(new DealerCertification 
                    { 
                        DealerId = dealer.Id,
                        GameType = GameType.Blackjack, 
                        ProficiencyLevel = ProficiencyLevel.Intermediate,
                        CertifiedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                    if (HasRoulette) dealer.Certifications.Add(new DealerCertification 
                    { 
                        DealerId = dealer.Id,
                        GameType = GameType.Roulette, 
                        ProficiencyLevel = ProficiencyLevel.Intermediate,
                        CertifiedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                    if (HasCraps) dealer.Certifications.Add(new DealerCertification 
                    { 
                        DealerId = dealer.Id,
                        GameType = GameType.Craps, 
                        ProficiencyLevel = ProficiencyLevel.Intermediate,
                        CertifiedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                    if (HasPaiGow) dealer.Certifications.Add(new DealerCertification 
                    { 
                        DealerId = dealer.Id,
                        GameType = GameType.PaiGow, 
                        ProficiencyLevel = ProficiencyLevel.Intermediate,
                        CertifiedDate = DateTime.UtcNow,
                        IsActive = true
                    });
                    if (HasBaccarat) dealer.Certifications.Add(new DealerCertification 
                    { 
                        DealerId = dealer.Id,
                        GameType = GameType.Baccarat, 
                        ProficiencyLevel = ProficiencyLevel.Intermediate,
                        CertifiedDate = DateTime.UtcNow,
                        IsActive = true
                    });

                    await _unitOfWork.SaveChangesAsync();

                    await _auditService.LogActionAsync(
                        SessionManager.CurrentEmployee?.Id ?? 0,
                        ActionType.DealerUpdated,
                        $"Updated dealer: {FirstName} {LastName}"
                    );

                    MessageBox.Show("Dealer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            IsEditing = false;
            await LoadDealersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving dealer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        ClearForm();
    }

    private void ClearForm()
    {
        EmployeeNumber = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Password = string.Empty;
        SeniorityLevel = 1;
        PreferredPit = "Main";
        ShiftStart = new TimeSpan(6, 0, 0);
        ShiftEnd = new TimeSpan(14, 0, 0);
        HasBlackjack = false;
        HasRoulette = false;
        HasCraps = false;
        HasPaiGow = false;
        HasBaccarat = false;
    }

    #endregion
}

/// <summary>
/// ViewModel for displaying dealer in grid
/// </summary>
public class DealerViewModel
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SeniorityLevel { get; set; }
    public string PreferredPit { get; set; } = string.Empty;
    public string ShiftStart { get; set; } = string.Empty;
    public string ShiftEnd { get; set; } = string.Empty;
    public string Certifications { get; set; } = string.Empty;
}
