using System;
using System.Windows;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;

namespace WpfEngine.Core.Services;

/// <summary>
/// Service for resolving Views from ViewModels
/// Configuration-based mapping instead of convention
/// </summary>
public interface IViewLocatorService
{
    /// <summary>
    /// Resolves View for ViewModel type
    /// </summary>
    IView ResolveView<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Resolves View for ViewModel type (non-generic)
    /// </summary>
    IView ResolveView(Type viewModelType);

    /// <summary>
    /// Checks if mapping exists for ViewModel
    /// </summary>
    bool HasMapping<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Checks if mapping exists for ViewModel type
    /// </summary>
    bool HasMapping(Type viewModelType);
}

/// <summary>
/// Registry for configuring View mappings
/// </summary>
public interface IViewRegistry
{
    /// <summary>
    /// Maps ViewModel to Window
    /// </summary>
    IViewRegistry MapWindow<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : Window, IWindowView;

    /// <summary>
    /// Maps ViewModel to Dialog Window
    /// </summary>
    IViewRegistry MapDialog<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : Window, IDialogView;

    /// <summary>
    /// Maps ViewModel to UserControl (for workflow steps, content views)
    /// </summary>
    IViewRegistry MapControl<TViewModel, TControl>()
        where TViewModel : class
        where TControl : System.Windows.Controls.UserControl, IControlView;

    /// <summary>
    /// Maps ViewModel to Shell Window
    /// </summary>
    IViewRegistry MapShell<TViewModel, TShell>()
        where TViewModel : class
        where TShell : Window, IShellView;

    /// <summary>
    /// Removes mapping for ViewModel
    /// </summary>
    IViewRegistry RemoveMapping<TViewModel>() where TViewModel : class;

    /// <summary>
    /// Clears all mappings
    /// </summary>
    IViewRegistry Clear();
}

/// <summary>
/// Configuration class for View mappings
/// Implement this and register in DI container
/// </summary>
public abstract class ViewMappingConfiguration
{
    /// <summary>
    /// Configure View mappings
    /// Called during application startup
    /// </summary>
    public abstract void Configure(IViewRegistry registry);
}
