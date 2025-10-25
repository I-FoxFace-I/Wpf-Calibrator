using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// View mapping configuration for Demo application
/// Maps all Demo ViewModels to their corresponding Views
/// </summary>
public class ViewMappingConfiguration : Core.Services.ViewMappingConfiguration
{
    public override void Configure(IViewRegistry registry)
    {
        // ========== MAIN WINDOWS ==========
        registry.MapWindow<MainViewModel, WpfEngine.Demo.Views.MainWindow>();

        // ========== DEMO MENU ==========
        registry.MapWindow<AdvancedDemoMenuViewModel, AdvancedDemoMenuWindow>();

        // ========== CUSTOMER MANAGEMENT ==========
        registry.MapWindow<DemoCustomerListViewModel, DemoCustomerListWindow>();
        registry.MapWindow<DemoCustomerDetailViewModel, DemoCustomerDetailWindow>();

        // ========== PRODUCT MANAGEMENT ==========
        registry.MapWindow<DemoProductListViewModel, DemoProductListWindow>();
        registry.MapWindow<DemoProductDetailViewModel, DemoProductDetailWindow>();
        registry.MapWindow<DemoProductInfoViewModel, DemoProductInfoWindow>();

        // ========== WORKFLOW (Original) ==========
        registry.MapWindow<DemoWorkflowHostViewModel, DemoWorkflowHostWindow>();
        registry.MapControl<DemoWorkflowStep1ViewModel, DemoWorkflowStep1View>();
        registry.MapControl<DemoWorkflowStep2ViewModel, DemoWorkflowStep2View>();
        registry.MapControl<DemoWorkflowStep3ViewModel, DemoWorkflowStep3View>();

        // ========== WORKFLOW (Refactored - Session Support) ==========
        registry.MapWindow<DemoWorkflowHostViewModelRefactored, DemoWorkflowHostWindow>();
        registry.MapControl<DemoWorkflowStep1ViewModelRefactored, DemoWorkflowStep1View>();
        registry.MapControl<DemoWorkflowStep2ViewModelRefactored, DemoWorkflowStep2View>();
        registry.MapControl<DemoWorkflowStep3ViewModelRefactored, DemoWorkflowStep3View>();

        // ========== PRODUCT SELECTOR (for Workflow Session) ==========
        registry.MapWindow<ProductSelectorViewModel, ProductSelectorWindow>();
        registry.MapWindow<ProductDetailSelectorViewModel, ProductDetailSelectorWindow>();

        // ========== ORDER MANAGEMENT ==========
        registry.MapWindow<DemoOrderListViewModel, DemoOrderListWindow>();
        registry.MapWindow<DemoOrderDetailViewModel, DemoOrderDetailWindow>();
    }
}
