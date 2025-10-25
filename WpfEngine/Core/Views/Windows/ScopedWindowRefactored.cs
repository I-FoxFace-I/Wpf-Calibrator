using System;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Scopes;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Windows;

/// <summary>
/// Window with hierarchical scope support
/// 
/// ARCHITECTURE:
/// - Window creates its own child scope from parent scope
/// - ViewModel is resolved from PARENT scope (can access shared services)
/// - If window is Shell, its ContentManager resolves content from WINDOW scope
/// 
/// SCOPE HIERARCHY:
/// Parent Scope (e.g. WorkflowSession)
///   ├─ Shared Services (InstancePerMatchingLifetimeScope)
///   │
///   └─ Window.Scope (child of parent)
///        ├─ Window-specific services (IContentManager, etc.)
///        └─ Content ViewModels (resolved by ContentManager)
/// 
/// This allows:
/// - ViewModel sees shared services from parent
/// - Window has its own isolated services
/// - Content is managed within window scope
/// </summary>
public abstract class ScopedWindowNew : BaseWindow
{
    private readonly ILifetimeScope _windowScope;
    private readonly ScopeTag _scopeTag;
    private bool _scopeDisposed;

    /// <summary>
    /// Creates window with child scope from parent
    /// </summary>
    /// <param name="parentScope">Parent scope (root, session, or another window)</param>
    /// <param name="logger">Logger</param>
    /// <param name="scopeTag">Typed scope tag for this window</param>
    protected ScopedWindowNew(
        ILifetimeScope parentScope,
        ILogger logger,
        ScopeTag scopeTag) : base(logger)
    {
        _scopeTag = scopeTag;
        
        // Create child scope with tag
        _windowScope = parentScope.BeginLifetimeScope(scopeTag.ToString());
        
        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} created (Tag: {Tag}, Parent: {ParentTag})",
            GetType().Name, 
            scopeTag.ToShortString(),
            parentScope.Tag?.ToString() ?? "root");

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// This window's scope for resolving window-specific services
    /// Content ViewModels should be resolved from this scope
    /// </summary>
    public ILifetimeScope WindowScope => _windowScope;

    /// <summary>
    /// Scope tag for this window
    /// </summary>
    public ScopeTag ScopeTag => _scopeTag;

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} closed, disposing scope", GetType().Name);
        DisposeScope();
    }

    private void DisposeScope()
    {
        if (_scopeDisposed) return;

        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} disposing scope (Tag: {Tag})",
            GetType().Name, _scopeTag.ToShortString());

        _windowScope?.Dispose();
        _scopeDisposed = true;
    }
}

/// <summary>
/// Scoped window with strongly-typed ViewModel
/// </summary>
public abstract class ScopedWindowNew<TViewModel> : ScopedWindowNew, IWindowView<TViewModel>
    where TViewModel : IViewModel
{
    protected ScopedWindowNew(
        ILifetimeScope parentScope,
        ILogger logger,
        ScopeTag scopeTag) : base(parentScope, logger, scopeTag)
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel?)DataContext;
        set => DataContext = value;
    }
}

