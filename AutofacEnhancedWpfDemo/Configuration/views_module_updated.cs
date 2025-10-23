using Autofac;
using AutofacEnhancedWpfDemo.ViewModels;
using AutofacEnhancedWpfDemo.Views;

namespace AutofacWpfDemo.Configuration;

/// <summary>
/// Autofac module for Views and ViewModels
/// All registered as transient (InstancePerDependency)
/// </summary>
public class ViewsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register all ViewModels as InstancePerDependency (transient)
        builder.RegisterAssemblyTypes(typeof(BaseViewModel).Assembly)
               .Where(t => t.Name.EndsWith("ViewModel"))
               .AsSelf()
               .InstancePerDependency();

        // Register all Windows as InstancePerDependency (transient)
        builder.RegisterType<MainWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<ProductsWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<EditProductWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<CustomersWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<OrdersWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<OrderDetailWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<OrderWorkflowWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<ScopeHierarchyDemoWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<ChildDemoWindow>().AsSelf().InstancePerDependency();
    }
}