using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Windows;

/// <summary>
/// Window with its own child lifetime scope
/// Automatically disposes scope on close
/// </summary>
public abstract class ScopedWindow : BaseWindow
{
    private readonly ILifetimeScope _scope;
    private bool _scopeDisposed;

    protected ScopedWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        object? scopeTag = null) : base(logger)
    {
        _scope = parentScope.BeginLifetimeScope(scopeTag ?? GetType().Name);
        Logger.LogInformation("[{WindowType}] Created with own child scope (tag: {Tag})",
            GetType().Name, scopeTag ?? GetType().Name);

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// This window's scope for resolving services
    /// </summary>
    public ILifetimeScope Scope => _scope;

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        Logger.LogInformation("[{WindowType}] Closed, disposing scope", GetType().Name);
        DisposeScope();
    }

    private void DisposeScope()
    {
        if (_scopeDisposed) return;

        Logger.LogInformation("[{WindowType}] Disposing scope - cascades to all children",
            GetType().Name);

        _scope?.Dispose();
        _scopeDisposed = true;
    }
}



/// <summary>
/// Scoped window with strongly-typed ViewModel
/// </summary>
public abstract class ScopedWindow<TViewModel> : ScopedWindow, IWindowView<TViewModel>
    where TViewModel : IViewModel
{
    protected ScopedWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        object? scopeTag = null) : base(parentScope, logger, scopeTag)
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}


