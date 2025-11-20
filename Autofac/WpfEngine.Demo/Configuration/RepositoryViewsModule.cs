using Autofac;
using WpfEngine.Demo.Views.Customer.Repository;
using WpfEngine.Demo.Views.Order.Repository;
using WpfEngine.Demo.Views.Product.Repository;
using WpfEngine.Demo.Views.Workflow.Repository;
using WpfEngine.Demo.Views.Dialogs.Views.Customer.Repository;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// Registrace Views používajících Repository pattern
/// </summary>
public class RepositoryViewsModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Main Window (Repository pattern)
        builder.RegisterType<WpfEngine.Demo.Views.Repository.MainWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Customer Views
        builder.RegisterType<CustomerListWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<CustomerDetailWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<CreateAddressDialogWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<CreateCustomerDialogWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Product Views
        builder.RegisterType<ProductListWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<ProductDetailWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<ProductDetailSelectorWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<ProductSelectorWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<ProductInfoWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<ProductCreateDialogWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Order Views
        builder.RegisterType<OrderListWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<OrderDetailWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow Views
        builder.RegisterType<WorkflowHostWindow>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<WorkflowStep1View>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<WorkflowStep2View>()
               .AsSelf()
               .InstancePerDependency();
        builder.RegisterType<WorkflowStep3View>()
               .AsSelf()
               .InstancePerDependency();
    }
}


