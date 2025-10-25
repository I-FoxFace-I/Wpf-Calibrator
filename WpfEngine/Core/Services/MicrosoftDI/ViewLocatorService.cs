using System;
using WpfEngine.Core.Services;
using WpfEngine.Core.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services.MicrosoftDI;

/// <summary>
/// View locator service with configuration-based mapping for Microsoft DI
/// </summary>
public class ViewLocatorService : IViewLocatorService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ViewRegistry _registry;
    private readonly ILogger<ViewLocatorService> _logger;

    public ViewLocatorService(
        IServiceProvider serviceProvider,
        ViewRegistry registry,
        ILogger<ViewLocatorService> logger)
    {
        _serviceProvider = serviceProvider;
        _registry = registry;
        _logger = logger;
    }

    public IView ResolveView<TViewModel>() where TViewModel : class
    {
        return ResolveView(typeof(TViewModel));
    }

    public IView ResolveView(Type viewModelType)
    {
        _logger.LogDebug("[VIEW_LOCATOR_MSDI] Resolving view for {ViewModelType}", viewModelType.Name);

        if (!_registry.TryGetViewType(viewModelType, out var viewType))
        {
            var message = $"No view mapping found for ViewModel: {viewModelType.Name}. " +
                         $"Register mapping in ViewMappingConfiguration.";
            _logger.LogError("[VIEW_LOCATOR_MSDI] {Message}", message);
            throw new InvalidOperationException(message);
        }

        _logger.LogDebug("[VIEW_LOCATOR_MSDI] Found mapping: {ViewModelType} -> {ViewType}",
            viewModelType.Name, viewType.Name);

        // Resolve view from service provider
        var view = _serviceProvider.GetRequiredService(viewType) as IView;

        if (view == null)
        {
            var message = $"Failed to resolve view {viewType.Name} from service provider.";
            _logger.LogError("[VIEW_LOCATOR_MSDI] {Message}", message);
            throw new InvalidOperationException(message);
        }

        _logger.LogInformation("[VIEW_LOCATOR_MSDI] Resolved {ViewType} for {ViewModelType}",
            viewType.Name, viewModelType.Name);

        return view;
    }

    public bool HasMapping<TViewModel>() where TViewModel : class
    {
        return HasMapping(typeof(TViewModel));
    }

    public bool HasMapping(Type viewModelType)
    {
        return _registry.TryGetViewType(viewModelType, out _);
    }
}
