using Allure.Core.Services;
using Allure.Desktop.Services;
using Allure.Desktop.ViewModels;
using Allure.Desktop.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Allure.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureServices();
    }

    private static void ConfigureServices()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        string connectionString = configuration.GetConnectionString("Default")
            ?? "server=localhost;database=Yes;uid=root;pwd=;charset=utf8mb4";

        var services = new ServiceCollection();
        services.AddSingleton(new DbContextFactory(connectionString));
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IOrderCalculationService, OrderCalculationService>();
        services.AddSingleton<IOrderStatusService, OrderStatusService>();
        services.AddSingleton<IDocumentNumberGenerator, DocumentNumberGenerator>();
        services.AddSingleton<IIntegrityService, IntegrityService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<ValidationService>();
        services.AddSingleton<SessionService>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<OrderViewModel>();
        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loginViewModel = Services.GetRequiredService<LoginViewModel>();
            desktop.MainWindow = new LoginWindow(loginViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
