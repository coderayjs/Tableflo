using System.Windows;
using TableFlo.Services.Interfaces;
using TableFlo.Core.Enums;
using TableFlo.UI.ViewModels;

namespace TableFlo.UI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly DealerManagementViewModel _dealerViewModel;
    private readonly StringManagementViewModel _stringViewModel;
    private readonly TableManagementViewModel _tableViewModel;
    private readonly AnalyticsViewModel _analyticsViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get ViewModels from DI
        _mainViewModel = App.GetService<MainViewModel>();
        _dealerViewModel = App.GetService<DealerManagementViewModel>();
        _stringViewModel = App.GetService<StringManagementViewModel>();
        _tableViewModel = App.GetService<TableManagementViewModel>();
        _analyticsViewModel = App.GetService<AnalyticsViewModel>();
        _settingsViewModel = App.GetService<SettingsViewModel>();
        
        // Set main DataContext (for Dashboard view)
        DataContext = _mainViewModel;
        
        // Set current user name in header
        if (SessionManager.CurrentEmployee != null)
        {
            UserInfoText.Text = $"Logged in as: {SessionManager.CurrentEmployee.FullName} (#{SessionManager.CurrentEmployee.EmployeeNumber})";
        }
        
        // Set DataContext for each view
        DealersView.DataContext = _dealerViewModel;
        StringsView.DataContext = _stringViewModel;
        TablesView.DataContext = _tableViewModel;
        AnalyticsView.DataContext = _analyticsViewModel;
        SettingsView.DataContext = _settingsViewModel;
        
        // Show Dashboard by default
        ShowView("Dashboard");
    }

    private void ShowDashboard_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Dashboard");
    }

    private void ShowDealers_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Dealers");
    }

    private void ShowStrings_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Strings");
    }

    private void ShowTables_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Tables");
    }

    private void ShowAnalytics_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Analytics");
    }

    private void ShowSettings_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Settings");
    }

    private void ShowView(string viewName)
    {
        // Reset all button styles
        ResetButtonStyles();
        
        // Hide all views
        DashboardView.Visibility = Visibility.Collapsed;
        DealersView.Visibility = Visibility.Collapsed;
        StringsView.Visibility = Visibility.Collapsed;
        TablesView.Visibility = Visibility.Collapsed;
        AnalyticsView.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Collapsed;

        // Show selected view and highlight active button
        switch (viewName)
        {
            case "Dashboard":
                DashboardView.Visibility = Visibility.Visible;
                SetActiveButton(DashboardButton);
                break;
            case "Dealers":
                DealersView.Visibility = Visibility.Visible;
                SetActiveButton(DealersButton);
                break;
            case "Strings":
                StringsView.Visibility = Visibility.Visible;
                SetActiveButton(StringsButton);
                break;
            case "Tables":
                TablesView.Visibility = Visibility.Visible;
                SetActiveButton(TablesButton);
                break;
            case "Analytics":
                AnalyticsView.Visibility = Visibility.Visible;
                SetActiveButton(AnalyticsButton);
                break;
            case "Settings":
                SettingsView.Visibility = Visibility.Visible;
                SetActiveButton(SettingsButton);
                break;
        }
    }
    
    private void ResetButtonStyles()
    {
        var buttons = new[] { DashboardButton, DealersButton, StringsButton, TablesButton, AnalyticsButton, SettingsButton };
        foreach (var button in buttons)
        {
            button.Background = System.Windows.Media.Brushes.Transparent;
            button.Tag = null;
        }
    }
    
    private void SetActiveButton(System.Windows.Controls.Button button)
    {
        // Active state: darker background with subtle accent
        var activeBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(23, 30, 38));
        button.Background = activeBrush;
        button.Tag = "Active"; // Mark as active for styling
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (SessionManager.CurrentEmployee != null)
        {
            // Log the logout
            var auditService = App.GetService<IAuditService>();
            await auditService.LogActionAsync(
                SessionManager.CurrentEmployee.Id,
                ActionType.Logout,
                $"User {SessionManager.CurrentEmployee.FullName} logged out"
            );
        }

        SessionManager.Logout();

        // Show login window
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        // Close this window
        Close();
    }
}

