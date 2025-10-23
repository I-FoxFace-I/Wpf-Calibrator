using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacEnhancedWpfDemo.Configuration;
using AutofacEnhancedWpfDemo.Data;
using AutofacEnhancedWpfDemo.Data.Demo;
using AutofacEnhancedWpfDemo.ViewModels;
using AutofacEnhancedWpfDemo.Views;
using AutofacWpfDemo.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;

namespace AutofacEnhancedWpfDemo;

/// <summary>
/// Application entry point with Autofac + MS.DI integration
/// </summary>
public partial class App : System.Windows.Application
{
    private IContainer? _container;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // 1. Create MS.DI ServiceCollection
            var services = new ServiceCollection();

            // 2. Add MS.DI services (Logging, Configuration, etc.)
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            //services.AddDbContextFactory<AppDbContext>(options =>
            //     options.UseSqlite("Data Source=DemoDb")
            //            .EnableSensitiveDataLogging());

            // 3. Create Autofac ContainerBuilder
            var containerBuilder = new ContainerBuilder();

            // 4. Populate Autofac with MS.DI services
            containerBuilder.Populate(services);

            // 5. Register Autofac modules
            containerBuilder.RegisterModule<DataModule>();
            containerBuilder.RegisterModule<ServicesModule>();
            containerBuilder.RegisterModule<ViewsModule>();
            containerBuilder.RegisterModule<AdvancedDemoModule>();

            // 6. Build container
            _container = containerBuilder.Build();

            // 7. Initialize database
            InitializeDatabase();
            InitializeDemoDatabase();

            // 8. Open main window
            var mainWindow = _container.Resolve<Views.MainWindow>();

            // Resolve MainMenuViewModel and set as DataContext
            var mainViewModel = _container.Resolve<ViewModels.MainViewModel>();
            mainWindow.DataContext = mainViewModel;

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Application startup failed:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    private void InitializeDemoDatabase()
    {
        try
        {
            var contextFactory = _container!.Resolve<IDbContextFactory<DemoDbContext>>();

            using var context = contextFactory.CreateDbContext();

            context.Database.EnsureCreated();
            DemoDbSeeder.Seed(context);

            var logger = _container.Resolve<ILogger<App>>();
            logger.LogInformation("Demo database initialized successfully");
        }
        catch (Exception ex)
        {
            var logger = _container!.Resolve<ILogger>();
            logger.LogError(ex, "Failed to initialize demo database");
            throw;
        }
    }

    private void InitializeDatabase()
    {
        try
        {
            var contextFactory = _container!.Resolve<IDbContextFactory<AppDbContext>>();

            using var context = contextFactory.CreateDbContext();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed initial data
            DbSeeder.Seed(context);

            var logger = _container.Resolve<ILogger<App>>();
            logger.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            var logger = _container!.Resolve<ILogger<App>>();
            logger.LogError(ex, "Failed to initialize database");
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _container?.Dispose();
        base.OnExit(e);
    }
}