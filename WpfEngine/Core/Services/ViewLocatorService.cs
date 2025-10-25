using Autofac;
using WpfEngine.Core.Services;
using WpfEngine.Core.Views;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Services;

/// <summary>
/// View locator service with configuration-based mapping
/// </summary>
public class ViewLocatorService : IViewLocatorService
{
    private readonly ILifetimeScope _scope;
    private readonly ViewRegistry _registry;
    private readonly ILogger<ViewLocatorService> _logger;

    public ViewLocatorService(
        ILifetimeScope scope,
        ViewRegistry registry,
        ILogger<ViewLocatorService> logger)
    {
        _scope = scope;
        _registry = registry;
        _logger = logger;
    }

    public IView ResolveView<TViewModel>() where TViewModel : IViewModel
    {
        return ResolveView(typeof(TViewModel));
    }

    public IView ResolveView(Type viewModelType)
    {
        _logger.LogDebug("[VIEW_LOCATOR] Resolving view for {ViewModelType}", viewModelType.Name);

        if (!_registry.TryGetViewType(viewModelType, out var viewType))
        {
            var message = $"No view mapping found for ViewModel: {viewModelType.Name}. " +
                         $"Register mapping in ViewMappingConfiguration.";
            _logger.LogError("[VIEW_LOCATOR] {Message}", message);
            throw new InvalidOperationException(message);
        }

        _logger.LogDebug("[VIEW_LOCATOR] Found mapping: {ViewModelType} -> {ViewType}",
            viewModelType.Name, viewType.Name);

        // Resolve view from container
        var view = _scope.Resolve(viewType) as IView;

        if (view == null)
        {
            var message = $"Failed to resolve view {viewType.Name} from container.";
            _logger.LogError("[VIEW_LOCATOR] {Message}", message);
            throw new InvalidOperationException(message);
        }

        _logger.LogInformation("[VIEW_LOCATOR] Resolved {ViewType} for {ViewModelType}",
            viewType.Name, viewModelType.Name);

        return view;
    }

    public bool HasMapping<TViewModel>() where TViewModel : IViewModel
    {
        return HasMapping(typeof(TViewModel));
    }

    public bool HasMapping(Type viewModelType)
    {
        return _registry.TryGetViewType(viewModelType, out _);
    }
}
