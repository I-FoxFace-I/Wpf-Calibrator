using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

/// <summary>
/// Advanced Demo Menu - entry point for advanced features
/// </summary>
public partial class AdvancedDemoMenuViewModel : BaseViewModel
{
    private readonly IWindowManager _windowManager;

    public AdvancedDemoMenuViewModel(
        IWindowManager windowManager,
        ILogger<AdvancedDemoMenuViewModel> logger) : base(logger)
    {
        _windowManager = windowManager;
        Logger.LogInformation("AdvancedDemoMenuViewModel created");
    }

    [RelayCommand]
    private void OpenCustomerList()
    {
        Logger.LogInformation("Opening Demo Customer List");
        _windowManager.ShowWindow<DemoCustomerListViewModel>();
    }

    [RelayCommand]
    private void OpenProductList()
    {
        Logger.LogInformation("Opening Demo Product List");
        _windowManager.ShowWindow<DemoProductListViewModel>();
    }

    [RelayCommand]
    private void OpenWorkflow()
    {
        Logger.LogInformation("Opening Demo Workflow (Order Creation)");
        _windowManager.ShowWindow<DemoWorkflowHostViewModel>();
    }

    [RelayCommand]
    private void OpenOrderList()
    {
        Logger.LogInformation("Opening Demo Order List");
        _windowManager.ShowWindow<DemoOrderListViewModel>();
    }
}