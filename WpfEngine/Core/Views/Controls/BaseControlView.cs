using System.Windows.Controls;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Controls;

// ========== BASE CONTROL VIEW ==========

/// <summary>
/// Base UserControl for all views
/// </summary>
public abstract class BaseControlView : UserControl, IControlView
{
    protected BaseControlView()
    {
    }
}

/// <summary>
/// Base UserControl with strongly-typed ViewModel
/// </summary>
public abstract class BaseControlView<TViewModel> : BaseControlView, IControlView<TViewModel>
    where TViewModel : BaseViewModel
{
    protected BaseControlView()
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}