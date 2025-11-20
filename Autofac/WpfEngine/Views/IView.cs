using Autofac;
using WpfEngine.ViewModels;

namespace WpfEngine.Views;

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
/// Interface for windows that manage their own lifetime scope
/// Each window generates its own unique ID upon creation
/// </summary>
public interface IScopedView : IWindowView
{
    /// <summary>
    /// Window's unique ID - generated automatically when window is created
    /// This is immutable and set once during construction
    /// </summary>
    Guid AssignedWindowId { get; set; }
}