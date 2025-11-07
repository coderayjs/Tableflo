using System.Windows;
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
            await viewModel.LoginAsync();
        }
    }
}

