using System.Linq;
using Autofac;
using AutofacEnhancedWpfDemo.Services.Autofac;
using WpfEngine.Core.Services;
using WpfEngine.Services;
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
        builder.RegisterType<ViewLocatorService>()
               .As<IViewLocatorService>()
               .InstancePerLifetimeScope();

        // ========== VIEWMODEL FACTORY (InstancePerLifetimeScope) ==========
        // Factory uses current scope to create ViewModels
        builder.RegisterType<ViewModelFactory>()
               .As<IViewModelFactory>()
               .InstancePerLifetimeScope();

        // ========== NAVIGATION SERVICE (InstancePerLifetimeScope) ==========
        // Each window/scope has its own navigation service
        builder.RegisterType<NavigationService>()
               .As<INavigationService>()
               .InstancePerLifetimeScope();

        // ========== CONTENT MANAGER (InstancePerMatchingLifetimeScope - Window scopes) ==========
        // Each window has its own content manager for shell content navigation
        builder.RegisterType<WpfEngine.Core.Services.Autofac.ContentManager>()
               .As<IContentManager>()
               .InstancePerMatchingLifetimeScope((ILifetimeScope scope, Autofac.Core.IComponentRegistration request) =>
               {
                   var tag = scope.Tag?.ToString() ?? "";
                   return tag.StartsWith("Window:");
               });

        // ========== WINDOW SERVICE (InstancePerLifetimeScope) ==========
        // Each scope manages its own windows
        // Using WindowServiceRefactored for session support
        builder.RegisterType<WpfEngine.Core.Services.Autofac.WindowServiceRefactored>()
               .As<IWindowService>()
               .InstancePerLifetimeScope();

        // ========== DIALOG SERVICE (InstancePerLifetimeScope) ==========
        // Dialog service per scope
        builder.RegisterType<DialogService>()
               .As<IDialogService>()
               .InstancePerLifetimeScope();
    }
}
