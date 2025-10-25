using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Autofac;

namespace WpfEngine.Services.MicrosoftDI;

/// <summary>
/// ViewModel factory using Microsoft DI
/// Uses post-construction initialization pattern
/// Never uses Activator - all objects resolved from DI
/// </summary>
public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ViewModelFactory> _logger;

    public ViewModelFactory(
        IServiceProvider serviceProvider,
        ILogger<ViewModelFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // ========== CREATE WITHOUT SCOPE ==========

    public TViewModel Create<TViewModel>() where TViewModel : class
    {
        _logger.LogDebug("[VM_FACTORY_MSDI] Creating {ViewModelType}", typeof(TViewModel).Name);

        // Resolve from DI container (all dependencies injected)
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        _logger.LogInformation("[VM_FACTORY_MSDI] Created {ViewModelType} (ID: {Id})",
            typeof(TViewModel).Name,
            viewModel is IViewModel vm ? vm.Id : Guid.Empty);

        // Note: InitializeAsync must be called by consumer
        return viewModel;
    }

    public TViewModel Create<TViewModel, TOptions>(TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        _logger.LogDebug("[VM_FACTORY_MSDI] Creating {ViewModelType} with options (CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name, options.CorrelationId);

        // Resolve from DI container (NO options in constructor)
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        // Initialize with options AFTER construction
        if (viewModel is IInitializable<TOptions> initializable)
        {
            _logger.LogDebug("[VM_FACTORY_MSDI] Initializing {ViewModelType} with options", typeof(TViewModel).Name);
            
            // We need to initialize synchronously for factory pattern
            // Consumer should call InitializeAsync again if needed
            if (viewModel is IViewModel<TOptions> vmWithOptions)
            {
                // Set options property directly for IViewModel<TOptions>
                // This is a workaround - proper initialization happens via InitializeAsync
                var optionsProperty = viewModel.GetType().GetProperty(nameof(IViewModel<TOptions>.Options));
                optionsProperty?.SetValue(viewModel, options);
            }

            _logger.LogInformation("[VM_FACTORY_MSDI] Created {ViewModelType} with options (ID: {Id}, CorrelationId: {CorrelationId})",
                typeof(TViewModel).Name,
                viewModel is IViewModel vm ? vm.Id : Guid.Empty,
                options.CorrelationId);
        }
        else
        {
            _logger.LogWarning("[VM_FACTORY_MSDI] ViewModel {ViewModelType} does not implement IInitializable<{OptionsType}>",
                typeof(TViewModel).Name, typeof(TOptions).Name);
        }

        return viewModel;
    }

    // ========== CREATE WITH SCOPE ==========

    public TViewModel CreateScoped<TViewModel>(ILifetimeScope scope)
        where TViewModel : class
    {
        throw new NotSupportedException(
            "CreateScoped with ILifetimeScope is only supported with Autofac. " +
            "Use IServiceScope from Microsoft DI instead.");
    }

    public TViewModel CreateScoped<TViewModel, TOptions>(ILifetimeScope scope, TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        throw new NotSupportedException(
            "CreateScoped with ILifetimeScope is only supported with Autofac. " +
            "Use IServiceScope from Microsoft DI instead.");
    }

    // ========== MSDI SPECIFIC - WITH SERVICE SCOPE ==========

    /// <summary>
    /// Creates ViewModel within Microsoft DI scope
    /// </summary>
    public TViewModel CreateScoped<TViewModel>(IServiceScope scope)
        where TViewModel : class
    {
        _logger.LogDebug("[VM_FACTORY_MSDI] Creating scoped {ViewModelType}", typeof(TViewModel).Name);

        var viewModel = scope.ServiceProvider.GetRequiredService<TViewModel>();

        _logger.LogInformation("[VM_FACTORY_MSDI] Created scoped {ViewModelType} (ID: {Id})",
            typeof(TViewModel).Name,
            viewModel is IViewModel vm ? vm.Id : Guid.Empty);

        return viewModel;
    }

    /// <summary>
    /// Creates ViewModel with options within Microsoft DI scope
    /// </summary>
    public TViewModel CreateScoped<TViewModel, TOptions>(IServiceScope scope, TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        _logger.LogDebug("[VM_FACTORY_MSDI] Creating scoped {ViewModelType} with options (CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name, options.CorrelationId);

        var viewModel = scope.ServiceProvider.GetRequiredService<TViewModel>();

        // Set options property
        if (viewModel is IViewModel<TOptions> vmWithOptions)
        {
            var optionsProperty = viewModel.GetType().GetProperty(nameof(IViewModel<TOptions>.Options));
            optionsProperty?.SetValue(viewModel, options);
        }

        _logger.LogInformation("[VM_FACTORY_MSDI] Created scoped {ViewModelType} with options (ID: {Id}, CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name,
            viewModel is IViewModel vm ? vm.Id : Guid.Empty,
            options.CorrelationId);

        return viewModel;
    }

    // ========== NON-GENERIC CREATE ==========

    public object Create(Type viewModelType, ViewModelOptions? options = null)
    {
        _logger.LogDebug("[VM_FACTORY_MSDI] Creating {ViewModelType} (non-generic)", viewModelType.Name);

        var viewModel = _serviceProvider.GetRequiredService(viewModelType);

        if (options != null)
        {
            // Set options via reflection
            var optionsProperty = viewModelType.GetProperty("Options");
            optionsProperty?.SetValue(viewModel, options);

            _logger.LogInformation("[VM_FACTORY_MSDI] Created {ViewModelType} with options (CorrelationId: {CorrelationId})",
                viewModelType.Name, options.CorrelationId);
        }
        else
        {
            _logger.LogInformation("[VM_FACTORY_MSDI] Created {ViewModelType}", viewModelType.Name);
        }

        return viewModel;
    }

    public object CreateScoped(Type viewModelType, ILifetimeScope scope, ViewModelOptions? options = null)
    {
        throw new NotSupportedException(
            "CreateScoped with ILifetimeScope is only supported with Autofac. " +
            "Use IServiceScope from Microsoft DI instead.");
    }

    /// <summary>
    /// Creates ViewModel with Microsoft DI scope (non-generic)
    /// </summary>
    public object CreateScoped(Type viewModelType, IServiceScope scope, ViewModelOptions? options = null)
    {
        _logger.LogDebug("[VM_FACTORY_MSDI] Creating scoped {ViewModelType} (non-generic)", viewModelType.Name);

        var viewModel = scope.ServiceProvider.GetRequiredService(viewModelType);

        if (options != null)
        {
            var optionsProperty = viewModelType.GetProperty("Options");
            optionsProperty?.SetValue(viewModel, options);

            _logger.LogInformation("[VM_FACTORY_MSDI] Created scoped {ViewModelType} with options (CorrelationId: {CorrelationId})",
                viewModelType.Name, options.CorrelationId);
        }
        else
        {
            _logger.LogInformation("[VM_FACTORY_MSDI] Created scoped {ViewModelType}", viewModelType.Name);
        }

        return viewModel;
    }
}
