using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Windows;
using System;
using WpfEngine.Demo.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Services;
using WpfEngine.ViewModels.Managed;

namespace WpfEngine.Demo.ViewModels.Workflow.Repository;

/// <summary>
/// Workflow Host ViewModel - Shell pattern (Simplified)
/// 
/// ARCHITECTURE:
/// - Shell window creates scope with tag "workflow-host" or "order-workflow"
/// - IOrderBuilderService is registered as InstancePerMatchingLifetimeScope for this tag
/// - All content ViewModels see the SAME IOrderBuilderService instance
/// - No need for explicit session - window scope IS the session!
/// </summary>
public partial class WorkflowHostViewModel : ShellViewModel
{
    public WorkflowHostViewModel(
        INavigator navigator,
        IWindowContext windowService,           // Global singleton
        ILogger<WorkflowHostViewModel> logger) 
        : base(navigator, windowService, logger)
    {
        Logger.LogInformation("[WORKFLOW_HOST] ViewModel created");
    }

    public override async Task InitializeAsync()
    {
        Logger.LogInformation("[WORKFLOW_HOST] Starting workflow - navigating to Step 1 (override without parameter)");
        await Navigator.NavigateToAsync<WorkflowStep1ViewModel>();
    }

    public override async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        Logger.LogInformation("[WORKFLOW_HOST] Starting workflow - navigating to Step 1 (override with CancellationToken)");
        await Navigator.NavigateToAsync<WorkflowStep1ViewModel>();
    }
}