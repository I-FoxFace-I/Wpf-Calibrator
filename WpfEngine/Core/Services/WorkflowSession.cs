using System;
using System.Collections.Generic;
using System.Windows;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Core.Services.Autofac;

/// <summary>
/// Workflow session implementation
/// Manages a session scope and all windows within it
/// </summary>
public class WorkflowSession : IWorkflowSession
{
    private readonly ILifetimeScope _sessionScope;
    private readonly IWindowService _windowService;
    private readonly ILogger<WorkflowSession> _logger;
    private readonly List<Guid> _windowIds = new();
    private bool _disposed;

    internal WorkflowSession(
        Guid sessionId,
        string sessionTag,
        ILifetimeScope sessionScope,
        IWindowService windowService,
        ILogger<WorkflowSession> logger)
    {
        SessionId = sessionId;
        SessionTag = sessionTag;
        _sessionScope = sessionScope;
        _windowService = windowService;
        _logger = logger;

        _logger.LogInformation("[WORKFLOW_SESSION] Created session {SessionId} with tag '{Tag}'", 
            SessionId, SessionTag);
    }

    public Guid SessionId { get; }
    public string SessionTag { get; }
    public bool IsActive => !_disposed;

    public event EventHandler? SessionClosed;

    // ========== OPEN WINDOW ==========

    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        ThrowIfDisposed();
        
        _logger.LogInformation("[WORKFLOW_SESSION] Opening window {ViewModelType} in session {SessionId}",
            typeof(TViewModel).Name, SessionId);

        // Resolve ViewModel and View from session scope, then create window
        var viewModel = _sessionScope.Resolve<TViewModel>();
        var view = _sessionScope.Resolve<IViewLocatorService>().ResolveView<TViewModel>();
        
        view.DataContext = viewModel;
        
        var windowId = Guid.NewGuid();
        _windowIds.Add(windowId);
        
        // Track and show window
        var window = view as Window ?? throw new InvalidOperationException("View is not a Window");
        
        // Subscribe to close
        var vmKey = viewModel.GetVmKey();
        window.Closed += (s, e) => 
        {
            _windowIds.Remove(windowId);
            _logger.LogInformation("[WORKFLOW_SESSION] Window {WindowId} in session {SessionId} closed", 
                windowId, SessionId);
        };
        
        window.Show();
        
