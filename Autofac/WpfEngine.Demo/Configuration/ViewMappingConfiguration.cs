using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Demo.ViewModels.Obsolete;
using WpfEngine.Demo.ViewModels.Repository;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Demo.Views;
using WpfEngine.Demo.Views.Customer;
using WpfEngine.Demo.Views.Dialogs;
using WpfEngine.Services;
using CustomerViewRepo = WpfEngine.Demo.Views.Customer.Repository;
using DialogsViewRepo = WpfEngine.Demo.Views.Dialogs.Views.Customer.Repository;
using OrderViewRepo = WpfEngine.Demo.Views.Order.Repository;
using ProductViewRepo = WpfEngine.Demo.Views.Product.Repository;
using WorkflowViewRepo = WpfEngine.Demo.Views.Workflow.Repository;
using CustomerRepo = WpfEngine.Demo.ViewModels.Customer.Repository;
using ProductRepo = WpfEngine.Demo.ViewModels.Product.Repository;
using OrderRepo = WpfEngine.Demo.ViewModels.Order.Repository;
using WorkflowRepo = WpfEngine.Demo.ViewModels.Workflow.Repository;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// View mapping configuration for Demo application
/// Maps all Demo ViewModels to their corresponding Views
/// </summary>
public class ViewMappingConfiguration : Abstract.ViewMappingConfiguration
{
    public override void Configure(IViewRegistry registry)
    {
        // ========== CUSTOMER MANAGEMENT ==========
        registry.MapWindow<CustomerRepo.CustomerListViewModel, CustomerViewRepo.CustomerListWindow>();
        registry.MapWindow<CustomerRepo.CustomerDetailViewModel, CustomerViewRepo.CustomerDetailWindow>();
        registry.MapDialog<CustomerRepo.CreateCustomerViewModel, CustomerViewRepo.CreateCustomerDialogWindow>();
        registry.MapDialog<CustomerRepo.CreateAddressDialogViewModel, DialogsViewRepo.CreateAddressDialogWindow>();

        // ========== PRODUCT MANAGEMENT ==========
        registry.MapWindow<ProductRepo.ProductListViewModel, ProductViewRepo.ProductListWindow>();
        registry.MapWindow<ProductRepo.ProductDetailViewModel, ProductViewRepo.ProductDetailWindow>();
        registry.MapWindow<ProductRepo.ProductInfoViewModel, ProductViewRepo.ProductInfoWindow>();
        registry.MapWindow<ProductRepo.ProductSelectorViewModel, ProductViewRepo.ProductSelectorWindow>();
        registry.MapWindow<ProductRepo.ProductDetailSelectorViewModel, ProductViewRepo.ProductDetailSelectorWindow>();
        registry.MapDialog<ProductRepo.ProductCreateViewModel, ProductViewRepo.ProductCreateDialogWindow>();

        // ========== ORDER MANAGEMENT ==========
        registry.MapWindow<OrderRepo.OrderListViewModel, OrderViewRepo.OrderListWindow>();
        registry.MapWindow<OrderRepo.OrderDetailViewModel, OrderViewRepo.OrderDetailWindow>();

        // ========== WORKFLOW ==========
        registry.MapWindow<WorkflowRepo.WorkflowHostViewModel, WorkflowViewRepo.WorkflowHostWindow>();
        registry.MapControl<WorkflowRepo.WorkflowStep1ViewModel, WorkflowViewRepo.WorkflowStep1View>();
        registry.MapControl<WorkflowRepo.WorkflowStep2ViewModel, WorkflowViewRepo.WorkflowStep2View>();
        registry.MapControl<WorkflowRepo.WorkflowStep3ViewModel, WorkflowViewRepo.WorkflowStep3View>();



        // Sessions demo classes removed - session_demo_project.cs was deleted
        
        // ========== MAIN WINDOWS ==========
        registry.MapWindow<MainViewModel, Views.Repository.MainWindow>();

        // ========== DEMO MENU ==========
        registry.MapWindow<AdvancedMenuViewModel, AdvancedDemoMenuWindow>();

        // ========== CUSTOMER MANAGEMENT ==========
        registry.MapWindow<CustomerListViewModel, CustomerListWindow>();
        registry.MapWindow<CustomerDetailViewModel, CustomerDetailWindow>();

        // ========== PRODUCT MANAGEMENT ==========
        registry.MapWindow<ProductListViewModel, ProductListWindow>();
        registry.MapWindow<ProductDetailViewModel, ProductDetailWindow>();
        registry.MapWindow<ProductInfoViewModel, ProductInfoWindow>();

        // ========== WORKFLOW (Original) ==========
        registry.MapWindow<WorkflowHostViewModel, WorkflowHostWindow>();
        registry.MapControl<WorkflowStep1ViewModel, WorkflowStep1View>();
        registry.MapControl<WorkflowStep2ViewModel, WorkflowStep2View>();
        registry.MapControl<WorkflowStep3ViewModel, WorkflowStep3View>();

        // ========== WORKFLOW (Refactored - Session Support) ==========
        registry.MapWindow<WorkflowHostViewModelRefactored, WorkflowHostWindow>();
        registry.MapControl<WorkflowStep1ViewModelRefactored, WorkflowStep1View>();
        registry.MapControl<WorkflowStep2ViewModelRefactored, WorkflowStep2View>();
        registry.MapControl<WorkflowStep3ViewModelRefactored, WorkflowStep3View>();


        // ========== PRODUCT SELECTOR (for Workflow Session) ==========
        registry.MapWindow<ProductSelectorViewModel, ProductSelectorWindow>();
        registry.MapWindow<ProductDetailSelectorViewModel, ProductDetailSelectorWindow>();

        // ========== ORDER MANAGEMENT ==========
        registry.MapWindow<OrderListViewModel, OrderListWindow>();
        registry.MapWindow<OrderDetailViewModel, OrderDetailWindow>();

        registry.MapDialog<CreateAddressDialogViewModel, CreateAddressDialogWindow>();
        registry.MapDialog<ProductCreateViewModel, ProductCreateDialogWindow>();
        registry.MapDialog<CreateCustomerViewModel, CreateCustomerDialogWindow>();
    }
}
