using System.IO;
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
        // Global exception handler
        DispatcherUnhandledException += (sender, e) =>
        {
            MessageBox.Show($"An unexpected error occurred:\n\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Database - store in ProgramData for shared access across all users
                // This is the professional approach for enterprise casino operations
                var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var tableFloFolder = Path.Combine(programDataPath, "TableFlo");
                
                try
                {
                    Directory.CreateDirectory(tableFloFolder); // Ensure folder exists
                }
                catch (UnauthorizedAccessException)
                {
                    // Fallback to user's AppData if ProgramData is not writable
                    programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    tableFloFolder = Path.Combine(programDataPath, "TableFlo");
                    Directory.CreateDirectory(tableFloFolder);
                }
                
                var dbPath = Path.Combine(tableFloFolder, "tableflo.db");
                
                services.AddDbContext<TableFloDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));

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
                services.AddScoped<DealerManagementViewModel>(); // Scoped to maintain state
                services.AddTransient<TableManagementViewModel>();
                services.AddTransient<AnalyticsViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Get database path for display (match the logic from App constructor)
        var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var tableFloFolder = Path.Combine(programDataPath, "TableFlo");
        
        // Check if we have access to ProgramData, otherwise use AppData
        if (!Directory.Exists(tableFloFolder))
        {
            try
            {
                Directory.CreateDirectory(tableFloFolder);
            }
            catch (UnauthorizedAccessException)
            {
                programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                tableFloFolder = Path.Combine(programDataPath, "TableFlo");
            }
        }
        
        var dbPath = Path.Combine(tableFloFolder, "tableflo.db");

        // Initialize database BEFORE showing any windows
        try
        {
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TableFloDbContext>();
                
                // Ensure database exists
                var isNewDatabase = !File.Exists(dbPath);
                await context.Database.EnsureCreatedAsync();
                
                // Seed initial data if needed
                await SeedDataAsync(context);
                
                // Show success message on first run
                if (isNewDatabase)
                {
                    MessageBox.Show($"✅ Database created successfully!\n\nLocation: {dbPath}\n\nDemo credentials:\nEmployee #: ADMIN001\nPassword: admin123",
                        "TableFlo - First Run", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization failed: {ex.Message}\n\nDatabase path: {dbPath}",
                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
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
        {
            // Data already exists, no need to seed
            return;
        }

        // Seed comprehensive demo data
        try
        {
            var authService = _host.Services.GetRequiredService<IAuthenticationService>();
            var seeder = new Services.DataSeeder(context, authService);
            await seeder.SeedAsync();
            
            // Verify seeding was successful
            var employeeCount = await context.Employees.CountAsync();
            if (employeeCount == 0)
            {
                throw new Exception("Data seeding completed but no employees were created.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to seed demo data: {ex.Message}\n\nThe app may not function properly.",
                "Seeding Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            throw;
        }
    }
}

