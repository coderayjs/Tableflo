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
public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;

    public LoginViewModel(IAuthenticationService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    private string _employeeNumber = string.Empty;
    public string EmployeeNumber
    {
        get => _employeeNumber;
        set => SetProperty(ref _employeeNumber, value);
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

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var employee = await _authService.AuthenticateAsync(EmployeeNumber, Password);

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
            try
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // Close login window
                var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                loginWindow?.Close();
            }
            catch (Exception mainWindowEx)
            {
                ErrorMessage = $"Failed to open main window: {mainWindowEx.Message}";
                IsLoading = false;
                SessionManager.Logout();
                return;
            }
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

