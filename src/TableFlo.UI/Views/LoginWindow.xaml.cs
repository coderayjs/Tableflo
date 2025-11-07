using System.Windows;
using System.Windows.Media.Animation;
using TableFlo.UI.ViewModels;

namespace TableFlo.UI.Views;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = App.GetService<LoginViewModel>();
        
        // Subscribe to login success event
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.LoginSucceeded += OnLoginSucceeded;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private async void SignIn_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as LoginViewModel;
        if (viewModel != null)
        {
            // Get password from PasswordBox
            viewModel.Password = PasswordBox.Password;
            await viewModel.LoginAsync();
        }
    }

    private async void OnLoginSucceeded(object? sender, string userName)
    {
        // Hide loading, show success
        LoadingOverlay.Visibility = Visibility.Collapsed;
        SuccessOverlay.Visibility = Visibility.Visible;
        WelcomeText.Text = $"Welcome back, {userName}!";
        
        // Wait 1.5 seconds to show success message
        await Task.Delay(1500);
        
        // Fade out and open main window
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
        fadeOut.Completed += (s, e) =>
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        };
        this.BeginAnimation(OpacityProperty, fadeOut);
    }
}

