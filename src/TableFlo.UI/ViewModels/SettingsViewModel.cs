using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TableFlo.Data;

namespace TableFlo.UI.ViewModels;

/// <summary>
/// Settings/Configuration ViewModel
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly TableFloDbContext _context;

    public SettingsViewModel(TableFloDbContext context)
    {
        _context = context;

        SaveSettingsCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(SaveSettingsAsync);
        RefreshCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(LoadSettingsAsync);

        _ = LoadSettingsAsync();
    }

    #region Properties

    private int _breakIntervalMinutes = 120;
    public int BreakIntervalMinutes
    {
        get => _breakIntervalMinutes;
        set => SetProperty(ref _breakIntervalMinutes, value);
    }

    private int _pushIntervalMinutes = 20;
    public int PushIntervalMinutes
    {
        get => _pushIntervalMinutes;
        set => SetProperty(ref _pushIntervalMinutes, value);
    }

    private int _mealDeadlineHours = 5;
    public int MealDeadlineHours
    {
        get => _mealDeadlineHours;
        set => SetProperty(ref _mealDeadlineHours, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    #endregion

    #region Commands

    public ICommand SaveSettingsCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadSettingsAsync()
    {
        try
        {
            // Load from first shift (default settings)
            var shift = await _context.Shifts.FirstOrDefaultAsync();
            if (shift != null)
            {
                BreakIntervalMinutes = shift.BreakIntervalMinutes;
                MealDeadlineHours = shift.MealDeadlineHours;
            }

            // Push interval from first table
            var table = await _context.Tables.FirstOrDefaultAsync();
            if (table != null)
            {
                PushIntervalMinutes = table.PushIntervalMinutes;
            }

            StatusMessage = "Settings loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading settings: {ex.Message}";
        }
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            // Update all shifts
            var shifts = await _context.Shifts.ToListAsync();
            foreach (var shift in shifts)
            {
                shift.BreakIntervalMinutes = BreakIntervalMinutes;
                shift.MealDeadlineHours = MealDeadlineHours;
            }

            // Update all tables
            var tables = await _context.Tables.ToListAsync();
            foreach (var table in tables)
            {
                table.PushIntervalMinutes = PushIntervalMinutes;
            }

            await _context.SaveChangesAsync();
            StatusMessage = "Settings saved successfully!";
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}

