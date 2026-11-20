using Allure.Core.Models;
using Allure.Desktop.ViewModels;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Allure.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<LoginViewModel>();
        WireViewModel();
    }

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        WireViewModel();
    }

    private void WireViewModel()
    {
        if (DataContext is LoginViewModel viewModel)
            viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void OnLoginSucceeded(User user)
    {
        var mainViewModel = App.Services.GetRequiredService<MainViewModel>();
        var mainWindow = new MainWindow(mainViewModel);
        mainWindow.Show();
        Close();
    }
}
