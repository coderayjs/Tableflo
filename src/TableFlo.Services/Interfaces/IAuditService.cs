using TableFlo.Core.Enums;
using TableFlo.Core.Models;

namespace TableFlo.Services.Interfaces;

/// <summary>
/// Audit logging service for tracking all system actions
/// </summary>
public interface IAuditService
{
    Task LogActionAsync(int employeeId, ActionType actionType, string description, int? relatedEntityId = null, string? relatedEntityType = null, object? additionalData = null);
    Task<IEnumerable<AuditLog>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null, int? employeeId = null, ActionType? actionType = null);
    Task<IEnumerable<AuditLog>> GetLogsByEmployeeAsync(int employeeId, int limit = 100);
    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int limit = 50);
}

