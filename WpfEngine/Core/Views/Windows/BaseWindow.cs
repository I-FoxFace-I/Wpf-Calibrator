using Microsoft.Extensions.Logging;
using System.Windows;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Windows;

/// <summary>
/// Base window without scope management
/// </summary>
public abstract class BaseWindow : Window, IWindowView
{
    protected readonly ILogger Logger;

    protected BaseWindow(ILogger logger)
    {
        Logger = logger;
        WindowId = Guid.NewGuid();
        Logger.LogDebug("[{WindowType}] Window created with ID {WindowId}", GetType().Name, WindowId);
    }

    /// <summary>
    /// Unique identifier for this window instance
    /// </summary>
    public Guid WindowId { get; }
}

/// <summary>
/// Base window with strongly-typed ViewModel
/// </summary>
public abstract class BaseWindow<TViewModel> : BaseWindow, IWindowView<TViewModel>
    where TViewModel : IViewModel
{
    protected BaseWindow(ILogger logger) : base(logger)
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}


