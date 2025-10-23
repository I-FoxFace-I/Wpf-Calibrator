using System.Reflection;
using Autofac;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Services;

namespace AutofacEnhancedWpfDemo.Configuration;

/// <summary>
/// Autofac module for service registrations
/// </summary>
public class ServicesModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //// ViewLocator - Singleton
        //builder.RegisterType<ViewLocator>()
        //       .As<IViewLocator>()
        //       .SingleInstance();
        
        // Navigator - InstancePerLifetimeScope (each window has its own)
        builder.RegisterType<WindowNavigator>()
               .As<IWindowNavigator>()
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
