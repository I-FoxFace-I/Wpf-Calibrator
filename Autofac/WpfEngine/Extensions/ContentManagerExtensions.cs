using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;

namespace WpfEngine.Extensions;

/// <summary>
/// Extension methods for registering Content Manager services
/// </summary>
public static class ContentManagerExtensions
{
    /// <summary>
    /// Registers Content Manager services in the Autofac container
    /// </summary>
    /// <param name="builder">Container builder</param>
    /// <param name="useNavigatorWithContentManager">
    /// If true, registers NavigatorWithContentManager instead of the standard Navigator.
    /// Default is false for backward compatibility.
    /// </param>
    /// <returns>The container builder for method chaining</returns>
    public static ContainerBuilder AddContentManager(
        this ContainerBuilder builder,
        bool useNavigatorWithContentManager = false)
    {
        // Register IContentManager (lightweight version)
        builder.RegisterType<ContentManager>()
            .As<IContentManager>()
            .InstancePerLifetimeScope();  // Per window/session scope

        // Optionally register Navigator with Content Manager
        if (useNavigatorWithContentManager)
        {
            builder.RegisterType<Navigator>()
                .As<INavigator>()
                .InstancePerLifetimeScope();
        }

        return builder;
    }

    /// <summary>
    /// Registers Content Manager and Navigator with Content Manager in a window scope
    /// </summary>
    /// <param name="scope">Lifetime scope to register services in</param>
    /// <param name="windowId">Window ID for logging and tracking</param>
    /// <returns>The updated scope for method chaining</returns>
    public static ILifetimeScope WithContentManager(
        this ILifetimeScope scope,
        System.Guid windowId)
    {
        return scope.BeginLifetimeScope($"ContentManager:{windowId}", builder =>
        {
            builder.RegisterType<ContentManager>()
                .As<IContentManager>()
                .SingleInstance();  // Singleton within this window scope

            builder.RegisterType<Navigator>()
                .As<INavigator>()
                .SingleInstance();
        });
    }

    /// <summary>
    /// Registers lightweight Content Factory (only create/dispose, no tracking)
    /// Recommended for most applications where you don't need content metadata/events
    /// </summary>
    /// <param name="builder">Container builder</param>
    /// <returns>The container builder for method chaining</returns>
    public static ContainerBuilder AddLightContentManager(
        this ContainerBuilder builder)
    {
        // Register lightweight factory
        builder.RegisterType<ContentManager>()
            .As<IContentManager>()
            .InstancePerLifetimeScope();  // Per window/session scope

        // Register Navigator with light factory
        builder.RegisterType<Navigator>()
            .As<INavigator>()
            .InstancePerLifetimeScope();

        return builder;
    }
}

