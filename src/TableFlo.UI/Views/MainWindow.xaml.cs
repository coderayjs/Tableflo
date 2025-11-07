using System.Windows;
using TableFlo.Services.Interfaces;
using TableFlo.Core.Enums;

namespace TableFlo.UI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set current user name in header
        if (SessionManager.CurrentEmployee != null)
        {
            DataContext = new
            {
                CurrentUserName = $"Logged in as: {SessionManager.CurrentEmployee.FullName} (#{SessionManager.CurrentEmployee.EmployeeNumber})"
            };
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

