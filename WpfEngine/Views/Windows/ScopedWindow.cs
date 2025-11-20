using Autofac;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using WpfEngine.ViewModels;
using WpfEngine.Views;

namespace WpfEngine.Views.Windows;


/// <summary>
/// Base class for windows with automatic scope management
/// Implements IScopedView pattern:
/// - Generates unique WindowId automatically upon creation
/// - Creates and manages its own child scope
/// - Automatically registers itself in scope for IWindowContext resolution
/// - Disposes scope when window closes
/// </summary>
public abstract class ScopedWindow : BaseWindow, IScopedView
{
    private bool _disposed;
    public override Guid WindowId => AssignedWindowId;
    public Guid AssignedWindowId { get; set; } = Guid.NewGuid();

    protected ScopedWindow(ILogger logger) : base(logger)
    {
        Loaded += OnLoaded;
        Closed += OnClosedDetach;
    }


    private void OnLoaded(object? s, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
    }


    private void OnClosedDetach(object? s, EventArgs e)
    {
        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} closed (WindowId: {WindowId})", GetType().Name, AssignedWindowId);
        DataContext = null;
        Closed -= OnClosedDetach;
        Loaded -= OnLoaded;
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_disposed) return;

            DataContext = null;
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Strongly-typed scoped window with ViewModel
/// </summary>
public abstract class ScopedWindow<TViewModel> : ScopedWindow, IView<TViewModel>
    where TViewModel : IViewModel
{
    protected ScopedWindow(ILogger logger)
        : base(logger)
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}
