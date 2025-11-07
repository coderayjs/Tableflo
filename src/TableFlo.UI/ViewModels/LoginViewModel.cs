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

    // Event to notify when login succeeds
    public event EventHandler<string>? LoginSucceeded;

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

            // Fire event to notify success (UI will handle the transition)
            LoginSucceeded?.Invoke(this, employee.FullName);
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

