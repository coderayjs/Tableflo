using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Enums;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Services.Interfaces;

namespace TableFlo.Services;

/// <summary>
/// Service for managing rotation strings/groups
/// </summary>
public class RotationStringService : IRotationStringService
{
    private readonly TableFloDbContext _context;
    private readonly IRotationService _rotationService;
    private readonly IAuditService _auditService;

    public RotationStringService(
        TableFloDbContext context,
        IRotationService rotationService,
        IAuditService auditService)
    {
        _context = context;
        _rotationService = rotationService;
        _auditService = auditService;
    }

    public async Task<IEnumerable<RotationString>> GetAllStringsAsync()
    {
        return await _context.RotationStrings
            .Include(rs => rs.DealerAssignments)
                .ThenInclude(dsa => dsa.Dealer)
                    .ThenInclude(d => d!.Employee)
            .Where(rs => rs.IsActive)
            .OrderBy(rs => rs.Priority)
            .ToListAsync();
    }

    public async Task<RotationString?> GetStringByIdAsync(int stringId)
    {
        return await _context.RotationStrings
            .Include(rs => rs.DealerAssignments)
                .ThenInclude(dsa => dsa.Dealer)
                    .ThenInclude(d => d!.Employee)
            .FirstOrDefaultAsync(rs => rs.Id == stringId);
    }

    public async Task<RotationString> CreateStringAsync(string name, string description)
    {
        var rotationString = new RotationString
        {
            Name = name,
            Description = description,
            IsActive = true,
            Priority = await _context.RotationStrings.CountAsync() + 1
        };

        await _context.RotationStrings.AddAsync(rotationString);
        await _context.SaveChangesAsync();

        return rotationString;
    }

    public async Task<bool> AddDealerToStringAsync(int dealerId, int stringId, int rotationOrder)
    {
        try
        {
            // Check if dealer already in this string
            var existing = await _context.DealerStringAssignments
                .FirstOrDefaultAsync(dsa => dsa.DealerId == dealerId && dsa.RotationStringId == stringId && dsa.IsActive);

            if (existing != null)
                return false; // Already assigned

            var assignment = new DealerStringAssignment
            {
                DealerId = dealerId,
                RotationStringId = stringId,
                RotationOrder = rotationOrder,
                IsActive = true
            };

            await _context.DealerStringAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveDealerFromStringAsync(int dealerId, int stringId)
    {
        try
        {
            var assignment = await _context.DealerStringAssignments
                .FirstOrDefaultAsync(dsa => dsa.DealerId == dealerId && dsa.RotationStringId == stringId && dsa.IsActive);

            if (assignment == null)
                return false;

            assignment.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExecuteStringRotationAsync(int stringId, int employeeId)
    {
        try
        {
            var rotationString = await GetStringByIdAsync(stringId);
            if (rotationString == null)
                return false;

            // Get dealers in rotation order
            var dealers = rotationString.DealerAssignments
                .Where(dsa => dsa.IsActive)
                .OrderBy(dsa => dsa.RotationOrder)
                .Select(dsa => dsa.Dealer)
                .Where(d => d != null)
                .ToList();

            if (!dealers.Any())
                return false;

            // Get tables for this string
            var tables = await _context.Tables
                .Where(t => rotationString.TableIds.Contains(t.Id) && t.Status == TableStatus.Open)
                .ToListAsync();

            // Assign dealers to tables in rotation order
            for (int i = 0; i < Math.Min(dealers.Count, tables.Count); i++)
            {
                var dealer = dealers[i];
                var table = tables[i];

                if (dealer != null)
                {
                    await _rotationService.AssignDealerAsync(dealer.Id, table.Id, true, employeeId);
                }
            }

            await _auditService.LogActionAsync(
                employeeId,
                ActionType.ManualOverride,
                $"Executed rotation for string: {rotationString.Name}"
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Dealer>> GetDealersInStringAsync(int stringId)
    {
        var assignments = await _context.DealerStringAssignments
            .Include(dsa => dsa.Dealer)
                .ThenInclude(d => d!.Employee)
            .Where(dsa => dsa.RotationStringId == stringId && dsa.IsActive)
            .OrderBy(dsa => dsa.RotationOrder)
            .ToListAsync();

        return assignments.Select(dsa => dsa.Dealer).Where(d => d != null).Cast<Dealer>();
    }
}

