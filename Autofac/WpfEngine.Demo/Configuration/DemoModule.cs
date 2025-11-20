using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Reflection;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Data.Sessions;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Data;
using WpfEngine.Demo.Services;
using WpfEngine.Demo.ViewModels;
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Demo.ViewModels.Obsolete;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Demo.Views;
using WpfEngine.Demo.Views.Customer;
using WpfEngine.Demo.Views.Dialogs;

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

        // Sessions demo classes removed - session_demo_project.cs was deleted

        // Register view mappings
        //builder.Register(c =>
        //{
        //    var registry = c.Resolve<IViewRegistry>();
        //    registry.MapWindow<Sessions.SessionMenuViewModel, Sessions.SessionMenuWindow>();
        //    registry.MapWindow<Sessions.OrderWorkflowMainViewModel, Sessions.OrderWorkflowMainWindow>();
        //    registry.MapWindow<Sessions.CustomerSelectorViewModel, Sessions.CustomerSelectorWindow>();
        //    registry.MapWindow<Sessions.ProductSelectorViewModel, Sessions.ProductSelectorWindow>();
        //    registry.MapWindow<Sessions.OrderSummaryViewModel, Sessions.OrderSummaryWindow>();
        //    registry.MapWindow<Sessions.DashboardViewModel, Sessions.DashboardWindow>();
        //    registry.MapWindow<Sessions.AnalyticsViewModel, Sessions.AnalyticsWindow>();
        //    return registry;
        //}).SingleInstance();

        // ========== WORKFLOW SHARED SERVICES ==========

        // IOrderBuilderService - shared within workflow session scope
        // All ViewModels in workflow session see the SAME instance
        // Using new ScopeTag system: Workflow:order-workflow
        // ScopeTag.Workflow("order-workflow").ToAutofacTag() returns "Workflow:order-workflow"
        var workflowTag = ScopeTag.Workflow("order-workflow").ToAutofacTag();
        
        builder.RegisterType<Services.OrderBuilderService>()
               .As<Services.IOrderBuilderService>()
               .InstancePerMatchingLifetimeScope(workflowTag);
               

        // WorkflowState - also shared in workflow session scope
        builder.RegisterType<WorkflowState>()
               .AsSelf()
               .InstancePerMatchingLifetimeScope(workflowTag);
               

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
        //builder.Register<Func<Guid, string, IWorkflowSession>>((c, p) =>
        //{
        //    var windowService = c.Resolve<IWindowService>();
        //    var logger = c.Resolve<ILogger<WorkflowSession>>();
            
        //    return (sessionId, sessionName) => new WorkflowSession(sessionId, sessionName, windowService, logger);
        //});

        // ========== DEMO VIEWMODELS ==========
        // Register as InstancePerDependency (transient) for proper parameterization

        //builder.RegisterType<ViewModels.Repository.MainViewModel>()
        //       .AsSelf()
        //       .InstancePerDependency();

        // Main menu
        builder.RegisterType<AdvancedMenuViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Session demo
        builder.RegisterType<ViewModels.Menu.SessionDemoViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Customer management
        builder.RegisterType<CustomerListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CustomerDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Product management
        builder.RegisterType<ProductListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<OrderListViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<OrderDetailViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow (Original - keep for backwards compatibility)
        builder.RegisterType<WorkflowHostViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep1ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep2ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep3ViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow (REFACTORED - New approach)
        builder.RegisterType<WorkflowHostViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep1ViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep2ViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep3ViewModelRefactored>()
               .AsSelf()
               .InstancePerDependency();

        // Product selector (for workflow)
        builder.RegisterType<ViewModels.ProductSelectorViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductDetailSelectorViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductInfoViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductCreateViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CreateCustomerViewModel>()
               .AsSelf()
               .InstancePerDependency();

        // ========== DEMO VIEWS ==========
        // Register as InstancePerDependency

        // Main menu
        builder.RegisterType<AdvancedDemoMenuWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Customer management
        builder.RegisterType<CustomerListWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CustomerDetailWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Product management
        builder.RegisterType<ProductListWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductDetailWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow
        builder.RegisterType<WorkflowHostWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Workflow steps (UserControls)
        builder.RegisterType<WorkflowStep1View>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep2View>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<WorkflowStep3View>()
               .AsSelf()
               .InstancePerDependency();

        // Product selector windows (for workflow)
        builder.RegisterType<Views.ProductSelectorWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductDetailSelectorWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductInfoWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Order management windows
        builder.RegisterType<OrderListWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<OrderDetailWindow>()
               .AsSelf()
               .InstancePerDependency();

        // Create Address Dialog windows
        builder.RegisterType<CreateAddressDialogViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CreateAddressDialogWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<ProductCreateDialogWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<CreateCustomerDialogWindow>()
               .AsSelf()
               .InstancePerDependency();

    }
}