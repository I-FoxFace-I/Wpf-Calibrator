using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Parameters;
using WpfEngine.Enums;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Window manager implementation using the new IScopeManager session system
/// </summary>
public class ScopedWindowManager : WindowManager, IScopedWindowManager
{
    private readonly IScopeManager _scopeManager;

    public ScopedWindowManager(
        ILifetimeScope rootScope,
        IViewRegistry registry,
        IWindowTracker windowTracker,
        IScopeManager scopeManager,
        ILogger<ScopedWindowManager> logger)
        : base(rootScope, registry, windowTracker, logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _logger.LogInformation("[SCOPED_WINDOW_MANAGER] Service initialized with IScopeManager");
    }

    // ========== SESSION/SCOPE HANDLING ==========
    
    protected override ILifetimeScope? GetSessionScope(Guid sessionId)
    {
        return _scopeManager.GetSession(sessionId)?.Scope;
    }

    protected override Guid? GetWindowSessionId(Guid windowId)
    {
        return _windowTracker.WithMetadata(windowId, metadata => metadata.SessionId, null);
    }

    // ========== WINDOW OPENING (primary implementation) ==========
    
    public override Guid OpenWindow<TViewModel>()
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Opening root window for {ViewModelType}", 
            typeof(TViewModel).Name);
        
        return OpenWindowCore<TViewModel>(_rootScope, null, null);
    }

    public override Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Opening root window for {ViewModelType} with parameters", 
            typeof(TViewModel).Name);
        
        return OpenWindowCore<TViewModel, TParameters>(_rootScope, parameters, null, null);
    }

    public override Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Opening child window for {ViewModelType}", 
            typeof(TViewModel).Name);

        Guid? sessionId = null;
        ILifetimeScope? parentScope = null;

        _windowTracker.WithMetadata(parentWindowId, meta =>
        {
            sessionId = meta.SessionId;
            parentScope = meta.WindowScope;
        });

        return OpenWindowCore<TViewModel, BaseModelParameters>(parentScope ?? _rootScope, null, parentWindowId, sessionId);
    }

    public override Guid OpenChildWindow<TViewModel, TParameters>(
        Guid parentWindowId, 
        TParameters parameters)
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Opening child window for {ViewModelType} with parameters", 
            typeof(TViewModel).Name);

        Guid? sessionId = null;
        ILifetimeScope? parentScope = null;

        _windowTracker.WithMetadata(parentWindowId, meta =>
        {
            sessionId = meta.SessionId;
            parentScope = meta.WindowScope;
        });
        
        return OpenWindowCore<TViewModel, TParameters>(parentScope ?? _rootScope, parameters, parentWindowId, sessionId);
    }

    public override Guid OpenWindowInSession<TViewModel>(Guid sessionId)
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Opening window in session {SessionId}", sessionId);
        
        var session = _scopeManager.GetSession(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        return OpenWindowCore<TViewModel>(session.Scope, null, sessionId);
    }

    public override Guid OpenWindowInSession<TViewModel, TParameters>(
        Guid sessionId, 
        TParameters parameters)
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Opening window in session {SessionId} with parameters", 
            sessionId);
        
        var session = _scopeManager.GetSession(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        return OpenWindowCore<TViewModel, TParameters>(session.Scope, parameters, null, sessionId);
    }

    // ========== WINDOW CLOSING (primary implementation) ==========
    
    public override void CloseWindow(Guid windowId)
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Closing window {WindowId}", windowId);
        
        // Close all children first
        CloseAllChildWindows(windowId);
        
        // Close the window
        CloseWindowCore(windowId);
    }

    public override void CloseAllChildWindows(Guid parentId)
    {
        _logger.LogDebug("[SCOPED_WINDOW_MANAGER] Closing all children of {ParentId}", parentId);
        
        var children = _windowTracker.GetChildWindows(parentId).ToList();
        var errors = new List<string>();

        foreach (var childId in children)
        {
            try
            {
                CloseWindow(childId);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to close child {childId}: {ex.Message}");
                _logger.LogError(ex, "[SCOPED_WINDOW_MANAGER] Error closing child {ChildId}", childId);
            }
        }

        if (errors.Any())
        {
            throw new InvalidOperationException(string.Join("; ", errors));
        }
    }

    public override void CloseAllSessionWindows(Guid sessionId)
    {
        _logger.LogInformation("[SCOPED_WINDOW_MANAGER] Closing all windows in session {SessionId}", 
            sessionId);
        
        var windowIds = _windowTracker.GetSessionWindows(sessionId).ToList();
        var errors = new List<string>();

        foreach (var windowId in windowIds)
        {
            try
            {
                CloseWindow(windowId);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to close window {windowId}: {ex.Message}");
                _logger.LogError(ex, "[SCOPED_WINDOW_MANAGER] Error closing window {WindowId}", windowId);
            }
        }

        _logger.LogInformation("[SCOPED_WINDOW_MANAGER] Closed {Count} windows in session {SessionId}", 
            windowIds.Count, sessionId);

        if (errors.Any())
        {
            throw new InvalidOperationException(string.Join("; ", errors));
        }
    }

    public Task<DialogResult> ShowDialogAsync<TViewModel>(Guid? ownerWindowId, DialogModality modality = DialogModality.WindowModal)
        where TViewModel : class, IViewModel, IDialogViewModel
    {

        ILifetimeScope? parentScope = null;

        _windowTracker.WithMetadata(ownerWindowId ?? Guid.Empty, meta =>
        {
            parentScope = meta.WindowScope;
        });

        return OpenDialogCore<TViewModel, DialogResult, bool>(ownerWindowId, parentScope ?? _rootScope, null, modality);
    }

    public Task<DialogResult> ShowDialogAsync<TViewModel, TParameters>(Guid? ownerWindowId, TParameters parameters, DialogModality modality = DialogModality.WindowModal)
        where TViewModel : class, IViewModel, IDialogViewModel<TParameters>
        where TParameters : IViewModelParameters
    {

        ILifetimeScope? parentScope = null;

        _windowTracker.WithMetadata(ownerWindowId ?? Guid.Empty, meta =>
        {
            parentScope = meta.WindowScope;
        });

        return OpenDialogCore<TViewModel, DialogResult, bool>(ownerWindowId, parentScope ?? _rootScope, parameters, modality);
    }

    // ===================== TYPED =====================
    public Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>(Guid? ownerWindowId, DialogModality modality = DialogModality.WindowModal)
        where TViewModel : class, IViewModel, IDialogViewModel, IResultDialogViewModel<TResult>
        where TResult : class
    {
        ILifetimeScope? parentScope = null;

        _windowTracker.WithMetadata(ownerWindowId ?? Guid.Empty, meta =>
        {
            parentScope = meta.WindowScope;
        });

        return OpenDialogCore<TViewModel, DialogResult<TResult>, TResult>(ownerWindowId, parentScope ?? _rootScope, null, modality);
    }

    public Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TParameters, TResult>(Guid? ownerWindowId, TParameters parameters, DialogModality modality = DialogModality.WindowModal)
        where TViewModel : class, IViewModel, IDialogViewModel<TParameters>, IResultDialogViewModel<TResult>
        where TParameters : IViewModelParameters
        where TResult : class
    {
        ILifetimeScope? parentScope = null;

        _windowTracker.WithMetadata(ownerWindowId ?? Guid.Empty, meta =>
        {
            parentScope = meta.WindowScope;
        });

        return OpenDialogCore<TViewModel, DialogResult<TResult>, TResult>(ownerWindowId, parentScope ?? _rootScope, parameters, modality);
    }
}

