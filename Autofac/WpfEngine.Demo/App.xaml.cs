using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using WpfEngine.Configuration;
using WpfEngine.Demo.Configuration;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using WpfEngine.Services;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

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

            services.AddDbContextPool<DemoDbContext>(options =>
                 options.UseSqlite("Data Source=DemoDb_V2.db")
                        .EnableSensitiveDataLogging());

            // 3. Create Autofac ContainerBuilder
            var containerBuilder = new ContainerBuilder();

            // 4. Populate Autofac with MS.DI services
            containerBuilder.Populate(services);

            // 5. Register Autofac modules
            containerBuilder.RegisterModule<CoreServicesModule>();
            containerBuilder.RegisterViewMappingConfiguration<ViewMappingConfiguration>();
            containerBuilder.RegisterModule<ViewsModule>();
            
            containerBuilder.RegisterModule<DemoModule>();
            // Repository pattern modules (alternative to CQRS)
            containerBuilder.RegisterModule<RepositoryModule>();
            containerBuilder.RegisterModule<RepositoryViewModelsModule>();
            containerBuilder.RegisterModule<RepositoryViewsModule>();


            // 6. Build container
            _container = containerBuilder.Build();

            // 7. Configure View mappings
            _container.ConfigureViewMappings();

            // 8. Initialize database
            InitializeDemoDatabase();

            // 9. Open main window using IWindowService (which is GlobalWindowService singleton)
            var windowService = _container.Resolve<IWindowManager>();
            windowService.OpenWindow<ViewModels.Repository.MainViewModel>();


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

            if (_container != null)
            {
                var logger = _container.Resolve<ILogger<App>>();
                logger.LogInformation("Demo database initialized successfully");
            }
        }
        catch (Exception ex)
        {
            if (_container != null)
            {
                var logger = _container.Resolve<ILogger<App>>();
                logger.LogError(ex, "Failed to initialize demo database");
            }
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _container?.Dispose();
        base.OnExit(e);
    }
}