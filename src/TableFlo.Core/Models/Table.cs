using TableFlo.Core.Enums;

namespace TableFlo.Core.Models;

/// <summary>
/// Represents a casino table/game
/// </summary>
public class Table
{
    public int Id { get; set; }
    
    /// <summary>
    /// Table identifier (e.g., BJ302, R12, CR4)
    /// </summary>
    public string TableNumber { get; set; } = string.Empty;
    
    public GameType GameType { get; set; }
    
    public TableStatus Status { get; set; } = TableStatus.Closed;
    
    /// <summary>
    /// Minimum bet
    /// </summary>
    public decimal MinBet { get; set; }
    
    /// <summary>
    /// Maximum bet
    /// </summary>
    public decimal MaxBet { get; set; }
    
    /// <summary>
    /// Is this a high-limit table
    /// </summary>
    public bool IsHighLimit { get; set; }
    
    /// <summary>
    /// Pit/section location
    /// </summary>
    public string Pit { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of dealers required (e.g., Craps needs 3-4)
    /// </summary>
    public int RequiredDealerCount { get; set; } = 1;
    
    /// <summary>
    /// Specific crew requirements for games like Craps
    /// </summary>
    public List<CrapsRole> RequiredCrapsRoles { get; set; } = new();
    
    /// <summary>
    /// Standard push interval in minutes
    /// </summary>
    public int PushIntervalMinutes { get; set; } = 20;
    
    /// <summary>
    /// Currently assigned dealer(s)
    /// </summary>
    public List<Assignment> CurrentAssignments { get; set; } = new();
    
    /// <summary>
    /// Next scheduled dealer(s)
    /// </summary>
    public List<Assignment> NextAssignments { get; set; } = new();
    
    /// <summary>
    /// Is this assignment locked (manual override)
    /// </summary>
    public bool IsLocked { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

