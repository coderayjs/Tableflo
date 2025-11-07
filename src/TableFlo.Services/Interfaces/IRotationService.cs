using TableFlo.Core.Models;

namespace TableFlo.Services.Interfaces;

/// <summary>
/// Service for managing dealer rotations and table assignments
/// </summary>
public interface IRotationService
{
    /// <summary>
    /// Execute push - swap current and next dealers
    /// </summary>
    Task<bool> ExecutePushAsync(int tableId, int employeeId);
    
    /// <summary>
    /// Assign dealer to table
    /// </summary>
    Task<Assignment> AssignDealerAsync(int dealerId, int tableId, bool isCurrent, int employeeId);
    
    /// <summary>
    /// Remove dealer from table
    /// </summary>
    Task<bool> RemoveDealerAsync(int assignmentId, int employeeId);
    
    /// <summary>
    /// Send dealer to break
    /// </summary>
    Task<BreakRecord> SendToBreakAsync(int dealerId, string breakType, int durationMinutes, int employeeId);
    
    /// <summary>
    /// Return dealer from break
    /// </summary>
    Task<bool> ReturnFromBreakAsync(int dealerId, int employeeId);
    
    /// <summary>
    /// Get current assignment for a dealer
    /// </summary>
    Task<Assignment?> GetCurrentAssignmentAsync(int dealerId);
    
    /// <summary>
    /// Get all dealers currently on break
    /// </summary>
    Task<IEnumerable<Dealer>> GetDealersOnBreakAsync();
}

