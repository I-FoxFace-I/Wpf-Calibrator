using System.Reflection;
using Autofac;
using WpfEngine.Core.Services;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Demo.Services;

namespace WpfEngine.Demo.Configuration;

/// <summary>
/// Autofac module for Advanced Demo features
/// Registers INavigationService, IWindowService, Demo entities, handlers, VMs, Views
/// </summary>
public class DemoModule : Autofac.Module
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

        // ========== WORKFLOW SESSION SERVICES ==========
        
        // IOrderBuilderService - shared across ALL windows in workflow session
        // Uses InstancePerMatchingLifetimeScope to match WorkflowSession tags
        builder.RegisterType<OrderBuilderService>()
               .As<IOrderBuilderService>()
               .InstancePerMatchingLifetimeScope((ILifetimeScope scope, Autofac.Core.IComponentRegistration request) =>
               {
                   var tag = scope.Tag?.ToString() ?? "";
                   return tag.StartsWith("WorkflowSession:");
               });

        // WorkflowState - also shared in workflow session
        builder.RegisterType<WorkflowState>()
               .AsSelf()
               .InstancePerMatchingLifetimeScope((ILifetimeScope scope, Autofac.Core.IComponentRegistration request) =>
               {
                   var tag = scope.Tag?.ToString() ?? "";
                   return tag.StartsWith("WorkflowSession:");
               });

        // ========== DEMO CQRS HANDLERS ==========

        // Register all Demo Command Handlers
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .Where(t => t.Namespace != null &&
                          t.Namespace.Contains("WpfEngine.Demo.Application") &&
                          t.GetInterfaces().Any(i => i.IsGenericType &&
                                                    i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
               .AsClosedTypesOf(typeof(ICommandHandler<>))
               .InstancePerDependency();

        // Register all Demo Query Handlers
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
               .Where(t => t.Namespace != null &&
                          t.Namespace.Contains("WpfEngine.Demo.Application") &&
                          t.GetInterfaces().Any(i => i.IsGenericType &&
                                                    i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
               .AsClosedTypesOf(typeof(IQueryHandler<,>))
               .InstancePerDependency();

        // ========== WORKFLOW SESSION FACTORY ==========
        
        // Factory for creating workflow sessions
        builder.Register<Func<Guid, string, IWorkflowSession>>((c, p) =>
        {
            var windowService = c.Resolve<IWindowService>();
            var logger = c.Resolve<ILogger<WorkflowSession>>();
            
            return (sessionId, sessionName) => new WorkflowSession(sessionId, sessionName, windowService, logger);
        });

        // ========== DEMO VIEWMODELS ==========
        // Register as InstancePerDependency (transient) for proper parameterization

        builder.RegisterType<MainViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Main menu
        builder.RegisterType<AdvancedDemoMenuViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Customer management
        builder.RegisterType<DemoCustomerListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoCustomerDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Product management
        builder.RegisterType<DemoProductListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoProductDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow (Original - keep for backwards compatibility)
        builder.RegisterType<DemoWorkflowHostViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep1ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep2ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep3ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow (REFACTORED - New approach)
        builder.RegisterType<DemoWorkflowHostViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep1ViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep2ViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep3ViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        // Product selector (for workflow)
        builder.RegisterType<ProductSelectorViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductDetailSelectorViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoProductInfoViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // ========== DEMO VIEWS ==========
        // Register as InstancePerDependency

        // Main menu
        builder.RegisterType<AdvancedDemoMenuWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Customer management
        builder.RegisterType<DemoCustomerListWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoCustomerDetailWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Product management
        builder.RegisterType<DemoProductListWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoProductDetailWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow
        builder.RegisterType<DemoWorkflowHostWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow steps (UserControls)
        builder.RegisterType<DemoWorkflowStep1View>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep2View>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep3View>()
               .AsSelf()
               .InstancePerDependency();

        // Product selector windows (for workflow)
        builder.RegisterType<ProductSelectorWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductDetailSelectorWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoProductInfoWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Order management windows
        builder.RegisterType<DemoOrderListWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoOrderDetailWindow>()
               .AsSelf()
               .InstancePerDependency();
    }
}