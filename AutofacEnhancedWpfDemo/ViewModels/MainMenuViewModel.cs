using AutofacEnhancedWpfDemo.Services;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Main Menu window
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly IWindowManager _windowManager;

    public MainViewModel(
        IWindowManager windowManager,
        ILogger<MainViewModel> logger) : base(logger)
    {
        _windowManager = windowManager;
    }

    // PØIDAT NOVÝ COMMAND:
    [RelayCommand]
    private void OpenAdvancedDemo()
    {
        Logger.LogInformation("Opening Advanced Patterns Demo");
        _windowManager.ShowWindow<Demo.AdvancedDemoMenuViewModel>();
    }

    [RelayCommand]
    private void OpenProducts()
    {
        Logger.LogInformation("Opening Products window");
        _windowManager.ShowWindow<ProductsViewModel>();
    }

    [RelayCommand]
    private void OpenCustomers()
    {
        Logger.LogInformation("Opening Customers window");
        _windowManager.ShowWindow<CustomersViewModel>();
    }

    [RelayCommand]
    private void OpenOrders()
    {
        Logger.LogInformation("Opening Orders window");
        _windowManager.ShowWindow<OrdersViewModel>();
    }

    [RelayCommand]
    private void OpenWorkflow()
    {
        Logger.LogInformation("Opening Order Workflow window");
        _windowManager.ShowWindow<OrderWorkflowViewModel>();
    }

    [RelayCommand]
    private void OpenScopeHierarchyDemo()
    {
        Logger.LogInformation("Opening Scope Hierarchy Demo window");
        _windowManager.ShowWindow<ScopeHierarchyDemoViewModel>();
    }
}
