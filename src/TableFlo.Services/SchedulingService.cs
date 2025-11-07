using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Enums;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Data.Interfaces;
using TableFlo.Services.Interfaces;

namespace TableFlo.Services;

/// <summary>
/// AI-powered scheduling service implementation
/// This is the core intelligence of TableFlo
/// </summary>
public class SchedulingService : ISchedulingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TableFloDbContext _context;
    private const int BREAK_INTERVAL_MINUTES = 120; // 2 hours
    private const int MEAL_DEADLINE_HOURS = 5;

    public SchedulingService(IUnitOfWork unitOfWork, TableFloDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<ScheduleResult> GenerateScheduleAsync(DateTime startTime, int pushCount = 10)
    {
        var result = new ScheduleResult();
        
        try
        {
            // Get all open tables
            var tables = await _context.Tables
                .Include(t => t.CurrentAssignments)
                .Where(t => t.Status == TableStatus.Open)
                .ToListAsync();

            if (!tables.Any())
            {
                result.Success = false;
                result.Message = "No open tables found";
                return result;
            }

            // Get all available dealers (on shift, not on break)
            var dealers = await _context.Dealers
                .Include(d => d.Employee)
                .Include(d => d.Certifications)
                .Include(d => d.AssignmentHistory)
                .Where(d => d.Status == DealerStatus.Available || d.Status == DealerStatus.Dealing)
                .ToListAsync();

            if (!dealers.Any())
            {
                result.Success = false;
                result.Message = "No available dealers found";
                return result;
            }

            // Generate initial assignments for each table
            foreach (var table in tables)
            {
                var dealer = await SelectBestDealerForTable(table, dealers, startTime);
                
                if (dealer != null)
                {
                    var assignment = new Assignment
                    {
                        DealerId = dealer.Id,
                        TableId = table.Id,
                        StartTime = startTime,
                        IsCurrent = true,
                        IsAIGenerated = true
                    };
                    
                    result.Assignments.Add(assignment);
                    dealers.Remove(dealer); // Remove from available pool
                }
                else
                {
                    result.Warnings.Add($"Could not find qualified dealer for table {table.TableNumber}");
                }
            }

            // Calculate fairness metrics
            result.Metrics["TotalAssignments"] = result.Assignments.Count;
            result.Metrics["TablesWithoutDealers"] = tables.Count - result.Assignments.Count;
            result.Metrics["AvailableDealers"] = dealers.Count;

            result.Success = true;
            result.Message = $"Successfully generated schedule with {result.Assignments.Count} assignments";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Error generating schedule: {ex.Message}";
        }

        return result;
    }

    public async Task<Dealer?> RecommendNextDealerAsync(int tableId)
    {
        var table = await _context.Tables
            .Include(t => t.CurrentAssignments)
            .ThenInclude(a => a.Dealer)
            .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table == null)
            return null;

        var availableDealers = await _context.Dealers
            .Include(d => d.Employee)
            .Include(d => d.Certifications)
            .Include(d => d.AssignmentHistory)
            .Where(d => d.Status == DealerStatus.Available || d.Status == DealerStatus.OnBreak)
            .ToListAsync();

        return await SelectBestDealerForTable(table, availableDealers, DateTime.UtcNow);
    }

    public async Task<ScheduleResult> HandleCallInAsync(int dealerId)
    {
        var result = new ScheduleResult { Message = "Handling call-in" };

        try
        {
            var dealer = await _context.Dealers
                .Include(d => d.AssignmentHistory)
                .FirstOrDefaultAsync(d => d.Id == dealerId);

            if (dealer == null)
            {
                result.Success = false;
                result.Message = "Dealer not found";
                return result;
            }

            // Mark dealer as called in
            dealer.Status = DealerStatus.CalledIn;

            // Find current assignment
            var currentAssignment = await _context.Assignments
                .Where(a => a.DealerId == dealerId && a.IsCurrent && a.EndTime == null)
                .FirstOrDefaultAsync();

            if (currentAssignment != null)
            {
                // End current assignment
                currentAssignment.EndTime = DateTime.UtcNow;

                // Find replacement dealer
                var table = await _context.Tables.FindAsync(currentAssignment.TableId);
                if (table != null)
                {
                    var replacement = await RecommendNextDealerAsync(table.Id);
                    if (replacement != null)
                    {
                        var newAssignment = new Assignment
                        {
                            DealerId = replacement.Id,
                            TableId = table.Id,
                            StartTime = DateTime.UtcNow,
                            IsCurrent = true,
                            IsAIGenerated = true
                        };
                        
                        result.Assignments.Add(newAssignment);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            result.Success = true;
            result.Message = "Successfully handled call-in and reassigned tables";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Error handling call-in: {ex.Message}";
        }

        return result;
    }

    public async Task<bool> NeedsBreakAsync(int dealerId)
    {
        var dealer = await _context.Dealers.FindAsync(dealerId);
        
        if (dealer == null || dealer.LastBreakTime == null)
            return true; // No break yet, needs one

        var timeSinceLastBreak = DateTime.UtcNow - dealer.LastBreakTime.Value;
        return timeSinceLastBreak.TotalMinutes >= BREAK_INTERVAL_MINUTES;
    }

    public async Task<IEnumerable<Dealer>> GetDealersDueForBreakAsync()
    {
        var dealers = await _context.Dealers
            .Include(d => d.Employee)
            .Where(d => d.Status == DealerStatus.Dealing)
            .ToListAsync();

        var dueForBreak = new List<Dealer>();

        foreach (var dealer in dealers)
        {
            if (await NeedsBreakAsync(dealer.Id))
            {
                dueForBreak.Add(dealer);
            }
        }

        return dueForBreak;
    }

    public async Task<double> CalculateFairnessScoreAsync(int dealerId)
    {
        var dealer = await _context.Dealers
            .Include(d => d.AssignmentHistory)
            .FirstOrDefaultAsync(d => d.Id == dealerId);

        if (dealer == null || !dealer.AssignmentHistory.Any())
            return 1.0; // Perfect score for new dealer

        // Calculate rotation diversity (how many different games they've worked)
        var gamesWorked = dealer.AssignmentHistory
            .Select(a => a.Table?.GameType)
            .Distinct()
            .Count();

        // Calculate total time worked today
        var totalMinutesWorked = dealer.AssignmentHistory
            .Where(a => a.StartTime.Date == DateTime.Today)
            .Sum(a => ((a.EndTime ?? DateTime.UtcNow) - a.StartTime).TotalMinutes);

        // Higher score = needs more assignments for fairness
        var diversityScore = 1.0 / (gamesWorked + 1);
        var workloadScore = 1.0 / (totalMinutesWorked + 1);

        return (diversityScore + workloadScore) / 2.0;
    }

    /// <summary>
    /// Core AI algorithm: Select best dealer for a table based on multiple factors
    /// </summary>
    private async Task<Dealer?> SelectBestDealerForTable(Table table, List<Dealer> availableDealers, DateTime assignmentTime)
    {
        var scoredDealers = new List<(Dealer dealer, double score)>();

        foreach (var dealer in availableDealers)
        {
            double score = 0;

            // 1. SKILL MATCHING (highest priority)
            var certification = dealer.Certifications
                .FirstOrDefault(c => c.GameType == table.GameType && c.IsActive);

            if (certification == null)
                continue; // Skip dealers without certification

            score += (int)certification.ProficiencyLevel * 20; // 20-100 points

            // 2. FAIRNESS SCORE (rotation diversity and workload balance)
            var fairnessScore = await CalculateFairnessScoreAsync(dealer.Id);
            score += fairnessScore * 30; // Up to 30 points

            // 3. SENIORITY (minor factor)
            score += dealer.SeniorityLevel * 2; // Up to 10 points for senior dealers

            // 4. PREFERRED PIT (bonus if they prefer this pit)
            if (!string.IsNullOrEmpty(dealer.PreferredPit) && dealer.PreferredPit == table.Pit)
                score += 15;

            // 5. TIME SINCE LAST BREAK (penalty if needs break soon)
            if (dealer.LastBreakTime.HasValue)
            {
                var timeSinceBreak = assignmentTime - dealer.LastBreakTime.Value;
                if (timeSinceBreak.TotalMinutes < 90)
                    score += 10; // Bonus for recently rested
                else if (timeSinceBreak.TotalMinutes > 120)
                    score -= 20; // Penalty if needs break soon
            }

            // 6. MOVEMENT EFFICIENCY (bonus for same pit to minimize walking)
            var lastAssignment = dealer.AssignmentHistory
                .OrderByDescending(a => a.StartTime)
                .FirstOrDefault();

            if (lastAssignment?.Table?.Pit == table.Pit)
                score += 10; // Same pit bonus

            scoredDealers.Add((dealer, score));
        }

        // Return dealer with highest score
        return scoredDealers
            .OrderByDescending(sd => sd.score)
            .FirstOrDefault()
            .dealer;
    }
}

