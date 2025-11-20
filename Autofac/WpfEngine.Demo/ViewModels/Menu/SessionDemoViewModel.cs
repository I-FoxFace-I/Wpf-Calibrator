using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Sessions;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Services;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Extensions;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels.Menu;

/// <summary>
/// Demo ViewModel showing usage of new IScopeManager session system
/// Demonstrates:
/// - Creating workflow sessions
/// - Creating database sessions
/// - Creating child sessions
/// - Using Fluent API for session operations
/// </summary>
public partial class SessionDemoViewModel : BaseViewModel
{
    private readonly IScopeManager _scopeManager;
    private readonly IScopedWindowManager _windowManager;
    private readonly IWindowContext _windowContext;

    public SessionDemoViewModel(
        IScopeManager scopeManager,
        IScopedWindowManager windowManager,
        IWindowContext windowContext,
        ILogger<SessionDemoViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager;
        _windowManager = windowManager;
        _windowContext = windowContext;
        Logger.LogInformation("SessionDemoViewModel created");
    }

    // ========== WORKFLOW SESSION EXAMPLES ==========

    [RelayCommand]
    private void CreateWorkflowSession()
    {
        Logger.LogInformation("Creating workflow session using new IScopeManager");

        // Example 1: Simple workflow session creation
        var workflowSession = _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .Build();

        Logger.LogInformation("Workflow session created: {SessionId}", workflowSession.SessionId);

        // Open window in the workflow session
        _windowManager.OpenWindowInSession<WorkflowHostViewModelRefactored>(workflowSession.SessionId);
    }

    [RelayCommand]
    private void CreateWorkflowSessionWithServices()
    {
        Logger.LogInformation("Creating workflow session with service configuration");

        // Example 2: Workflow session - services are automatically available via DI
        var workflowSession = _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .Build();

        // Services registered with InstancePerMatchingLifetimeScope are automatically available
        var orderService = workflowSession.Resolve<IOrderBuilderService>();
        Logger.LogInformation("OrderBuilderService resolved in workflow session: {SessionId}", workflowSession.SessionId);
    }

    // ========== DATABASE SESSION EXAMPLES ==========

    [RelayCommand]
    private async Task CreateDatabaseSessionAsync()
    {
        Logger.LogInformation("Creating database session for data operations");

        // Example 3: Database session for data operations
        using (var session = _scopeManager
            .CreateDatabaseSession("demo-db")
            .Build())
        {
            // Perform database operations
            // Session automatically handles SaveChanges and Rollback
            Logger.LogInformation("Executing database operations in session: {SessionId}", session.SessionId);
            
            // Example: Save changes
            await session.SaveChangesAsync();
        }
    }

    // ========== CHILD SESSION EXAMPLES ==========

    [RelayCommand]
    private void CreateChildSession()
    {
        Logger.LogInformation("Creating child session from parent workflow session");

        // Example 4: Create parent session, then child session
        var parentSession = _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .Build();

        Logger.LogInformation("Parent session created: {SessionId}", parentSession.SessionId);

        // Create child session for specific operation
        var childSession = parentSession
            .CreateChild(ScopeTag.Custom("order-detail"))
            .Build();

        Logger.LogInformation("Child session created: {ChildSessionId} under parent {ParentSessionId}", 
            childSession.SessionId, parentSession.SessionId);

        // Child session can access services from parent session
        // Close child session (parent remains open)
        childSession.Dispose();
    }

    // ========== FLUENT API EXAMPLES ==========

    [RelayCommand]
    private async Task FluentApiExampleAsync()
    {
        Logger.LogInformation("Demonstrating Fluent API for session operations");

        // Example 5: Fluent API with service resolution
        await _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .WithService<IOrderBuilderService>()
            .WithService<WorkflowState>()
            .ExecuteAsync(async (orderService, workflowState) =>
            {
                // Both services are automatically resolved and available
                Logger.LogInformation("OrderBuilderService and WorkflowState available in session");
                
                // Perform operations...
                // Session is automatically disposed after ExecuteAsync completes
                await Task.CompletedTask;
            });
    }

    // ========== WINDOW OPERATIONS IN SESSION ==========

    [RelayCommand]
    private void OpenWindowInSession()
    {
        Logger.LogInformation("Opening window in workflow session");

        // Example 6: Create session and open window
        var session = _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .Build();

        // Open window in the session
        var windowId = _windowManager.OpenWindowInSession<WorkflowHostViewModelRefactored>(session.SessionId);
        
        Logger.LogInformation("Window {WindowId} opened in session {SessionId}", windowId, session.SessionId);
    }

    // ========== SESSION LIFECYCLE ==========

    [RelayCommand]
    private void DemonstrateSessionLifecycle()
    {
        Logger.LogInformation("Demonstrating session lifecycle");

        // Example 7: Manual session management
        var session = _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .Build();

        Logger.LogInformation("Session created: {SessionId}, Tag: {Tag}", 
            session.SessionId, session.Tag);

        // Use session...
        var orderService = session.Resolve<IOrderBuilderService>();
        Logger.LogInformation("Service resolved from session");

        // Session is automatically tracked by IScopeManager
        // When all windows in session are closed, session can be disposed
        // Or manually dispose:
        // session.Dispose();
    }
}

