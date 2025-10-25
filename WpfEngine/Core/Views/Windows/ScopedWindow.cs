using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Windows;

/// <summary>
/// Window with its own child lifetime scope
/// Automatically disposes scope on close
/// Creates child scope from parent scope (can be root or session scope)
/// </summary>
public abstract class ScopedWindow : BaseWindow
{
    private readonly ILifetimeScope _scope;
    private bool _scopeDisposed;

    /// <summary>
    /// Creates window with child scope from parent
    /// </summary>
    /// <param name="parentScope">Parent scope (can be root, session, or another window scope)</param>
    /// <param name="logger">Logger</param>
    /// <param name="scopeTag">Optional tag for this scope</param>
    protected ScopedWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        object? scopeTag = null) : base(logger)
    {
        // Create child scope with optional tag
        _scope = parentScope.BeginLifetimeScope(scopeTag ?? GetType().Name);
        
        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} created with scope (Tag: {Tag}, ParentTag: {ParentTag})",
            GetType().Name, 
            scopeTag ?? GetType().Name,
            parentScope.Tag?.ToString() ?? "root");

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// This window's scope for resolving services
    /// </summary>
    public ILifetimeScope Scope => _scope;

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} closed, disposing scope", GetType().Name);
        DisposeScope();
    }

    private void DisposeScope()
    {
        if (_scopeDisposed) return;

        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} disposing scope - cascades to all children",
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


