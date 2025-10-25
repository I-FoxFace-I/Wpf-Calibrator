using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Demo.Services;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Workflow Host ViewModel - Shell pattern with session support
/// 
/// ARCHITECTURE:
/// - Resolved from PARENT (root) scope
/// - Creates workflow session on initialization
/// - Has IContentManager for navigation (resolved from WINDOW scope)
/// - Content ViewModels resolved from WINDOW scope (but see session services!)
/// </summary>
public partial class DemoWorkflowHostViewModelRefactored : ShellViewModel
{
    private readonly IWindowService _windowService;
    private readonly Func<Guid, string, IWorkflowSession> _sessionFactory;
    private IWorkflowSession? _workflowSession;
    private Guid _sessionId;

    public DemoWorkflowHostViewModelRefactored(
        IContentManager contentManager,        // From WINDOW scope
        IWindowService windowService,           // From PARENT scope (or window scope)
        Func<Guid, string, IWorkflowSession> sessionFactory,  // Factory from root
        ILogger<DemoWorkflowHostViewModelRefactored> logger) 
        : base(contentManager, windowService, logger)
    {
        _windowService = windowService;
        _sessionFactory = sessionFactory;
        
        Logger.LogInformation("[WORKFLOW_HOST] ViewModel created");
    }

    public override async Task InitializeAsync()
    {
        // 1. Create workflow session
        _sessionId = _windowService.CreateSession("order-workflow");
        _workflowSession = _sessionFactory(_sessionId, "order-workflow");
        
        Logger.LogInformation("[WORKFLOW_HOST] Created workflow session {SessionId}", _sessionId);

        // 2. Navigate to first step
        Logger.LogInformation("[WORKFLOW_HOST] Starting workflow - navigating to Step 1");
        await ContentManager.NavigateToAsync<DemoWorkflowStep1ViewModelRefactored>();
    }

    public override void Dispose()
    {
        if (_workflowSession != null)
        {
            Logger.LogInformation("[WORKFLOW_HOST] Disposing workflow session");
            _workflowSession.CloseSession();
        }
        
        base.Dispose();
    }
}

