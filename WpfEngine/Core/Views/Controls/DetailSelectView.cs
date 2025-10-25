using System.Windows;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Controls;

// ========== DETAIL SELECT VIEW ==========

/// <summary>
/// Detail view with selection capability
/// Used for selecting from list with detail preview
/// </summary>
public abstract class DetailSelectView : DetailView, IDetailSelectView
{
    protected DetailSelectView()
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
            typeof(DetailSelectView),
            new PropertyMetadata(false, OnIsSelectedChanged));

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DetailSelectView view && (bool)e.NewValue)
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
/// Detail select view with strongly-typed ViewModel
/// </summary>
public abstract class DetailSelectView<TViewModel> : DetailSelectView, IDetailSelectView<TViewModel>
    where TViewModel : IViewModel
{
    protected DetailSelectView()
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}

