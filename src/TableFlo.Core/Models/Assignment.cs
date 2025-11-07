using TableFlo.Core.Enums;

namespace TableFlo.Core.Models;

/// <summary>
/// Represents a dealer assignment to a table
/// </summary>
public class Assignment
{
    public int Id { get; set; }
    
    public int DealerId { get; set; }
    
    public Dealer? Dealer { get; set; }
    
    public int TableId { get; set; }
    
    public Table? Table { get; set; }
    
    /// <summary>
    /// When the dealer started at this table
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// When the dealer finished at this table
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// For Craps assignments - specific role
    /// </summary>
    public CrapsRole CrapsRole { get; set; } = CrapsRole.None;
    
    /// <summary>
    /// Is this the current assignment or next scheduled
    /// </summary>
    public bool IsCurrent { get; set; }
    
    /// <summary>
    /// Was this assignment made by AI or manual override
    /// </summary>
    public bool IsAIGenerated { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

