using TableFlo.Core.Enums;

namespace TableFlo.Core.Models;

/// <summary>
/// Represents a casino dealer with skills, certifications, and availability
/// </summary>
public class Dealer
{
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    
    public Employee? Employee { get; set; }
    
    /// <summary>
    /// Current status of the dealer
    /// </summary>
    public DealerStatus Status { get; set; } = DealerStatus.Available;
    
    /// <summary>
    /// Seniority level (used for rotation priority)
    /// </summary>
    public int SeniorityLevel { get; set; }
    
    /// <summary>
    /// Shift start time
    /// </summary>
    public TimeSpan ShiftStart { get; set; }
    
    /// <summary>
    /// Shift end time
    /// </summary>
    public TimeSpan ShiftEnd { get; set; }
    
    /// <summary>
    /// Last break time
    /// </summary>
    public DateTime? LastBreakTime { get; set; }
    
    /// <summary>
    /// Last meal time
    /// </summary>
    public DateTime? LastMealTime { get; set; }
    
    /// <summary>
    /// Preferred pit/section
    /// </summary>
    public string? PreferredPit { get; set; }
    
    /// <summary>
    /// Game certifications and proficiency levels
    /// </summary>
    public List<DealerCertification> Certifications { get; set; } = new();
    
    /// <summary>
    /// Assignment history for tracking rotation diversity
    /// </summary>
    public List<Assignment> AssignmentHistory { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

