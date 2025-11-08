using TableFlo.Core.Enums;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Services.Interfaces;

namespace TableFlo.Services;

/// <summary>
/// Seeds sample data for demo purposes
/// </summary>
public class DataSeeder
{
    private readonly TableFloDbContext _context;
    private readonly IAuthenticationService _authService;

    public DataSeeder(TableFloDbContext context, IAuthenticationService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task SeedAsync()
    {
        // Check if already seeded
        if (_context.Employees.Any())
            return;

        await SeedEmployeesAndDealersAsync();
        await SeedTablesAsync();
        await SeedShiftsAsync();
        await SeedRotationStringsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedEmployeesAndDealersAsync()
    {
        // Create dealers
        var dealerNames = new[]
        {
            ("George", "Soros"), ("Bob", "Schmidt"), ("Jimmy", "John"),
            ("Chris", "Scoff"), ("Sally", "Jones"), ("Amy", "Smith"),
            ("Wilson", "Chen"), ("Garcia", "Lopez"), ("Keller", "Brown"),
            ("Davis", "Taylor"), ("Nguyen", "Tran")
        };

        int empNumber = 1001;
        foreach (var (firstName, lastName) in dealerNames)
        {
            var employee = await _authService.RegisterEmployeeAsync(
                $"D{empNumber}",
                firstName,
                lastName,
                "dealer123",
                "Dealer"
            );

            var dealer = new Dealer
            {
                EmployeeId = employee.Id,
                Status = DealerStatus.Available,
                SeniorityLevel = Random.Shared.Next(1, 10),
                ShiftStart = new TimeSpan(6, 0, 0),
                ShiftEnd = new TimeSpan(14, 0, 0),
                PreferredPit = Random.Shared.Next(0, 2) == 0 ? "Main" : "HighLimit"
            };

            // Add random certifications
            var gamesToCertify = new[] { GameType.Blackjack, GameType.Roulette, GameType.Craps, GameType.PaiGow, GameType.Baccarat };
            var certCount = Random.Shared.Next(2, 5);
            
            for (int i = 0; i < certCount; i++)
            {
                var gameType = gamesToCertify[Random.Shared.Next(gamesToCertify.Length)];
                
                if (!dealer.Certifications.Any(c => c.GameType == gameType))
                {
                    dealer.Certifications.Add(new DealerCertification
                    {
                        GameType = gameType,
                        ProficiencyLevel = (ProficiencyLevel)Random.Shared.Next(3, 6),
                        CertifiedDate = DateTime.UtcNow.AddMonths(-Random.Shared.Next(1, 24)),
                        IsActive = true
                    });
                }
            }

            await _context.Dealers.AddAsync(dealer);
            empNumber++;
        }

        // Create admin/supervisors
        var admin = await _authService.RegisterEmployeeAsync(
            "ADMIN001",
            "Admin",
            "User",
            "admin123",
            "Administrator"
        );
        
        var supervisor = await _authService.RegisterEmployeeAsync(
            "SUP001",
            "John",
            "Manager",
            "super123",
            "Supervisor"
        );
    }

    private async Task SeedTablesAsync()
    {
        var tables = new List<Table>
        {
            new Table { TableNumber = "BJ1", GameType = GameType.Blackjack, Status = TableStatus.Open, MinBet = 25, MaxBet = 1000, Pit = "Main", RequiredDealerCount = 1, PushIntervalMinutes = 20 },
            new Table { TableNumber = "BJ2", GameType = GameType.Blackjack, Status = TableStatus.Open, MinBet = 25, MaxBet = 1000, Pit = "Main", RequiredDealerCount = 1, PushIntervalMinutes = 20 },
            new Table { TableNumber = "BJ302", GameType = GameType.Blackjack, Status = TableStatus.Open, MinBet = 50, MaxBet = 2000, Pit = "HighLimit", RequiredDealerCount = 1, IsHighLimit = true, PushIntervalMinutes = 20 },
            new Table { TableNumber = "R12", GameType = GameType.Roulette, Status = TableStatus.Open, MinBet = 10, MaxBet = 500, Pit = "Main", RequiredDealerCount = 1, PushIntervalMinutes = 20 },
            new Table { TableNumber = "R301", GameType = GameType.Roulette, Status = TableStatus.Open, MinBet = 25, MaxBet = 1000, Pit = "Main", RequiredDealerCount = 1, PushIntervalMinutes = 20 },
            new Table { TableNumber = "CR4", GameType = GameType.Craps, Status = TableStatus.Open, MinBet = 15, MaxBet = 1000, Pit = "Main", RequiredDealerCount = 3, PushIntervalMinutes = 30 },
            new Table { TableNumber = "PG101", GameType = GameType.PaiGow, Status = TableStatus.Open, MinBet = 25, MaxBet = 500, Pit = "Main", RequiredDealerCount = 1, PushIntervalMinutes = 20 },
            new Table { TableNumber = "TX311", GameType = GameType.TexasHoldem, Status = TableStatus.Open, MinBet = 50, MaxBet = 2000, Pit = "HighLimit", RequiredDealerCount = 1, IsHighLimit = true, PushIntervalMinutes = 20 },
            new Table { TableNumber = "SD101", GameType = GameType.SpanishTwentyOne, Status = TableStatus.Open, MinBet = 10, MaxBet = 500, Pit = "Main", RequiredDealerCount = 1, PushIntervalMinutes = 20 },
            new Table { TableNumber = "BJ102", GameType = GameType.Blackjack, Status = TableStatus.Closed, MinBet = 25, MaxBet = 1000, Pit = "Main", RequiredDealerCount = 1, PushIntervalMinutes = 20 }
        };

        foreach (var table in tables)
        {
            await _context.Tables.AddAsync(table);
        }
    }

    private async Task SeedShiftsAsync()
    {
        var shifts = new List<Shift>
        {
            new Shift { ShiftName = "Day Shift", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(14, 0, 0), MinimumDealers = 8, BreakIntervalMinutes = 120, MealDeadlineHours = 5 },
            new Shift { ShiftName = "Swing Shift", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(22, 0, 0), MinimumDealers = 12, BreakIntervalMinutes = 120, MealDeadlineHours = 5 },
            new Shift { ShiftName = "Graveyard Shift", StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(6, 0, 0), MinimumDealers = 6, BreakIntervalMinutes = 120, MealDeadlineHours = 5 }
        };

        foreach (var shift in shifts)
        {
            await _context.Shifts.AddAsync(shift);
        }
    }

    private async Task SeedRotationStringsAsync()
    {
        // Only seed if no strings exist
        if (await _context.RotationStrings.AnyAsync())
            return;

        var strings = new List<Core.Models.RotationString>
        {
            new Core.Models.RotationString
            {
                Name = "Main Floor",
                Description = "Main casino floor rotation - Blackjack, Roulette, and standard games",
                IsActive = true,
                Priority = 1
            },
            new Core.Models.RotationString
            {
                Name = "Craps",
                Description = "Craps table rotation - Specialized dealers for Craps games",
                IsActive = true,
                Priority = 2
            },
            new Core.Models.RotationString
            {
                Name = "High Limit",
                Description = "High limit tables - Premium games with higher betting limits",
                IsActive = true,
                Priority = 3
            },
            new Core.Models.RotationString
            {
                Name = "String 4",
                Description = "Additional rotation group for overflow or special events",
                IsActive = true,
                Priority = 4
            },
            new Core.Models.RotationString
            {
                Name = "String 5",
                Description = "Additional rotation group for overflow or special events",
                IsActive = true,
                Priority = 5
            }
        };

        foreach (var rotationString in strings)
        {
            await _context.RotationStrings.AddAsync(rotationString);
        }
    }
}

