using Autofac;
using CustomerRepo = WpfEngine.Demo.ViewModels.Customer.Repository;
using ProductRepo = WpfEngine.Demo.ViewModels.Product.Repository;
using OrderRepo = WpfEngine.Demo.ViewModels.Order.Repository;
using WorkflowRepo = WpfEngine.Demo.ViewModels.Workflow.Repository;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// Autofac module pro registraci ViewModels používajících Repository pattern
/// Tyto ViewModely mají suffix "Repository" pro odlišení od CQRS variant
/// </summary>
public class RepositoryViewModelsModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ========== MAIN VIEWMODEL (Repository Pattern) ==========
        builder.RegisterType<WpfEngine.Demo.ViewModels.Repository.MainViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // ========== CUSTOMER VIEWMODELS ==========
        builder.RegisterType<CustomerRepo.CustomerListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CustomerRepo.CustomerDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CustomerRepo.CreateCustomerViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CustomerRepo.CreateAddressDialogParams>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CustomerRepo.CustomerEditDialogViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // ========== PRODUCT VIEWMODELS ==========
        builder.RegisterType<ProductRepo.ProductListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductRepo.ProductDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductRepo.ProductCreateViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductRepo.ProductInfoViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductRepo.ProductSelectorViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductRepo.ProductDetailSelectorViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // ========== ORDER VIEWMODELS ==========
        builder.RegisterType<OrderRepo.OrderListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<OrderRepo.OrderDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // ========== WORKFLOW VIEWMODELS ==========
        builder.RegisterType<WorkflowRepo.WorkflowHostViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowRepo.WorkflowStep1ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowRepo.WorkflowStep2ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowRepo.WorkflowStep3ViewModel>()
               .AsSelf()
               .InstancePerDependency();
    }
}