        return windowId;
    }

    public Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        ThrowIfDisposed();
        
        _logger.LogInformation("[WORKFLOW_SESSION] Opening window {ViewModelType} with options in session {SessionId}",
            typeof(TViewModel).Name, SessionId);

        // Resolve with TypedParameter from session scope
        var viewModel = _sessionScope.Resolve<TViewModel>(
            new TypedParameter(typeof(TOptions), options));
        var view = _sessionScope.Resolve<IViewLocatorService>().ResolveView<TViewModel>();
        
        view.DataContext = viewModel;
        
        var windowId = Guid.NewGuid();
        _windowIds.Add(windowId);
        
        var window = view as Window ?? throw new InvalidOperationException("View is not a Window");
        
        var vmKey = viewModel.GetVmKey();
        window.Closed += (s, e) => 
        {
            _windowIds.Remove(windowId);
        };
        
        // Initialize if needed
        if (viewModel is IInitializable initializable)
        {
            _ = initializable.InitializeAsync();
        }
        
        window.Show();
        
        return windowId;
    }

    // ========== OPEN CHILD WINDOW ==========

    public Guid OpenChildWindow<TViewModel>(Guid parentWindowId) where TViewModel : IViewModel
    {
        ThrowIfDisposed();
        
        _logger.LogInformation("[WORKFLOW_SESSION] Opening child window {ViewModelType} in session {SessionId}",
            typeof(TViewModel).Name, SessionId);

        // Find parent window
        var parentWindow = FindWindow(parentWindowId);
        if (parentWindow == null)
            throw new InvalidOperationException($"Parent window {parentWindowId} not found");

        var viewModel = _sessionScope.Resolve<TViewModel>();
        var view = _sessionScope.Resolve<IViewLocatorService>().ResolveView<TViewModel>();
        
        view.DataContext = viewModel;
        
        var windowId = Guid.NewGuid();
        _windowIds.Add(windowId);
        
        var window = view as Window ?? throw new InvalidOperationException("View is not a Window");
        window.Owner = parentWindow;
        
        var vmKey = viewModel.GetVmKey();
        window.Closed += (s, e) => _windowIds.Remove(windowId);
        
        if (viewModel is IInitializable initializable)
        {
            _ = initializable.InitializeAsync();
        }
        
        window.Show();
        
        return windowId;
    }

    public Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        ThrowIfDisposed();
        
        _logger.LogInformation("[WORKFLOW_SESSION] Opening child window {ViewModelType} with options in session {SessionId}",
            typeof(TViewModel).Name, SessionId);

        var parentWindow = FindWindow(parentWindowId);
        if (parentWindow == null)
            throw new InvalidOperationException($"Parent window {parentWindowId} not found");

        var viewModel = _sessionScope.Resolve<TViewModel>(
            new TypedParameter(typeof(TOptions), options));
        var view = _sessionScope.Resolve<IViewLocatorService>().ResolveView<TViewModel>();
        
        view.DataContext = viewModel;
        
        var windowId = Guid.NewGuid();
        _windowIds.Add(windowId);
        
        var window = view as Window ?? throw new InvalidOperationException("View is not a Window");
        window.Owner = parentWindow;
        
        if (viewModel is IInitializable initializable)
        {
            _ = initializable.InitializeAsync();
        }
        
        window.Show();
        
        return windowId;
    }

    // ========== SESSION CONTROL ==========

    public void Close()
    {
        if (_disposed) return;

        _logger.LogInformation("[WORKFLOW_SESSION] Closing session {SessionId} with {WindowCount} windows",
            SessionId, _windowIds.Count);

        // Close all windows in this session
        foreach (var windowId in _windowIds.ToList())
        {
            var window = FindWindow(windowId);
            window?.Close();
        }
        
        // Dispose session scope
        _sessionScope?.Dispose();
        
        _disposed = true;
        
        SessionClosed?.Invoke(this, EventArgs.Empty);
        
        _logger.LogInformation("[WORKFLOW_SESSION] Session {SessionId} closed", SessionId);
    }
    
    private Window? FindWindow(Guid windowId)
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is IWindowView windowView && windowView.WindowId == windowId)
                return window;
        }
        return null;
    }

    public void Dispose()
    {
        Close();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException($"WorkflowSession {SessionId}");
    }
}

/// <summary>
/// Factory for creating workflow sessions
/// </summary>
public class WorkflowSessionFactory : IWorkflowSessionFactory
{
    private readonly ILifetimeScope _rootScope;
    private readonly IWindowService _windowService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<WorkflowSessionFactory> _logger;

    public WorkflowSessionFactory(
        ILifetimeScope rootScope,
        IWindowService windowService,
        ILoggerFactory loggerFactory,
        ILogger<WorkflowSessionFactory> logger)
    {
        _rootScope = rootScope;
        _windowService = windowService;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public IWorkflowSession CreateSession(string? sessionTag = null)
    {
        var sessionId = Guid.NewGuid();
        var tag = sessionTag ?? $"workflow-session-{sessionId:N}";
        
        var logger = _loggerFactory.CreateLogger<WorkflowSession>();

        // We need to pass the session to child scopes, but session doesn't exist yet
        // Solution: Create dummy session scope first, then real session with instance registration
        
        WorkflowSession? sessionInstance = null;

        // Create session scope with deferred registration
        var sessionScope = _rootScope.BeginLifetimeScope(tag, builder =>
        {
            // Register a resolver that will return the session when it exists
            builder.Register(c => sessionInstance!)
                   .As<IWorkflowSession>()
                   .InstancePerLifetimeScope()
                   .OnlyIf(reg => sessionInstance != null);
        });

        // Create session instance
        sessionInstance = new WorkflowSession(
            sessionId,
            tag,
            sessionScope,
            _windowService,
            logger);

        _logger.LogInformation("[SESSION_FACTORY] Created workflow session {SessionId} with tag '{Tag}'", 
            sessionId, tag);

        return sessionInstance;
    }
}

