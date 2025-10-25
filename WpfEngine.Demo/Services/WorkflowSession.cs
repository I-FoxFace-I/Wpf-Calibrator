using System;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Workflow session implementation
/// Wraps WindowService and provides session-scoped operations
/// </summary>
public class WorkflowSession : IWorkflowSession
{
    private readonly Guid _sessionId;
    private readonly string _sessionName;
    private readonly IWindowService _windowService;
    private readonly ILogger<WorkflowSession> _logger;

    public WorkflowSession(
        Guid sessionId,
        string sessionName,
        IWindowService windowService,
        ILogger<WorkflowSession> logger)
    {
        _sessionId = sessionId;
        _sessionName = sessionName;
        _windowService = windowService;
        _logger = logger;
        
        _logger.LogInformation("[WORKFLOW_SESSION] Session '{SessionName}' (ID: {SessionId}) created",
            sessionName, sessionId);
    }

    public Guid SessionId => _sessionId;
    public string SessionName => _sessionName;

    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
    {
        _logger.LogInformation("[WORKFLOW_SESSION] Opening {ViewModelType} in session {SessionId}",
            typeof(TViewModel).Name, _sessionId);
        
        return _windowService.OpenWindowInSession<TViewModel>(_sessionId);
    }

    public Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        _logger.LogInformation("[WORKFLOW_SESSION] Opening {ViewModelType} in session {SessionId} with options",
            typeof(TViewModel).Name, _sessionId);
        
        return _windowService.OpenWindowInSession<TViewModel, TOptions>(_sessionId, options);
    }

    public Guid OpenChildWindow<TViewModel>(Guid parentWindowId) where TViewModel : IViewModel
    {
        _logger.LogInformation("[WORKFLOW_SESSION] Opening child {ViewModelType} in session {SessionId}",
            typeof(TViewModel).Name, _sessionId);
        
        return _windowService.OpenChildWindow<TViewModel>(parentWindowId);
    }

    public Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters
    {
        _logger.LogInformation("[WORKFLOW_SESSION] Opening child {ViewModelType} in session {SessionId} with options",
            typeof(TViewModel).Name, _sessionId);
        
        return _windowService.OpenChildWindow<TViewModel, TOptions>(parentWindowId, options);
    }

    public void CloseWindow(Guid windowId)
    {
        _logger.LogInformation("[WORKFLOW_SESSION] Closing window {WindowId} in session {SessionId}",
            windowId, _sessionId);
        
        _windowService.Close(windowId);
    }

    public void CloseSession()
    {
        _logger.LogInformation("[WORKFLOW_SESSION] Closing entire session {SessionId}", _sessionId);
        _windowService.CloseSession(_sessionId);
    }
}

