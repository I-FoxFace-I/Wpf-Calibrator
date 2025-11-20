using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Sessions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Managed;

namespace WpfEngine.Demo.ViewModels.Repository;

/// <summary>
/// Main ViewModel for Demo - Repository Pattern
/// Demonstrates Repository + Unit of Work pattern
/// </summary>
public partial class MainViewModel : ShellViewModel
{
    private readonly IScopeManager _scopeManager;
    private readonly IScopedWindowManager _scopedWindowManager;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MainViewModel(
        INavigator navigator,
        IWindowContext windowContext,
        IScopeManager scopeManager,
        IScopedWindowManager scopedWindowManager,
        ILogger<MainViewModel> logger)
        : base(navigator, windowContext, logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _scopedWindowManager = scopedWindowManager ?? throw new ArgumentNullException(nameof(scopedWindowManager));
        Logger.LogInformation("[DEMO] MainViewModel (Repository) created");
    }

    public override async Task InitializeAsync()
    {
        Logger.LogInformation("[DEMO] MainViewModel initialized (override without parameter)");
        await Task.CompletedTask;
    }

    public override async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        Logger.LogInformation("[DEMO] MainViewModel initialized (override with CancellationToken)");
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenAdvancedDemo()
    {
        Logger.LogInformation("Opening Advanced Patterns Demo (CQRS)");
        StatusMessage = "Opening Advanced Demo...";
        WindowContext.OpenWindow<WpfEngine.Demo.ViewModels.AdvancedMenuViewModel>();
        StatusMessage = "Ready";
    }

    [RelayCommand]
    private void OpenCustomers()
    {
        Logger.LogInformation("Opening Customers window (Repository)");
        StatusMessage = "Opening Customers...";
        WindowContext.OpenWindow<Customer.Repository.CustomerListViewModel>();
        StatusMessage = "Ready";
    }

    [RelayCommand]
    private void OpenProducts()
    {
        Logger.LogInformation("Opening Products window (Repository)");
        StatusMessage = "Opening Products...";
        WindowContext.OpenWindow<Product.Repository.ProductListViewModel>();
        StatusMessage = "Ready";
    }

    [RelayCommand]
    private void OpenOrders()
    {
        Logger.LogInformation("Opening Orders window (Repository)");
        StatusMessage = "Opening Orders...";
        WindowContext.OpenWindow<Order.Repository.OrderListViewModel>();
        StatusMessage = "Ready";
    }

    [RelayCommand]
    private void OpenWorkflow()
    {
        Logger.LogInformation("Opening Order Workflow using session management");
        StatusMessage = "Opening Workflow...";

        // Create workflow session using IScopeManager
        var workflowSession = _scopeManager
            .CreateSession(ScopeTag.Workflow("order-workflow"))
            .AutoCloseWhenEmpty()
            .OpenWindow<Workflow.Repository.WorkflowHostViewModel>();

        Logger.LogInformation("[DEMO] Workflow session created: {SessionId}", workflowSession.SessionId);
        StatusMessage = "Ready";
    }
}


