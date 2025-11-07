using TableFlo.Core.Enums;

namespace TableFlo.Core.Models;

/// <summary>
/// Audit trail record for all system actions
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    
    /// <summary>
    /// Employee who performed the action
    /// </summary>
    public int EmployeeId { get; set; }
    
    public Employee? Employee { get; set; }
    
    public ActionType ActionType { get; set; }
    
    /// <summary>
    /// Detailed description of the action
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Related entity ID (dealer, table, etc.)
    /// </summary>
    public int? RelatedEntityId { get; set; }
    
    /// <summary>
    /// Related entity type
    /// </summary>
    public string? RelatedEntityType { get; set; }
    
    /// <summary>
    /// Additional JSON data
    /// </summary>
    public string? AdditionalData { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

