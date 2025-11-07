using TableFlo.Core.Models;

namespace TableFlo.Services.Interfaces;

/// <summary>
/// Authentication service for employee login
/// </summary>
public interface IAuthenticationService
{
    Task<Employee?> AuthenticateAsync(string employeeNumber, string password);
    Task<Employee> RegisterEmployeeAsync(string employeeNumber, string firstName, string lastName, string password, string role);
    Task<bool> ChangePasswordAsync(int employeeId, string oldPassword, string newPassword);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

