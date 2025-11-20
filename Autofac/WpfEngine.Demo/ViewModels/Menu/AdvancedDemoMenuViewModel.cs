using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Sessions;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Advanced Demo Menu - entry point for advanced features
/// REFACTORED: Uses IWindowContext to open child windows
/// Uses new IScopeManager for session management
/// </summary>
public partial class AdvancedMenuViewModel : BaseViewModel
{
    private readonly IWindowContext _windowContext;
    private readonly IScopeManager _scopeManager;
    private readonly IScopedWindowManager _windowManager;

    public AdvancedMenuViewModel(
        IWindowContext windowContext,
        IScopeManager scopeManager,
        IScopedWindowManager windowManager,
        ILogger<AdvancedMenuViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager;
        _windowContext = windowContext;
        _windowManager = windowManager;
        Logger.LogInformation("AdvancedMenuViewModel created");
    }

    [RelayCommand]
    private void OpenCustomerList()
    {
        Logger.LogInformation("Opening Demo Customer List");
        _windowContext.OpenWindow<CustomerListViewModel>();
    }

    [RelayCommand]
    private void OpenProductList()
    {
        Logger.LogInformation("Opening Demo Product List");
        _windowContext.OpenWindow<ProductListViewModel>();
    }

    [RelayCommand]
    private void OpenWorkflow()
    {
        Logger.LogInformation("Opening Order Workflow window using new session system");
        
        // Create workflow session using new IScopeManager
        var workflowSession = _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .Build();
        
        // Open window in the workflow session
        _windowManager.OpenWindowInSession<WorkflowHostViewModelRefactored>(workflowSession.SessionId);
        
        Logger.LogInformation("Workflow session created: {SessionId}", workflowSession.SessionId);
    }

    [RelayCommand]
    private void OpenOrderList()
    {
        Logger.LogInformation("Opening Demo Order List");
        _windowContext.OpenWindow<OrderListViewModel>();
    }
}
