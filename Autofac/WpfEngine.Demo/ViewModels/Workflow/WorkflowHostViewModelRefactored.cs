using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using WpfEngine.Services.Autofac;
using WpfEngine.Demo.Services;
using WpfEngine.Services;
using WpfEngine.ViewModels.Managed;

namespace WpfEngine.Demo.ViewModels.Workflow;

/// <summary>
/// Workflow Host ViewModel - Shell pattern (Simplified)
/// 
/// ARCHITECTURE:
/// - Shell window creates scope with tag "workflow-host" or "order-workflow"
/// - IOrderBuilderService is registered as InstancePerMatchingLifetimeScope for this tag
/// - All content ViewModels see the SAME IOrderBuilderService instance
/// - No need for explicit session - window scope IS the session!
/// </summary>
public partial class WorkflowHostViewModelRefactored : ShellViewModel
{
    public WorkflowHostViewModelRefactored(
        INavigator navigator,
        IWindowContext windowService,           // Global singleton
        ILogger<WorkflowHostViewModelRefactored> logger) 
        : base(navigator, windowService, logger)
    {
        Logger.LogInformation("[WORKFLOW_HOST] ViewModel created");
    }

    public override async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        Logger.LogInformation("[WORKFLOW_HOST] Starting workflow - navigating to Step 1");
        await Navigator.NavigateToAsync<WorkflowStep1ViewModelRefactored>();
    }
}

