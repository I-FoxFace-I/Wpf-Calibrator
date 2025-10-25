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

        // ========== WORKFLOW STATE ==========
        // Shared state for workflow steps within a session
        // Uses InstancePerMatchingLifetimeScope to share across windows in same session
        builder.RegisterType<WorkflowState>()
               .AsSelf()
               .InstancePerMatchingLifetimeScope("workflow-session-*");

        // ========== SHARED WORKFLOW SERVICES ==========
        // Services shared within window scope (or session scope if window opened from session)
        // For workflow steps in same window: all steps share same instance (window scope)
        // For multi-window workflows: use session scope (see SESSION_PATTERN_GUIDE.md)
        builder.RegisterType<WpfEngine.Demo.Services.OrderBuilderService>()
               .As<WpfEngine.Demo.Services.IOrderBuilderService>()
               .InstancePerLifetimeScope(); // Shared per window (or per session if in session)

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

        builder.RegisterType<DemoProductInfoViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Order management
        builder.RegisterType<DemoOrderListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoOrderDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow
        builder.RegisterType<DemoWorkflowHostViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep1ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep2ViewModel>()
               .AsSelf()
               .PropertiesAutowired() // Enable property injection for Session
               .InstancePerDependency();

        builder.RegisterType<DemoWorkflowStep3ViewModel>()
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

        builder.RegisterType<DemoProductInfoWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Order management
        builder.RegisterType<DemoOrderListWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<DemoOrderDetailWindow>()
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

    }
}