using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TableFlo.Data;
using TableFlo.Services.Interfaces;

namespace TableFlo.UI.ViewModels;

/// <summary>
/// Analytics ViewModel - Dashboard metrics and reporting
/// </summary>
public class AnalyticsViewModel : ViewModelBase
{
    private readonly TableFloDbContext _context;
    private readonly IRotationService _rotationService;

    public AnalyticsViewModel(TableFloDbContext context, IRotationService rotationService)
    {
        _context = context;
        _rotationService = rotationService;

        RefreshCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadAnalyticsAsync);

        _ = LoadAnalyticsAsync();
    }

    #region Properties

    private int _totalDealers;
    public int TotalDealers
    {
        get => _totalDealers;
        set => SetProperty(ref _totalDealers, value);
    }

    private int _activeDealers;
    public int ActiveDealers
    {
        get => _activeDealers;
        set => SetProperty(ref _activeDealers, value);
    }

    private int _dealersOnBreak;
    public int DealersOnBreak
    {
        get => _dealersOnBreak;
        set => SetProperty(ref _dealersOnBreak, value);
    }

    private int _openTables;
    public int OpenTables
    {
        get => _openTables;
        set => SetProperty(ref _openTables, value);
    }

    private int _totalTables;
    public int TotalTables
    {
        get => _totalTables;
        set => SetProperty(ref _totalTables, value);
    }

    private string _statusMessage = "Loading analytics...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadAnalyticsAsync()
    {
        try
        {
            TotalDealers = await _context.Dealers.CountAsync();
            ActiveDealers = await _context.Dealers
                .Where(d => d.Status == Core.Enums.DealerStatus.Dealing)
                .CountAsync();
            DealersOnBreak = await _context.Dealers
                .Where(d => d.Status == Core.Enums.DealerStatus.OnBreak)
                .CountAsync();

            TotalTables = await _context.Tables.CountAsync();
            OpenTables = await _context.Tables
                .Where(t => t.Status == Core.Enums.TableStatus.Open)
                .CountAsync();

            StatusMessage = $"Last updated: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading analytics: {ex.Message}";
        }
    }

    #endregion
}
