using WpfEngine.Services;

namespace WpfEngine.Abstract;

/// <summary>
/// Configuration class for View mappings
/// Implement this and register in DI container
/// </summary>
public abstract class ViewMappingConfiguration
{
    /// <summary>
    /// Configure View mappings
    /// Called during application startup
    /// </summary>
    public abstract void Configure(IViewRegistry registry);
}
