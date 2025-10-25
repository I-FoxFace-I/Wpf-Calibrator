using System.Reflection;
using Autofac;
using AutofacEnhancedWpfDemo.Services.Autofac;
using WpfEngine.Core.Services;
using WpfEngine.Demo.Application;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// Autofac module for service registrations
/// </summary>
public class ServicesModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ViewLocator - Singleton
        builder.RegisterType<ViewLocatorService>()
               .As<IViewLocatorService>()
               .SingleInstance();

        //Navigator - InstancePerLifetimeScope(each window has its own)
        builder.RegisterType<NavigationService>()
               .As<INavigationService>()
               .InstancePerLifetimeScope();

        // Using WindowServiceRefactored for session support
        builder.RegisterType<WpfEngine.Core.Services.Autofac.WindowServiceRefactored>()
               .As<IWindowService>()
               .InstancePerLifetimeScope();

        builder.RegisterType<DialogService>()
               .As<IDialogService>()
               .InstancePerLifetimeScope();

        builder.RegisterType<ViewModelFactory>()
               .As<IViewModelFactory>()
               .InstancePerLifetimeScope();

        // Register all Command Handlers
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .AsClosedTypesOf(typeof(ICommandHandler<>))
               .InstancePerDependency();

        // Register all Query Handlers
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .AsClosedTypesOf(typeof(IQueryHandler<,>))
               .InstancePerDependency();
    }
}
