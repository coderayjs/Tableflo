using TableFlo.Core.Models;

namespace TableFlo.Services.Interfaces;

/// <summary>
/// AI-powered scheduling service for dealer rotations
/// </summary>
public interface ISchedulingService
{
    /// <summary>
    /// Generate optimized schedule for all open tables
    /// </summary>
    Task<ScheduleResult> GenerateScheduleAsync(DateTime startTime, int pushCount = 10);
    
    /// <summary>
    /// Get AI recommendation for next dealer at a specific table
    /// </summary>
    Task<Dealer?> RecommendNextDealerAsync(int tableId);
    
    /// <summary>
    /// Handle dealer call-in and automatically reassign their tables
    /// </summary>
    Task<ScheduleResult> HandleCallInAsync(int dealerId);
    
    /// <summary>
    /// Check if dealer needs break based on compliance rules
    /// </summary>
    Task<bool> NeedsBreakAsync(int dealerId);
    
    /// <summary>
    /// Get dealers due for breaks
    /// </summary>
    Task<IEnumerable<Dealer>> GetDealersDueForBreakAsync();
    
    /// <summary>
    /// Calculate fairness score for dealer rotation
    /// </summary>
    Task<double> CalculateFairnessScoreAsync(int dealerId);
}

/// <summary>
/// Result of schedule generation
/// </summary>
public class ScheduleResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Assignment> Assignments { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

