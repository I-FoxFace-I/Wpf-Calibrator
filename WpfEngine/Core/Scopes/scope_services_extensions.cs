using System;
using WpfEngine.Core.Scopes;
using WpfEngine.Core.Services;
using WpfEngine.Services.MicrosoftDI.Scopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.Themes;

namespace WpfEngine.Configuration;

/// <summary>
/// Extension methods for registering scope-based services
/// </summary>
public static class ScopeServicesExtensions
{
    /// <summary>
    /// Adds hierarchical scope support to service collection
    /// </summary>
    public static IServiceCollection AddScopeSupport(
        this IServiceCollection services,
        Action<IScopeModuleCollection>? configureModules = null)
    {
        // Register scope infrastructure
        services.AddSingleton<IScopeContextFactory, ScopeContextFactory>();
        services.AddSingleton<IScopeModuleCollection, ScopeModuleCollection>();

        // Register ViewLocator (needed by WindowService)
        services.AddSingleton<ViewRegistry>();
        services.AddSingleton<IViewRegistry>(sp => sp.GetRequiredService<ViewRegistry>());
        services.AddScoped<IViewLocatorService, Services.MicrosoftDI.ViewLocatorService>();

        // Register scoped WindowService (uses scope contexts)
        services.AddScoped<IWindowService, WindowServiceScoped>();

        // Register Navigation and Dialog services
        services.AddScoped<IViewModelFactory, Services.MicrosoftDI.ViewModelFactory>();
        services.AddScoped<INavigationService, NavigationService>();
        services.AddScoped<IDialogService, Services.MicrosoftDI.DialogService>();

        // Configure modules if provided
        if (configureModules != null)
        {
            var moduleCollection = new ScopeModuleCollection(
                null!); // Logger will be resolved from DI
            
            configureModules(moduleCollection);
            
            // Apply modules to service collection
            moduleCollection.ApplyModules(services);
        }

        return services;
    }

    /// <summary>
    /// Initializes root scope context after building service provider
    /// Must be called after BuildServiceProvider()
    /// </summary>
    public static IScopeContext InitializeRootScope(
        this IServiceProvider serviceProvider,
        string rootScopeTag = "root")
    {
        var scopeFactory = serviceProvider.GetRequiredService<IScopeContextFactory>();
        var rootScope = scopeFactory.CreateRootScope(rootScopeTag);

        // Notify modules about root scope creation
        var moduleCollection = serviceProvider.GetRequiredService<IScopeModuleCollection>();
        moduleCollection.NotifyScopeCreated(rootScope);

        return rootScope;
    }

    /// <summary>
    /// Registers scope module
    /// </summary>
    public static IServiceCollection AddScopeModule<TModule>(this IServiceCollection services)
        where TModule : class, IScopeModule, new()
    {
        services.AddSingleton<IScopeModule, TModule>();
        return services;
    }
}

/// <summary>
/// Example usage in App.xaml.cs:
/// 
/// protected override void OnStartup(StartupEventArgs e)
/// {
///     var services = new ServiceCollection();
///     
///     services.AddLogging(config => config.AddConsole());
///     
///     // Add scope support with modules
///     services.AddScopeSupport(modules =>
///     {
///         modules.RegisterModule(new WorkflowScopeModule());
///         modules.RegisterModule(new DataContextScopeModule());
///         modules.RegisterModule(new CachingScopeModule());
///     });
///     
///     // Or register modules individually
///     services.AddScopeModule<WorkflowScopeModule>();
///     
///     // Add View mappings
///     services.AddViewMappingConfiguration<MyViewMappingConfiguration>();
///     
///     // Register ViewModels and Views
///     services.AddTransient<MainViewModel>();
///     services.AddTransient<MainWindow>();
///     // ... etc
///     
///     // Build service provider
///     var serviceProvider = services.BuildServiceProvider();
///     
///     // Configure View mappings
///     serviceProvider.ConfigureViewMappings();
///     
///     // Initialize root scope
///     var rootScope = serviceProvider.InitializeRootScope("application-root");
///     
///     // Store root scope for later use
///     _rootScope = rootScope;
///     
///     // Open main window
///     var windowService = rootScope.ServiceProvider.GetRequiredService<IWindowService>();
///     windowService.OpenWindow<MainViewModel>();
/// }
/// 
/// protected override void OnExit(ExitEventArgs e)
/// {
///     _rootScope?.Dispose(); // Disposes entire hierarchy
///     base.OnExit(e);
/// }
/// </summary>
