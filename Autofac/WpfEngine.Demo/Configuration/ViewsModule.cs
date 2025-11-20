using Autofac;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.ViewModels.Repository;
using WpfEngine.Demo.Views;
using WpfEngine.Demo.Views.Repository;

namespace WpfEngine.Demo.Configuration;

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
        builder.RegisterType<MainViewModel>().AsSelf().InstancePerDependency();
    }
}