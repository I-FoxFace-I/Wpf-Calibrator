//using WpfEngine.Core.Services;
//using WpfEngine.Services.MicrosoftDI;
//using Microsoft.Extensions.DependencyInjection;

//namespace WpfEngine.Configuration;

///// <summary>
///// Extension methods for registering Core services with Microsoft DI
///// </summary>
//public static class CoreServicesExtensions
//{
//    /// <summary>
//    /// Adds Core services to Microsoft DI container
//    /// </summary>
//    public static IServiceCollection AddCoreServices(this IServiceCollection services)
//    {
//        // ========== VIEW REGISTRY (Singleton) ==========
//        // ViewRegistry is singleton - holds all VM->View mappings
//        services.AddSingleton<ViewRegistry>();
//        services.AddSingleton<IViewRegistry>(sp => sp.GetRequiredService<ViewRegistry>());

//        // ========== VIEW LOCATOR (Scoped) ==========
//        // ViewLocator needs scope to resolve views
//        services.AddScoped<IViewLocatorService, Services.MicrosoftDI.ViewLocatorService>();

//        // ========== VIEWMODEL FACTORY (Scoped) ==========
//        // Factory uses current scope to create ViewModels
//        services.AddScoped<IViewModelFactory, ViewModelFactory>();

//        // ========== NAVIGATION SERVICE (Scoped) ==========
//        // Each window/scope has its own navigation service
//        services.AddScoped<INavigationService, NavigationService>();

//        // ========== WINDOW SERVICE (Scoped) ==========
//        // Each scope manages its own windows
//        services.AddScoped<IWindowService, WindowService>();

//        // ========== DIALOG SERVICE (Scoped) ==========
//        // Dialog service per scope
//        services.AddScoped<IDialogService, DialogService>();

//        return services;
//    }

//    /// <summary>
//    /// Registers ViewMappingConfiguration
//    /// </summary>
//    public static IServiceCollection AddViewMappingConfiguration<TConfiguration>(
//        this IServiceCollection services)
//        where TConfiguration : ViewMappingConfiguration
//    {
//        services.AddSingleton<ViewMappingConfiguration, TConfiguration>();
//        return services;
//    }

//    /// <summary>
//    /// Configures View mappings using registered ViewMappingConfiguration instances
//    /// Call this after building ServiceProvider
//    /// </summary>
//    public static IServiceProvider ConfigureViewMappings(this IServiceProvider serviceProvider)
//    {
//        var registry = serviceProvider.GetRequiredService<IViewRegistry>();

//        // Resolve all ViewMappingConfiguration instances
//        var configurations = serviceProvider.GetServices<ViewMappingConfiguration>();

//        foreach (var config in configurations)
//        {
//            config.Configure(registry);
//        }

//        return serviceProvider;
//    }
//}

///// <summary>
///// Example usage in App.xaml.cs:
///// 
///// protected override void OnStartup(StartupEventArgs e)
///// {
/////     var services = new ServiceCollection();
/////     
/////     // Add logging
/////     services.AddLogging(config => config.AddConsole().AddDebug());
/////     
/////     // Add Core services
/////     services.AddCoreServices();
/////     
/////     // Add View mapping configuration
/////     services.AddViewMappingConfiguration<ExampleViewMappingConfiguration>();
/////     
/////     // Register your ViewModels (Transient - new instance each time)
/////     services.AddTransient<MainViewModel>();
/////     services.AddTransient<CustomerListViewModel>();
/////     services.AddTransient<CustomerDetailViewModel>();
/////     // ... etc
/////     
/////     // Register your Views (Transient)
/////     services.AddTransient<MainWindow>();
/////     services.AddTransient<CustomerListWindow>();
/////     services.AddTransient<CustomerDetailWindow>();
/////     // ... etc
/////     
/////     // Build service provider
/////     var serviceProvider = services.BuildServiceProvider();
/////     
/////     // Configure View mappings
/////     serviceProvider.ConfigureViewMappings();
/////     
/////     // Open main window
/////     var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
/////     mainWindow.Show();
///// }
///// </summary>
