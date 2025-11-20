using Autofac;
using WpfEngine.Abstract;
using WpfEngine.Services;

namespace WpfEngine.Configuration;

/// <summary>
/// Extension methods for configuring View mappings
/// </summary>
public static class ViewRegistryExtensions
{
    /// <summary>
    /// Configures View mappings using ViewMappingConfiguration
    /// Call this during application startup after building container
    /// </summary>
    public static void ConfigureViewMappings(this IContainer container)
    {
        var registry = container.Resolve<IViewRegistry>();

        // Resolve all ViewMappingConfiguration instances
        var configurations = container.Resolve<IEnumerable<ViewMappingConfiguration>>();

        foreach (var config in configurations)
        {
            config.Configure(registry);
        }
    }

    /// <summary>
    /// Registers ViewMappingConfiguration in ContainerBuilder
    /// </summary>
    public static ContainerBuilder RegisterViewMappingConfiguration<TConfiguration>(
        this ContainerBuilder builder)
        where TConfiguration : ViewMappingConfiguration
    {
        builder.RegisterType<TConfiguration>()
               .As<ViewMappingConfiguration>()
               .SingleInstance();

        return builder;
    }
}
