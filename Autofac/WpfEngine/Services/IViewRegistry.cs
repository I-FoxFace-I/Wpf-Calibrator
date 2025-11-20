using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using WpfEngine.ViewModels;
using WpfEngine.Views;

namespace WpfEngine.Services;

/// <summary>
/// Registry for configuring View mappings
/// </summary>
public interface IViewRegistry
{
    /// <summary>
    /// Resolves View type for ViewModel type
    /// </summary>
    Type ResolveViewType<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Resolves View type for ViewModel type
    /// </summary>
    bool TryResolveViewType<TViewModel>([NotNullWhen(true)] out Type? viewType) where TViewModel : IViewModel;

    bool TryGetViewType(Type viewModelType, [NotNullWhen(true)] out Type? viewType);
    /// <summary>
    /// Maps ViewModel to Window
    /// </summary>
    IViewRegistry MapWindow<TViewModel, TWindow>()
        where TViewModel : IViewModel
        where TWindow : Window, IWindowView;

    /// <summary>
    /// Maps ViewModel to Dialog Window
    /// </summary>
    IViewRegistry MapDialog<TViewModel, TWindow>()
        where TViewModel : IViewModel
        where TWindow : Window, IDialogView;

    /// <summary>
    /// Maps ViewModel to UserControl (for workflow steps, content views)
    /// </summary>
    IViewRegistry MapControl<TViewModel, TControl>()
        where TViewModel : IViewModel
        where TControl : System.Windows.Controls.UserControl, IControlView;

    /// <summary>
    /// Maps ViewModel to Shell Window
    /// </summary>
    IViewRegistry MapShell<TViewModel, TShell>()
        where TViewModel : IViewModel
        where TShell : Window, IShellView;

    /// <summary>
    /// Removes mapping for ViewModel
    /// </summary>
    IViewRegistry RemoveMapping<TViewModel>() where TViewModel : IViewModel;

    /// <summary>
    /// Clears all mappings
    /// </summary>
    IViewRegistry Clear();
}
