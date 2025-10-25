using Autofac;
using Autofac.Extensions.DependencyInjection;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Views;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;
using WpfEngine.Demo.Views;
using WpfEngie.Demo.Configuration;
using WpfEngine.Demo.Configuration;

namespace WpfEngine.Demo;

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
            containerBuilder.RegisterModule<CoreServicesModule>();
            containerBuilder.RegisterViewMappingConfiguration<ViewMappingConfiguration>();
            containerBuilder.RegisterModule<ViewsModule>();
            containerBuilder.RegisterModule<DemoModule>();
            // 6. Register View Mapping Configuration (NEW!)

            // 6. Build container
            _container = containerBuilder.Build();

            // 11. Configure View mappings (NEW!)
            _container.ConfigureViewMappings();

            // 7. Initialize database
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

    protected override void OnExit(ExitEventArgs e)
    {
        _container?.Dispose();
        base.OnExit(e);
    }
}