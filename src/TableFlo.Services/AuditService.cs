using System.Text.Json;
using TableFlo.Core.Enums;
using TableFlo.Core.Models;
using TableFlo.Data.Interfaces;
using TableFlo.Services.Interfaces;

namespace TableFlo.Services;

/// <summary>
/// Audit logging service implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogActionAsync(int employeeId, ActionType actionType, string description, 
        int? relatedEntityId = null, string? relatedEntityType = null, object? additionalData = null)
    {
        var log = new AuditLog
        {
            EmployeeId = employeeId,
            ActionType = actionType,
            Description = description,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null, 
        int? employeeId = null, ActionType? actionType = null)
    {
        var logs = await _unitOfWork.AuditLogs.GetAllAsync();
        
        var query = logs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value);

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);

        if (actionType.HasValue)
            query = query.Where(l => l.ActionType == actionType.Value);

        return query.OrderByDescending(l => l.Timestamp).ToList();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByEmployeeAsync(int employeeId, int limit = 100)
    {
        var logs = await _unitOfWork.AuditLogs.FindAsync(l => l.EmployeeId == employeeId);
        return logs.OrderByDescending(l => l.Timestamp).Take(limit);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int limit = 50)
    {
        var logs = await _unitOfWork.AuditLogs.GetAllAsync();
        return logs.OrderByDescending(l => l.Timestamp).Take(limit);
    }
}

