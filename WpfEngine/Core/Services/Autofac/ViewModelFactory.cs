using System;
using Autofac;
using Autofac.Core;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// ViewModel factory using Autofac TypedParameter injection
/// </summary>
public class ViewModelFactory : IViewModelFactory
{
    private readonly ILifetimeScope _scope;
    private readonly ILogger<ViewModelFactory> _logger;

    public ViewModelFactory(
        ILifetimeScope scope,
        ILogger<ViewModelFactory> logger)
    {
        _scope = scope;
        _logger = logger;
    }

    // ========== CREATE WITHOUT SCOPE ==========

    public TViewModel Create<TViewModel>() where TViewModel : IViewModel
    {
        _logger.LogDebug("[VM_FACTORY] Creating {ViewModelType}", typeof(TViewModel).Name);

        var viewModel = _scope.Resolve<TViewModel>();

        _logger.LogInformation("[VM_FACTORY] Created {ViewModelType} (ID: {Id})",
            typeof(TViewModel).Name,
            viewModel is IViewModel vm ? vm.Id : Guid.Empty);

        return viewModel;
    }

    public TViewModel Create<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        _logger.LogDebug("[VM_FACTORY] Creating {ViewModelType} with options (CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name, options.CorrelationId);

        // Use TypedParameter to inject options
        var viewModel = _scope.Resolve<TViewModel>(new TypedParameter(typeof(TOptions), options));

        _logger.LogInformation("[VM_FACTORY] Created {ViewModelType} with options (ID: {Id}, CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name,
            viewModel is IViewModel vm ? vm.Id : Guid.Empty,
            options.CorrelationId);

        return viewModel;
    }

    // ========== CREATE WITH SCOPE ==========

    public TViewModel CreateScoped<TViewModel>(ILifetimeScope scope)
        where TViewModel : class
    {
        _logger.LogDebug("[VM_FACTORY] Creating scoped {ViewModelType}", typeof(TViewModel).Name);

        var viewModel = scope.Resolve<TViewModel>();

        _logger.LogInformation("[VM_FACTORY] Created scoped {ViewModelType} (ID: {Id})",
            typeof(TViewModel).Name,
            viewModel is IViewModel vm ? vm.Id : Guid.Empty);

        return viewModel;
    }

    public TViewModel CreateScoped<TViewModel, TOptions>(ILifetimeScope scope, TOptions options)
        where TViewModel : class
        where TOptions : ViewModelOptions
    {
        _logger.LogDebug("[VM_FACTORY] Creating scoped {ViewModelType} with options (CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name, options.CorrelationId);

        var viewModel = scope.Resolve<TViewModel>(new TypedParameter(typeof(TOptions), options));

        _logger.LogInformation("[VM_FACTORY] Created scoped {ViewModelType} with options (ID: {Id}, CorrelationId: {CorrelationId})",
            typeof(TViewModel).Name,
            viewModel is IViewModel vm ? vm.Id : Guid.Empty,
            options.CorrelationId);

        return viewModel;
    }

    // ========== NON-GENERIC CREATE ==========

    public object Create(Type viewModelType, ViewModelOptions? options = null)
    {
        _logger.LogDebug("[VM_FACTORY] Creating {ViewModelType} (non-generic)", viewModelType.Name);

        object viewModel;

        if (options != null)
        {
            // Find options type
            var optionsType = options.GetType();
            viewModel = _scope.Resolve(viewModelType, new TypedParameter(optionsType, options));

            _logger.LogInformation("[VM_FACTORY] Created {ViewModelType} with options (CorrelationId: {CorrelationId})",
                viewModelType.Name, options.CorrelationId);
        }
        else
        {
            viewModel = _scope.Resolve(viewModelType);

            _logger.LogInformation("[VM_FACTORY] Created {ViewModelType}",
                viewModelType.Name);
        }

        return viewModel;
    }

    public object CreateScoped(Type viewModelType, ILifetimeScope scope, ViewModelOptions? options = null)
    {
        _logger.LogDebug("[VM_FACTORY] Creating scoped {ViewModelType} (non-generic)", viewModelType.Name);

        object viewModel;

        if (options != null)
        {
            var optionsType = options.GetType();
            viewModel = scope.Resolve(viewModelType, new TypedParameter(optionsType, options));

            _logger.LogInformation("[VM_FACTORY] Created scoped {ViewModelType} with options (CorrelationId: {CorrelationId})",
                viewModelType.Name, options.CorrelationId);
        }
        else
        {
            viewModel = scope.Resolve(viewModelType);

            _logger.LogInformation("[VM_FACTORY] Created scoped {ViewModelType}",
                viewModelType.Name);
        }

        return viewModel;
    }
}
