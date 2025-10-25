using System.Reflection;
using Autofac;
using WpfEngine.Core.Services;
using WpfEngine.Core.Services.Autofac;
using WpfEngine.Demo.Application;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// Autofac module for service registrations
/// </summary>
public class ServicesModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // NOTE: This module is OBSOLETE - CoreServicesModule handles this now
        // Keeping for reference only
        
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
