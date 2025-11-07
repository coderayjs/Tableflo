using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Data.Interfaces;
using TableFlo.Services.Interfaces;

namespace TableFlo.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TableFloDbContext _context;

    public AuthenticationService(IUnitOfWork unitOfWork, TableFloDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Employee?> AuthenticateAsync(string employeeNumber, string password)
    {
        var employees = await _context.Employees
            .Where(e => e.EmployeeNumber == employeeNumber && e.IsActive)
            .ToListAsync();
        
        var employee = employees.FirstOrDefault();
        
        if (employee == null)
            return null;

        if (!VerifyPassword(password, employee.PasswordHash))
            return null;

        // Update last login
        employee.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return employee;
    }

    public async Task<Employee> RegisterEmployeeAsync(string employeeNumber, string firstName, string lastName, string password, string role)
    {
        var employee = new Employee
        {
            EmployeeNumber = employeeNumber,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = HashPassword(password),
            Role = role,
            HireDate = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        return employee;
    }

    public async Task<bool> ChangePasswordAsync(int employeeId, string oldPassword, string newPassword)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
        
        if (employee == null)
            return false;

        if (!VerifyPassword(oldPassword, employee.PasswordHash))
            return false;

        employee.PasswordHash = HashPassword(newPassword);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var hash = HashPassword(password);
        return hash == passwordHash;
    }
}

