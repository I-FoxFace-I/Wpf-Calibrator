using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// ViewModel for Main Menu window
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly IWindowService _windowManager;

    public MainViewModel(
        IWindowService windowManager,
        ILogger<MainViewModel> logger) : base(logger)
    {
        _windowManager = windowManager;
    }

    // PŘIDAT NOVÝ COMMAND:
    [RelayCommand]
    private void OpenAdvancedDemo()
    {
        Logger.LogInformation("Opening Advanced Patterns Demo");
        _windowManager.OpenWindow<AdvancedDemoMenuViewModel>();
    }

    [RelayCommand]
    private void OpenProducts()
    {
        Logger.LogInformation("Opening Products window");
        //_windowManager.OpenWindow<ProductsViewModel>();
    }

    [RelayCommand]
    private void OpenCustomers()
    {
        Logger.LogInformation("Opening Customers window");
        //_windowManager.OpenWindow<CustomersViewModel>();
    }

    [RelayCommand]
    private void OpenOrders()
    {
        Logger.LogInformation("Opening Orders window");
        //_windowManager.OpenWindow<OrdersViewModel>();
    }

    [RelayCommand]
    private void OpenWorkflow()
    {
        Logger.LogInformation("Opening Order Workflow window");
        //_windowManager.OpenWindow<OrderWorkflowViewModel>();
    }

    [RelayCommand]
    private void OpenScopeHierarchyDemo()
    {
        Logger.LogInformation("Opening Scope Hierarchy Demo window");
        //_windowManager.OpenWindow<ScopeHierarchyDemoViewModel>();
    }
}
