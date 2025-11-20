using Autofac;

namespace WpfEngine.Services.Sessions;

/// <summary>
/// Session module for complex service registration scenarios
/// </summary>
public interface ISessionModule
{
    /// <summary>
    /// Configure services for this session module
    /// </summary>
    /// <param name="builder">Container builder</param>
    void ConfigureServices(ContainerBuilder builder);
}

