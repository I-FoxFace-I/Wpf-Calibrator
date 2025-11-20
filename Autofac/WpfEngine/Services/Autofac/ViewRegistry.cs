using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Configuration;
using System.Collections.ObjectModel;
using Accessibility;
using WpfEngine.ViewModels;
using WpfEngine.Views;
using WpfEngine.Services;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Registry for View mappings
/// Singleton that holds all VM -> View mappings
/// </summary>
public class ViewRegistry : IViewRegistry
{
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
    private readonly IDictionary<Type, Type> _mappings = new Dictionary<Type, Type>();

    private readonly ILogger<ViewRegistry> _logger;

    public ViewRegistry(ILogger<ViewRegistry> logger)
    {
        _logger = logger;
    }

    // ========== MAPPING METHODS ==========

    public Type ResolveViewType<TViewModel>() where TViewModel : IViewModel
    {

        if (TryGetViewType(typeof(TViewModel), out var viewType))
        {
            return viewType;
        }
        else
        {
            throw new InvalidOperationException($"View mapping for ViewModel: {typeof(TViewModel).Name} is not registered.");
        }
    }

    public bool TryResolveViewType<TViewModel>([NotNullWhen(true)] out Type? viewType) where TViewModel : IViewModel
    {
        return TryGetViewType(typeof(TViewModel), out viewType);
    }

    public IViewRegistry MapWindow<TViewModel, TWindow>()
        where TViewModel : IViewModel
        where TWindow : Window, IWindowView
    {
        return RegisterMapping<TViewModel, TWindow>();
    }

    public IViewRegistry MapDialog<TViewModel, TWindow>()
        where TViewModel : IViewModel
        where TWindow : Window, IDialogView
    {
        return RegisterMapping<TViewModel, TWindow>();
    }

    public IViewRegistry MapControl<TViewModel, TControl>()
        where TViewModel : IViewModel
        where TControl : UserControl, IControlView
    {
        return RegisterMapping<TViewModel, TControl>();
    }

    public IViewRegistry MapShell<TViewModel, TShell>()
        where TViewModel : IViewModel
        where TShell : Window, IShellView
    {
        return RegisterMapping<TViewModel, TShell>();
    }

    public IViewRegistry RemoveMapping<TViewModel>() where TViewModel : IViewModel
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
        where TViewModel : IViewModel
    {


        var viewType = typeof(TView);
        var viewModelType = typeof(TViewModel);


        _rwLock.EnterUpgradeableReadLock();

        try
        {
            if (_mappings.ContainsKey(viewModelType))
            {
                _logger.LogWarning("[VIEW_REGISTRY] Overwriting existing mapping for {ViewModelType}: {OldViewType} -> {NewViewType}",
                    viewModelType.Name, _mappings[viewModelType].Name, viewType.Name);
            }

            _rwLock.EnterWriteLock();

            try
            {
                _mappings[viewModelType] = viewType;
                _logger.LogInformation("[VIEW_REGISTRY] Registered mapping: {ViewModelType} -> {ViewType}",
                    viewModelType.Name, viewType.Name);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        finally
        {
            _rwLock.ExitUpgradeableReadLock();
        }




        return this;
    }

    public bool TryGetViewType(Type viewModelType, [NotNullWhen(true)] out Type? viewType)
    {
        _rwLock.EnterReadLock();

        try
        {
            if (_mappings.TryGetValue(viewModelType, out var target) && target is Type targetType)
            {
                viewType = targetType;

                return true;
            }
            else
            {
                viewType = null;

                return false;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError("[VIEW_REGISTRY] Error when tried to get mapping View for {ViewModelType} type", viewModelType.Name);

            viewType = null;

            return false;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets all registered mappings (for diagnostics)
    /// </summary>
    public IReadOnlyDictionary<Type, Type> GetAllMappings()
    {
        _rwLock.EnterReadLock();
        try
        {
            return _mappings.ToDictionary().AsReadOnly();
        }
        catch
        {
            return ReadOnlyDictionary<Type, Type>.Empty;
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }


}
