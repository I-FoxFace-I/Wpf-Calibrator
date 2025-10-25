using Autofac;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;

namespace WpfEngie.Demo.Configuration;

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
        builder.RegisterType<WpfEngine.Demo.Views.MainWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<WpfEngine.Demo.ViewModels.MainViewModel>().AsSelf().InstancePerDependency();
    }
}