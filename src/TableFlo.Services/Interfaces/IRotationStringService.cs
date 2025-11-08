using TableFlo.Core.Models;

namespace TableFlo.Services.Interfaces;

/// <summary>
/// Service for managing rotation strings/groups
/// </summary>
public interface IRotationStringService
{
    /// <summary>
    /// Get all rotation strings
    /// </summary>
    Task<IEnumerable<RotationString>> GetAllStringsAsync();
    
    /// <summary>
    /// Get rotation string by ID
    /// </summary>
    Task<RotationString?> GetStringByIdAsync(int stringId);
    
    /// <summary>
    /// Create new rotation string
    /// </summary>
    Task<RotationString> CreateStringAsync(string name, string description);
    
    /// <summary>
    /// Add dealer to rotation string
    /// </summary>
    Task<bool> AddDealerToStringAsync(int dealerId, int stringId, int rotationOrder);
    
    /// <summary>
    /// Remove dealer from rotation string
    /// </summary>
    Task<bool> RemoveDealerFromStringAsync(int dealerId, int stringId);
    
    /// <summary>
    /// Assign all dealers from a string to their next tables
    /// </summary>
    Task<bool> ExecuteStringRotationAsync(int stringId, int employeeId);
    
    /// <summary>
    /// Get dealers in a rotation string
    /// </summary>
    Task<IEnumerable<Dealer>> GetDealersInStringAsync(int stringId);
}

