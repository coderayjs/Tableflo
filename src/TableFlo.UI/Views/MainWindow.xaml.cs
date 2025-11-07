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
    private readonly TableManagementViewModel _tableViewModel;
    private readonly AnalyticsViewModel _analyticsViewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get ViewModels from DI
        _mainViewModel = App.GetService<MainViewModel>();
        _dealerViewModel = App.GetService<DealerManagementViewModel>();
        _tableViewModel = App.GetService<TableManagementViewModel>();
        _analyticsViewModel = App.GetService<AnalyticsViewModel>();
        
        // Set main DataContext (for Dashboard view)
        DataContext = _mainViewModel;
        
        // Set current user name in header
        if (SessionManager.CurrentEmployee != null)
        {
            UserInfoText.Text = $"Logged in as: {SessionManager.CurrentEmployee.FullName} (#{SessionManager.CurrentEmployee.EmployeeNumber})";
        }
        
        // Set DataContext for each view
        DealersView.DataContext = _dealerViewModel;
        TablesView.DataContext = _tableViewModel;
        AnalyticsView.DataContext = _analyticsViewModel;
    }

    private void ShowDashboard_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Dashboard");
    }

    private void ShowDealers_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Dealers");
    }

    private void ShowTables_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Tables");
    }

    private void ShowAnalytics_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Analytics");
    }

    private void ShowView(string viewName)
    {
        // Hide all views
        DashboardView.Visibility = Visibility.Collapsed;
        DealersView.Visibility = Visibility.Collapsed;
        TablesView.Visibility = Visibility.Collapsed;
        AnalyticsView.Visibility = Visibility.Collapsed;

        // Show selected view
        switch (viewName)
        {
            case "Dashboard":
                DashboardView.Visibility = Visibility.Visible;
                break;
            case "Dealers":
                DealersView.Visibility = Visibility.Visible;
                break;
            case "Tables":
                TablesView.Visibility = Visibility.Visible;
                break;
            case "Analytics":
                AnalyticsView.Visibility = Visibility.Visible;
                break;
        }
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

