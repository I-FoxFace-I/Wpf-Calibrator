using System.Windows;
using WpfEngine.ViewModels;
using WpfEngine.ViewModels.Base;
using WpfEngine.Views;

namespace WpfEngine.Views.Controls;

/// <summary>
/// UserControl for displaying entity details (read-only)
/// </summary>
public abstract class DetailView : BaseControlView, IDetailView
{
    protected DetailView()
    {
    }

    /// <summary>
    /// Read-only mode
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(DetailView),
            new PropertyMetadata(true)); // Default: read-only
}

/// <summary>
/// Detail control view with strongly-typed ViewModel
/// </summary>
public abstract class DetailView<TViewModel> : DetailView, IDetailView<TViewModel>
    where TViewModel : BaseViewModel
{
    protected DetailView()
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}