using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TableFlo.Data;
using TableFlo.Data.Interfaces;
using TableFlo.Data.Repositories;
using TableFlo.Services;
using TableFlo.Services.Interfaces;
using TableFlo.UI.ViewModels;

namespace TableFlo.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Database
                services.AddDbContext<TableFloDbContext>(options =>
                    options.UseSqlite("Data Source=tableflo.db"));

                // Data Layer
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Services
                services.AddScoped<IAuthenticationService, AuthenticationService>();
                services.AddScoped<IAuditService, AuditService>();
                services.AddScoped<ISchedulingService, SchedulingService>();
                services.AddScoped<IRotationService, RotationService>();

                // ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<DealerManagementViewModel>();
                services.AddTransient<TableManagementViewModel>();
                services.AddTransient<AnalyticsViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Initialize database
        using (var scope = _host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TableFloDbContext>();
            await context.Database.EnsureCreatedAsync();
            
            // Seed initial data if needed
            await SeedDataAsync(context);
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host.Services.GetRequiredService<T>();
    }

    private async Task SeedDataAsync(TableFloDbContext context)
    {
        // Check if we already have data
        if (await context.Employees.AnyAsync())
            return;

        // Seed comprehensive demo data
        var authService = _host.Services.GetRequiredService<IAuthenticationService>();
        var seeder = new Services.DataSeeder(context, authService);
        await seeder.SeedAsync();
    }
}

