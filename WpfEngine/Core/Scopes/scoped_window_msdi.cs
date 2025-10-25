using System;
using System.Windows;
using WpfEngine.Core.Scopes;
using WpfEngine.Core.Views;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.Views.Windows.MicrosoftDI;

/// <summary>
/// Base window with own scope context for Microsoft DI
/// Automatically creates and disposes scope on window lifecycle
/// </summary>
public abstract class ScopedWindowBase : Window, IWindow
{
    protected readonly ILogger Logger;
    private bool _disposed;

    protected ScopedWindowBase(ILogger logger, IScopeContext scopeContext)
    {
        Logger = logger;
        ScopeContext = scopeContext;

        Logger.LogInformation("[SCOPED_WINDOW] Created {WindowType} with scope {ScopeId} (Tag: {ScopeTag})",
            GetType().Name, scopeContext.ScopeId, scopeContext.ScopeTag);

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// Scope context for this window
    /// </summary>
    public IScopeContext ScopeContext { get; }

    Window IWindow.Window => this;

    object? IView.DataContext
    {
        get => DataContext;
        set => DataContext = value;
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} closed, disposing scope {ScopeId}",
            GetType().Name, ScopeContext.ScopeId);
        
        Dispose();
    }

    protected virtual void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[SCOPED_WINDOW] Disposing {WindowType} and scope {ScopeId}",
            GetType().Name, ScopeContext.ScopeId);

        ScopeContext?.Dispose();
        _disposed = true;

        Logger.LogInformation("[SCOPED_WINDOW] {WindowType} disposed", GetType().Name);
    }
}

/// <summary>
/// Scoped window with strongly-typed ViewModel
/// </summary>
public abstract class ScopedWindowBase<TViewModel> : ScopedWindowBase, IWindowView<TViewModel>
    where TViewModel : class
{
    protected ScopedWindowBase(ILogger logger, IScopeContext scopeContext)
        : base(logger, scopeContext)
    {
    }

    public new TViewModel? ContextModel
    {
        get => base.DataContext as TViewModel;
        set => base.DataContext = value;
    }
}

/// <summary>
/// Scoped dialog window
/// </summary>
public abstract class ScopedDialogWindowBase : ScopedWindowBase, IDialogWindow
{
    protected ScopedDialogWindowBase(ILogger logger, IScopeContext scopeContext)
        : base(logger, scopeContext)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}

/// <summary>
/// Scoped dialog window with strongly-typed ViewModel
/// </summary>
public abstract class ScopedDialogWindowBase<TViewModel> : ScopedWindowBase<TViewModel>, IDialogView<TViewModel>
    where TViewModel : class
{
    protected ScopedDialogWindowBase(ILogger logger, IScopeContext scopeContext)
        : base(logger, scopeContext)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
}

/// <summary>
/// Scoped workflow window
/// </summary>
public abstract class ScopedWorkflowWindowBase : ScopedWindowBase, IWorkflowView
{
    protected ScopedWorkflowWindowBase(ILogger logger, IScopeContext scopeContext, string? workflowName = null)
        : base(logger, scopeContext)
    {
    }

    public object? CurrentContent
    {
        get => GetValue(CurrentContentProperty);
        set => SetValue(CurrentContentProperty, value);
    }

    public static readonly DependencyProperty CurrentContentProperty =
        DependencyProperty.Register(
            nameof(CurrentContent),
            typeof(object),
            typeof(ScopedWorkflowWindowBase),
            new PropertyMetadata(null));
}

/// <summary>
/// Scoped workflow window with strongly-typed ViewModel
/// </summary>
public abstract class ScopedWorkflowWindowBase<TViewModel> : ScopedWorkflowWindowBase, IWorkflowView<TViewModel>
    where TViewModel : class
{
    protected ScopedWorkflowWindowBase(ILogger logger, IScopeContext scopeContext, string? workflowName = null)
        : base(logger, scopeContext, workflowName)
    {
    }

    public new TViewModel? ContextModel
    {
        get => base.DataContext as TViewModel;
        set => base.DataContext = value;
    }
}
