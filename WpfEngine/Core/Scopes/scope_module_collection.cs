using System.Collections.Generic;
using System.Linq;
using WpfEngine.Core.Scopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services.MicrosoftDI.Scopes;

/// <summary>
/// Collection of scope modules
/// Manages registration and application of modules
/// </summary>
public class ScopeModuleCollection : IScopeModuleCollection
{
    private readonly List<IScopeModule> _modules = new();
    private readonly ILogger<ScopeModuleCollection> _logger;

    public ScopeModuleCollection(ILogger<ScopeModuleCollection> logger)
    {
        _logger = logger;
    }

    public void RegisterModule(IScopeModule module)
    {
        if (_modules.Any(m => m.ModuleName == module.ModuleName))
        {
            _logger.LogWarning("[SCOPE_MODULES] Module {ModuleName} already registered, skipping", 
                module.ModuleName);
            return;
        }

        _modules.Add(module);
        _logger.LogInformation("[SCOPE_MODULES] Registered module {ModuleName}", module.ModuleName);
    }

    public IScopeModule[] GetModules()
    {
        return _modules.ToArray();
    }

    public void ApplyModules(IServiceCollection services)
    {
        _logger.LogInformation("[SCOPE_MODULES] Applying {Count} modules to service collection", 
            _modules.Count);

        foreach (var module in _modules)
        {
            _logger.LogDebug("[SCOPE_MODULES] Applying module {ModuleName}", module.ModuleName);
            module.ConfigureServices(services);
        }

        _logger.LogInformation("[SCOPE_MODULES] All modules applied successfully");
    }
}

/// <summary>
/// Extension methods for scope modules
/// </summary>
public static class ScopeModuleExtensions
{
    /// <summary>
    /// Notifies all modules about scope creation
    /// </summary>
    public static void NotifyScopeCreated(
        this IScopeModuleCollection moduleCollection, 
        IScopeContext scopeContext)
    {
        foreach (var module in moduleCollection.GetModules())
        {
            module.OnScopeCreated(scopeContext);
        }
    }

    /// <summary>
    /// Notifies all modules about scope disposal
    /// </summary>
    public static void NotifyScopeDisposed(
        this IScopeModuleCollection moduleCollection, 
        IScopeContext scopeContext)
    {
        foreach (var module in moduleCollection.GetModules())
        {
            module.OnScopeDisposed(scopeContext);
        }
    }
}
