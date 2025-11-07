using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Enums;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Data.Interfaces;
using TableFlo.Services.Interfaces;

namespace TableFlo.Services;

/// <summary>
/// Rotation service for managing dealer assignments and pushes
/// </summary>
public class RotationService : IRotationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TableFloDbContext _context;
    private readonly IAuditService _auditService;

    public RotationService(IUnitOfWork unitOfWork, TableFloDbContext context, IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _auditService = auditService;
    }

    public async Task<bool> ExecutePushAsync(int tableId, int employeeId)
    {
        try
        {
            var table = await _context.Tables
                .Include(t => t.CurrentAssignments)
                .ThenInclude(a => a.Dealer)
                .Include(t => t.NextAssignments)
                .ThenInclude(a => a.Dealer)
                .FirstOrDefaultAsync(t => t.Id == tableId);

            if (table == null)
                return false;

            var currentAssignment = table.CurrentAssignments.FirstOrDefault(a => a.EndTime == null);
            var nextAssignment = table.NextAssignments.FirstOrDefault();

            if (currentAssignment == null || nextAssignment == null)
                return false;

            // End current assignment
            currentAssignment.EndTime = DateTime.UtcNow;
            currentAssignment.IsCurrent = false;

            // Activate next assignment
            nextAssignment.IsCurrent = true;
            nextAssignment.StartTime = DateTime.UtcNow;

            // Update dealer statuses
            if (currentAssignment.Dealer != null)
            {
                currentAssignment.Dealer.Status = DealerStatus.Available;
            }

            if (nextAssignment.Dealer != null)
            {
                nextAssignment.Dealer.Status = DealerStatus.Dealing;
            }

            // Move current to break/available, next becomes current
            await _unitOfWork.SaveChangesAsync();

            // Log action
            await _auditService.LogActionAsync(
                employeeId,
                ActionType.PushExecuted,
                $"Push executed at table {table.TableNumber}",
                tableId,
                "Table"
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Assignment> AssignDealerAsync(int dealerId, int tableId, bool isCurrent, int employeeId)
    {
        var assignment = new Assignment
        {
            DealerId = dealerId,
            TableId = tableId,
            StartTime = isCurrent ? DateTime.UtcNow : DateTime.UtcNow.AddMinutes(20),
            IsCurrent = isCurrent,
            IsAIGenerated = false
        };

        await _unitOfWork.Assignments.AddAsync(assignment);

        // Update dealer status
        var dealer = await _unitOfWork.Dealers.GetByIdAsync(dealerId);
        if (dealer != null && isCurrent)
        {
            dealer.Status = DealerStatus.Dealing;
        }

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(
            employeeId,
            ActionType.DealerAssigned,
            $"Dealer manually assigned to table",
            dealerId,
            "Dealer"
        );

        return assignment;
    }

    public async Task<bool> RemoveDealerAsync(int assignmentId, int employeeId)
    {
        var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId);
        
        if (assignment == null)
            return false;

        assignment.EndTime = DateTime.UtcNow;
        assignment.IsCurrent = false;

        var dealer = await _unitOfWork.Dealers.GetByIdAsync(assignment.DealerId);
        if (dealer != null)
        {
            dealer.Status = DealerStatus.Available;
        }

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(
            employeeId,
            ActionType.DealerRemoved,
            $"Dealer removed from assignment",
            assignment.DealerId,
            "Dealer"
        );

        return true;
    }

    public async Task<BreakRecord> SendToBreakAsync(int dealerId, string breakType, int durationMinutes, int employeeId)
    {
        var dealer = await _context.Dealers.FindAsync(dealerId);
        
        if (dealer == null)
            throw new Exception("Dealer not found");

        // End current assignment
        var currentAssignment = await GetCurrentAssignmentAsync(dealerId);
        if (currentAssignment != null)
        {
            currentAssignment.EndTime = DateTime.UtcNow;
            currentAssignment.IsCurrent = false;
        }

        // Update dealer status
        dealer.Status = breakType == "Meal" ? DealerStatus.OnMeal : DealerStatus.OnBreak;
        dealer.LastBreakTime = DateTime.UtcNow;

        // Create break record
        var breakRecord = new BreakRecord
        {
            DealerId = dealerId,
            BreakType = breakType,
            StartTime = DateTime.UtcNow,
            ExpectedDurationMinutes = durationMinutes
        };

        await _unitOfWork.BreakRecords.AddAsync(breakRecord);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(
            employeeId,
            ActionType.DealerSentToBreak,
            $"Dealer sent to {breakType.ToLower()}",
            dealerId,
            "Dealer"
        );

        return breakRecord;
    }

    public async Task<bool> ReturnFromBreakAsync(int dealerId, int employeeId)
    {
        var dealer = await _context.Dealers.FindAsync(dealerId);
        
        if (dealer == null)
            return false;

        // End break record
        var activeBreak = await _context.BreakRecords
            .Where(b => b.DealerId == dealerId && b.EndTime == null)
            .OrderByDescending(b => b.StartTime)
            .FirstOrDefaultAsync();

        if (activeBreak != null)
        {
            activeBreak.EndTime = DateTime.UtcNow;
        }

        // Update dealer status
        dealer.Status = DealerStatus.Available;

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogActionAsync(
            employeeId,
            ActionType.DealerReturnedFromBreak,
            $"Dealer returned from break",
            dealerId,
            "Dealer"
        );

        return true;
    }

    public async Task<Assignment?> GetCurrentAssignmentAsync(int dealerId)
    {
        return await _context.Assignments
            .Include(a => a.Table)
            .Where(a => a.DealerId == dealerId && a.IsCurrent && a.EndTime == null)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Dealer>> GetDealersOnBreakAsync()
    {
        return await _context.Dealers
            .Include(d => d.Employee)
            .Where(d => d.Status == DealerStatus.OnBreak || d.Status == DealerStatus.OnMeal)
            .ToListAsync();
    }
}

