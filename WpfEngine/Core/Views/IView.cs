using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views;

// ========== BASE VIEW INTERFACES ==========

/// <summary>
/// Base interface for all Views (Windows and UserControls)
/// Provides type information for generic methods
/// </summary>
public interface IView
{
    /// <summary>
    /// DataContext as object
    /// </summary>
    object? DataContext { get; set; }
}

/// <summary>
/// View with strongly-typed ViewModel
/// </summary>
public interface IView<TViewModel> : IView
    where TViewModel : IViewModel
{
    /// <summary>
    /// Strongly-typed DataContext
    /// </summary>
    TViewModel? ContextModel { get; set; }
}

/// <summary>
/// View that serves as a shell with changeable content
/// </summary>
public interface IShellView : IView
{
    /// <summary>
    /// Current content ViewModel
    /// </summary>
    object? CurrentContent { get; set; }
}

/// <summary>
/// Shell view with strongly-typed content
/// </summary>
public interface IShellView<TViewModel> : IShellView, IView<TViewModel>
    where TViewModel : IViewModel
{
}