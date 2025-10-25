using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Advanced Demo Menu - entry point for advanced features
/// </summary>
public partial class AdvancedDemoMenuViewModel : BaseViewModel
{
    private readonly IWindowService _windowService;

    public AdvancedDemoMenuViewModel(
        IWindowService WindowService,
        ILogger<AdvancedDemoMenuViewModel> logger) : base(logger)
    {
        _windowService = WindowService;
        Logger.LogInformation("AdvancedDemoMenuViewModel created");
    }

    [RelayCommand]
    private void OpenCustomerList()
    {
        Logger.LogInformation("Opening Demo Customer List");
        _windowService.OpenWindow<DemoCustomerListViewModel>();
    }

    [RelayCommand]
    private void OpenProductList()
    {
        Logger.LogInformation("Opening Demo Product List");
        _windowService.OpenWindow<DemoProductListViewModel>();
    }

    [RelayCommand]
    private void OpenWorkflow()
    {
        Logger.LogInformation("Opening Demo Workflow (Order Creation)");
        _windowService.OpenWindow<DemoWorkflowHostViewModel>();
    }

    [RelayCommand]
    private void OpenOrderList()
    {
        Logger.LogInformation("Opening Demo Order List");
        _windowService.OpenWindow<DemoOrderListViewModel>();
    }
}