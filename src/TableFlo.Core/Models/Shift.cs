namespace TableFlo.Core.Models;

/// <summary>
/// Represents a work shift
/// </summary>
public class Shift
{
    public int Id { get; set; }
    
    public string ShiftName { get; set; } = string.Empty;
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
    
    /// <summary>
    /// Minimum dealers required for this shift
    /// </summary>
    public int MinimumDealers { get; set; }
    
    /// <summary>
    /// Break interval in minutes
    /// </summary>
    public int BreakIntervalMinutes { get; set; } = 120;
    
    /// <summary>
    /// Meal deadline (hours into shift)
    /// </summary>
    public int MealDeadlineHours { get; set; } = 5;
    
    public bool IsActive { get; set; } = true;
}

