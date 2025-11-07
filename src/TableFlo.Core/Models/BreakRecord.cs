namespace TableFlo.Core.Models;

/// <summary>
/// Represents a dealer break or meal record
/// </summary>
public class BreakRecord
{
    public int Id { get; set; }
    
    public int DealerId { get; set; }
    
    public Dealer? Dealer { get; set; }
    
    /// <summary>
    /// Break or Meal
    /// </summary>
    public string BreakType { get; set; } = "Break";
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Expected duration in minutes
    /// </summary>
    public int ExpectedDurationMinutes { get; set; }
    
    /// <summary>
    /// Was this break compliant with labor rules
    /// </summary>
    public bool IsCompliant { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

