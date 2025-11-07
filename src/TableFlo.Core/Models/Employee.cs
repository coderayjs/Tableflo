namespace TableFlo.Core.Models;

/// <summary>
/// Represents an employee (supervisor, manager, or dealer) in the system
/// </summary>
public class Employee
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique employee number assigned during hiring
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}";
    
    /// <summary>
    /// Password hash for authentication
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Role: Supervisor, Manager, Dealer, Admin
    /// </summary>
    public string Role { get; set; } = "Dealer";
    
    public bool IsActive { get; set; } = true;
    
    public DateTime HireDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
}

