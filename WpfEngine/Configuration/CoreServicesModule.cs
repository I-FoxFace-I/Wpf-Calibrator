using System.Linq;
using Autofac;
using WpfEngine.Core.Services;
using WpfEngine.Core.Services.Autofac;
using WpfEngine.Services.Autofac;

namespace WpfEngine.Configuration;

/// <summary>
/// Autofac module for Core services registration
/// </summary>
public class CoreServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ========== VIEW REGISTRY (Singleton) ==========
        // ViewRegistry is singleton - holds all VM->View mappings
        builder.RegisterType<ViewRegistry>()
               .AsSelf()
               .As<IViewRegistry>()
               .SingleInstance();

        // ========== VIEW LOCATOR (InstancePerLifetimeScope) ==========
        // ViewLocator needs scope to resolve views
        builder.RegisterType<Services.ViewLocatorService>()
               .As<IViewLocatorService>()
               .InstancePerLifetimeScope();

        // ========== VIEWMODEL FACTORY (InstancePerLifetimeScope) ==========
        // Factory uses current scope to create ViewModels
        builder.RegisterType<Services.Autofac.ViewModelFactory>()
               .As<IViewModelFactory>()
               .InstancePerLifetimeScope();

        // ========== NAVIGATION SERVICE (InstancePerLifetimeScope) ==========
        // Each window/scope has its own navigation service
        builder.RegisterType<NavigationService>()
               .As<INavigationService>()
               .InstancePerLifetimeScope();

        // ========== WINDOW SERVICE (InstancePerLifetimeScope) ==========
        // Each scope manages its own windows
        builder.RegisterType<WpfEngine.Core.Services.Autofac.WindowService>()
               .As<IWindowService>()
               .InstancePerLifetimeScope();

        // ========== DIALOG SERVICE (InstancePerLifetimeScope) ==========
        // Dialog service per scope
        builder.RegisterType<Services.Autofac.DialogService>()
               .As<IDialogService>()
               .InstancePerLifetimeScope();

        // ========== WORKFLOW SESSION FACTORY (Singleton) ==========
        // Factory for creating workflow sessions
        builder.RegisterType<WpfEngine.Core.Services.Autofac.WorkflowSessionFactory>()
               .As<IWorkflowSessionFactory>()
               .SingleInstance();
    }
}
