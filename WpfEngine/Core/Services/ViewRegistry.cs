using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfEngine.Core.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.Services;

/// <summary>
/// Registry for View mappings
/// Singleton that holds all VM -> View mappings
/// </summary>
public class ViewRegistry : IViewRegistry
{
    private readonly Dictionary<Type, Type> _mappings = new();
    private readonly ILogger<ViewRegistry> _logger;

    public ViewRegistry(ILogger<ViewRegistry> logger)
    {
        _logger = logger;
    }

    // ========== MAPPING METHODS ==========

    public IViewRegistry MapWindow<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : Window, IWindowView
    {
        return RegisterMapping<TViewModel, TWindow>();
    }

    public IViewRegistry MapDialog<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : Window, IDialogView
    {
        return RegisterMapping<TViewModel, TWindow>();
    }

    public IViewRegistry MapControl<TViewModel, TControl>()
        where TViewModel : class
        where TControl : UserControl, IControlView
    {
        return RegisterMapping<TViewModel, TControl>();
    }

    public IViewRegistry MapShell<TViewModel, TShell>()
        where TViewModel : class
        where TShell : Window, IShellView
    {
        return RegisterMapping<TViewModel, TShell>();
    }

    public IViewRegistry RemoveMapping<TViewModel>() where TViewModel : class
    {
        var viewModelType = typeof(TViewModel);
        if (_mappings.Remove(viewModelType))
        {
            _logger.LogInformation("[VIEW_REGISTRY] Removed mapping for {ViewModelType}", viewModelType.Name);
        }
        return this;
    }

    public IViewRegistry Clear()
    {
        var count = _mappings.Count;
        _mappings.Clear();
        _logger.LogInformation("[VIEW_REGISTRY] Cleared all mappings (removed {Count})", count);
        return this;
    }

    // ========== INTERNAL METHODS ==========

    private IViewRegistry RegisterMapping<TViewModel, TView>()
        where TViewModel : class
    {
        var viewModelType = typeof(TViewModel);
        var viewType = typeof(TView);

        if (_mappings.ContainsKey(viewModelType))
        {
            _logger.LogWarning("[VIEW_REGISTRY] Overwriting existing mapping for {ViewModelType}: {OldViewType} -> {NewViewType}",
                viewModelType.Name, _mappings[viewModelType].Name, viewType.Name);
        }

        _mappings[viewModelType] = viewType;
        _logger.LogInformation("[VIEW_REGISTRY] Registered mapping: {ViewModelType} -> {ViewType}",
            viewModelType.Name, viewType.Name);

        return this;
    }

    public bool TryGetViewType(Type viewModelType, out Type viewType)
    {
        return _mappings.TryGetValue(viewModelType, out viewType!);
    }

    /// <summary>
    /// Gets all registered mappings (for diagnostics)
    /// </summary>
    public IReadOnlyDictionary<Type, Type> GetAllMappings()
    {
        return _mappings;
    }
}
