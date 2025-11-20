using System.Reflection;
using Autofac;
using WpfEngine.Demo.Application;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// Autofac module for Demo-specific service registrations
/// Core services (WindowService, NavigationService, etc.) are registered in CoreServicesModule
/// </summary>
public class ServicesModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // NOTE: Core services (WindowService, ViewRegistry, etc.) are now registered
        // in WpfEngine.Configuration.CoreServicesModule
        
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

