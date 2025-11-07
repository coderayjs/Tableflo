using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TableFlo.Core.Enums;
using TableFlo.Services.Interfaces;
using TableFlo.UI.Views;

namespace TableFlo.UI.ViewModels;

/// <summary>
/// ViewModel for Login screen
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;

    public LoginViewModel(IAuthenticationService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => !string.IsNullOrWhiteSpace(EmployeeNumber));
    }

    private string _employeeNumber = string.Empty;
    public string EmployeeNumber
    {
        get => _employeeNumber;
        set
        {
            SetProperty(ref _employeeNumber, value);
            LoginCommand.NotifyCanExecuteChanged();
        }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IAsyncRelayCommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // For demo purposes, using a simple password. In production, this would come from PasswordBox
            var employee = await _authService.AuthenticateAsync(EmployeeNumber, "admin123");

            if (employee == null)
            {
                ErrorMessage = "Invalid employee number or password";
                IsLoading = false;
                return;
            }

            // Log the login
            await _auditService.LogActionAsync(
                employee.Id,
                ActionType.Login,
                $"User {employee.FullName} logged in"
            );

            // Store current user session
            SessionManager.CurrentEmployee = employee;

            // Open main window
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Close login window
            Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)?.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

