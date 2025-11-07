using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Models;
using TableFlo.Data;
using TableFlo.Services.Interfaces;

namespace TableFlo.UI.ViewModels;

/// <summary>
/// Dashboard ViewModel for dealer rotation management
/// </summary>
public class DashboardViewModel : ViewModelBase
{
    private readonly TableFloDbContext _context;
    private readonly IRotationService _rotationService;
    private readonly ISchedulingService _schedulingService;

    public DashboardViewModel(TableFloDbContext context, IRotationService rotationService, ISchedulingService schedulingService)
    {
        _context = context;
        _rotationService = rotationService;
        _schedulingService = schedulingService;
        
        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
    }

    public ObservableCollection<Table> Tables { get; set; } = new();
    public ObservableCollection<Dealer> DealersOnBreak { get; set; } = new();

    public RelayCommand LoadDataCommand { get; }

    private async Task LoadDataAsync()
    {
        // Load open tables with assignments
        var tables = await _context.Tables
            .Include(t => t.CurrentAssignments)
                .ThenInclude(a => a.Dealer)
                    .ThenInclude(d => d!.Employee)
            .Include(t => t.NextAssignments)
                .ThenInclude(a => a.Dealer)
                    .ThenInclude(d => d!.Employee)
            .Where(t => t.Status == Core.Enums.TableStatus.Open)
            .ToListAsync();

        Tables.Clear();
        foreach (var table in tables)
        {
            Tables.Add(table);
        }

        // Load dealers on break
        var dealersOnBreak = await _rotationService.GetDealersOnBreakAsync();
        DealersOnBreak.Clear();
        foreach (var dealer in dealersOnBreak)
        {
            DealersOnBreak.Add(dealer);
        }
    }
}

// Placeholder RelayCommand for now
public class RelayCommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute)
    {
        _execute = execute;
    }

    public void Execute()
    {
        _execute();
    }
}

