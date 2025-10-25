using WpfEngine.Core.ViewModels;
using System.Windows;

namespace WpfEngine.Core.Views;

// ========== USERCONTROL INTERFACES ==========

/// <summary>
/// Marker interface for UserControl views
/// </summary>
public interface IControlView : IView
{
}

/// <summary>
/// UserControl with strongly-typed ViewModel
/// </summary>
public interface IControlView<TViewModel> : IControlView, IView<TViewModel>
    where TViewModel : IViewModel
{
}

/// <summary>
/// UserControl that serves as shell with changeable content
/// </summary>
public interface IShellControlView : IControlView, IShellView
{
}

/// <summary>
/// Detail view - displays entity details (can be Window or UserControl)
/// </summary>
public interface IDetailView : IView
{
    /// <summary>
    /// Indicates if view is in read-only mode
    /// </summary>
    bool IsReadOnly { get; set; }
}

/// <summary>
/// Detail view with strongly-typed ViewModel
/// </summary>
public interface IDetailView<TViewModel> : IDetailView, IView<TViewModel>
    where TViewModel : IViewModel
{
}

/// <summary>
/// Edit view - allows editing entity (can be Window or UserControl)
/// </summary>
public interface IEditView : IDetailView
{
    /// <summary>
    /// Validates input before save
    /// </summary>
    bool Validate();
}

/// <summary>
/// Edit view with strongly-typed ViewModel
/// </summary>
public interface IEditView<TViewModel> : IEditView, IDetailView<TViewModel>
    where TViewModel : IViewModel
{
}

/// <summary>
/// Detail view with selection capability (can be Window or UserControl)
/// </summary>
public interface IDetailSelectView : IDetailView
{
    /// <summary>
    /// Indicates if item is selected
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Selection confirmed event/command
    /// </summary>
    event System.EventHandler? SelectionConfirmed;
}

/// <summary>
/// Detail select view with strongly-typed ViewModel
/// </summary>
public interface IDetailSelectView<TViewModel> : IDetailSelectView, IDetailView<TViewModel>
    where TViewModel : IViewModel
{
}
