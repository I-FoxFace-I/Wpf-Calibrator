using System;
using System.Windows;
using System.Windows.Controls;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Controls;


// ========== SHELL CONTROL VIEW ==========

/// <summary>
/// UserControl that serves as shell with changeable content
/// </summary>
public abstract class ShellControlView : BaseControlView, IShellControlView
{
    protected ShellControlView()
    {
    }

    /// <summary>
    /// Current content ViewModel
    /// </summary>
    public object? CurrentContent
    {
        get => GetValue(CurrentContentProperty);
        set => SetValue(CurrentContentProperty, value);
    }

    public static readonly DependencyProperty CurrentContentProperty =
        DependencyProperty.Register(
            nameof(CurrentContent),
            typeof(object),
            typeof(ShellControlView),
            new PropertyMetadata(null));
}

/// <summary>
/// Shell control with strongly-typed host ViewModel
/// </summary>
public abstract class ShellControlView<TViewModel> : ShellControlView, IControlView<TViewModel>
    where TViewModel : BaseViewModel
{
    protected ShellControlView()
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}



