using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views;

// ========== WINDOW INTERFACES ==========

/// <summary>
/// Marker interface for Window views
/// Window inherits from this, so no need to return self
/// </summary>
public interface IWindowView : IView
{
    /// <summary>
    /// Unique identifier for this window instance
    /// </summary>
    Guid WindowId { get; }
}

/// <summary>
/// Window view with strongly-typed ViewModel
/// </summary>
public interface IWindowView<TViewModel> : IWindowView, IView<TViewModel>
    where TViewModel : IViewModel
{
}

/// <summary>
/// Dialog window view
/// </summary>
public interface IDialogView : IWindowView
{
    /// <summary>
    /// Dialog result
    /// </summary>
    bool? DialogResult { get; set; }

    /// <summary>
    /// Type/category of dialog
    /// </summary>
    DialogType DialogType { get; }

    /// <summary>
    /// Application module/section this dialog belongs to
    /// </summary>
    string? AppModule { get; }
}

/// <summary>
/// Dialog window with strongly-typed ViewModel
/// </summary>
public interface IDialogView<TViewModel> : IDialogView, IWindowView<TViewModel>
    where TViewModel : IViewModel
{

}

/// <summary>
/// Workflow host window with navigation shell
/// </summary>
public interface IWorkflowView : IWindowView, IShellView
{
}

/// <summary>
/// Workflow window with strongly-typed host ViewModel
/// </summary>
public interface IWorkflowView<TViewModel> : IWorkflowView, IWindowView<TViewModel>
    where TViewModel : IViewModel
{
}
