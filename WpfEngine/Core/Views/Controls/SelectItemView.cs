using System.Windows;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Controls;

// ========== DETAIL SELECT VIEW (can be UserControl or Window) ==========

/// <summary>
/// Detail view with selection capability
/// Used for selecting from list with detail preview
/// </summary>
public abstract class SelectItemView : DetailView, IDetailSelectView
{
    protected SelectItemView()
    {
    }

    /// <summary>
    /// Selection state
    /// </summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(SelectItemView),
            new PropertyMetadata(false, OnIsSelectedChanged));

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SelectItemView view && (bool)e.NewValue)
        {
            view.OnSelectionConfirmed();
        }
    }

    /// <summary>
    /// Selection confirmed event
    /// </summary>
    public event EventHandler? SelectionConfirmed;

    protected virtual void OnSelectionConfirmed()
    {
        SelectionConfirmed?.Invoke(this, EventArgs.Empty);
    }
}


/// <summary>
/// Detail select control view with strongly-typed ViewModel
/// </summary>
public abstract class SelectItemView<TViewModel> : SelectItemView, IDetailSelectView<TViewModel>
    where TViewModel : BaseViewModel
{
    protected SelectItemView()
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}
