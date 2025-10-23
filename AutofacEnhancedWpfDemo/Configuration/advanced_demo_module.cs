using System.Reflection;
using Autofac;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Data.Demo;
using AutofacEnhancedWpfDemo.Services.Demo;
using AutofacEnhancedWpfDemo.ViewModels.Demo;
using AutofacEnhancedWpfDemo.Views.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.Configuration;

/// <summary>
/// Autofac module for Advanced Demo features
/// Registers INavigator, IWindowManager, Demo entities, handlers, VMs, Views
/// </summary>
public class AdvancedDemoModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ========== DEMO DATABASE ==========
        
        builder.Register<IDbContextFactory<DemoDbContext>>(c =>
        {
            var loggerFactory = c.Resolve<ILoggerFactory>();
            
            var options = new DbContextOptionsBuilder<DemoDbContext>()
                .UseSqlite("Data Source=DemoDb_Advanced.db")
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging()
                .Options;

            return new PooledDbContextFactory<DemoDbContext>(options);
        })
        .As<IDbContextFactory<DemoDbContext>>()
        .SingleInstance();

        // ========== NEW SERVICES ==========

        // INavigator - for ViewModel navigation within a window
        builder.RegisterType<Navigator>()
               .As<INavigator>()
               .InstancePerLifetimeScope();

        // IWindowManager - for physical window management
        builder.RegisterType<WindowManager>()
               .As<IWindowManager>()
               .InstancePerLifetimeScope();

        // IViewLocator - for convention-based View discovery
        builder.RegisterType<ViewLocator>()
               .As<IViewLocator>()
               .InstancePerLifetimeScope();

        // ========== DEMO CQRS HANDLERS ==========

        // Register all Demo Command Handlers
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .Where(t => t.Namespace != null && 
                          t.Namespace.Contains("AutofacEnhancedWpfDemo.Application.Demo") &&
                          t.GetInterfaces().Any(i => i.IsGenericType && 
                                                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
               .AsClosedTypesOf(typeof(ICommandHandler<>))
               .InstancePerDependency();
        
        // Register all Demo Query Handlers
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .Where(t => t.Namespace != null && 
                          t.Namespace.Contains("AutofacEnhancedWpfDemo.Application.Demo") &&
                          t.GetInterfaces().Any(i => i.IsGenericType && 
                                                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
               .AsClosedTypesOf(typeof(IQueryHandler<,>))
               .InstancePerDependency();
        
        // ========== DEMO VIEWMODELS ==========
        
        builder.RegisterAssemblyTypes(typeof(AdvancedDemoMenuViewModel).Assembly)
               .Where(t => t.Namespace != null && 
                          t.Namespace.Contains("AutofacEnhancedWpfDemo.ViewModels.Demo"))
               .AsSelf()
               .InstancePerDependency();
        
        // ========== DEMO VIEWS ==========
        
        builder.RegisterType<AdvancedDemoMenuWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<DemoCustomerListWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<DemoCustomerDetailWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<DemoProductListWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<DemoProductDetailWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<DemoWorkflowHostWindow>().AsSelf().InstancePerDependency();
    }
}
