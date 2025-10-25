using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WpfEngine.Core.Scopes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Services.MicrosoftDI.Scopes;

/// <summary>
/// Hierarchical scope context implementation
/// Manages parent-child relationships and scope-specific instances
/// </summary>
public class ScopeContext : IScopeContext
{
    private readonly ILogger<ScopeContext> _logger;
    private readonly ConcurrentDictionary<Type, object> _scopedInstances = new();
    private readonly List<IScopeContext> _children = new();
    private bool _disposed;

    public ScopeContext(
        IServiceScope serviceScope,
        string scopeTag,
        IScopeContext? parent,
        ILogger<ScopeContext> logger)
    {
        ServiceScope = serviceScope;
        ScopeTag = scopeTag;
        Parent = parent;
        ScopeId = Guid.NewGuid();
        _logger = logger;

        // Register self
        _scopedInstances[typeof(IScopeContext)] = this;

        _logger.LogInformation("[SCOPE] Created scope {ScopeId} (Tag: {ScopeTag}, Parent: {ParentId})",
            ScopeId, ScopeTag, Parent?.ScopeId);
    }

    public Guid ScopeId { get; }
    public string ScopeTag { get; }
    public IScopeContext? Parent { get; }
    public IServiceScope ServiceScope { get; }
    public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;
    public bool IsDisposed => _disposed;

    public IScopeContext CreateChild(string childTag)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ScopeContext));

        _logger.LogDebug("[SCOPE] Creating child scope from {ParentId} with tag {ChildTag}",
            ScopeId, childTag);

        var childServiceScope = ServiceProvider.CreateScope();
        var childLogger = childServiceScope.ServiceProvider.GetRequiredService<ILogger<ScopeContext>>();
        
        var childContext = new ScopeContext(
            childServiceScope,
            childTag,
            this,
            childLogger);

        _children.Add(childContext);

        _logger.LogInformation("[SCOPE] Child scope {ChildId} created from parent {ParentId}",
            childContext.ScopeId, ScopeId);

        return childContext;
    }

    public void RegisterInstance<TService>(TService instance) where TService : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ScopeContext));

        var serviceType = typeof(TService);
        
        if (_scopedInstances.TryAdd(serviceType, instance))
        {
            _logger.LogDebug("[SCOPE] Registered instance {ServiceType} in scope {ScopeId}",
                serviceType.Name, ScopeId);
        }
        else
        {
            _logger.LogWarning("[SCOPE] Instance {ServiceType} already registered in scope {ScopeId}, overwriting",
                serviceType.Name, ScopeId);
            _scopedInstances[serviceType] = instance;
        }
    }

    public bool TryResolveInstance<TService>(out TService? instance) where TService : class
    {
        var serviceType = typeof(TService);

        // Try current scope
        if (_scopedInstances.TryGetValue(serviceType, out var obj))
        {
            instance = obj as TService;
            _logger.LogDebug("[SCOPE] Resolved instance {ServiceType} from scope {ScopeId}",
                serviceType.Name, ScopeId);
            return instance != null;
        }

        // Try parent scopes (walk up hierarchy)
        var currentParent = Parent;
        while (currentParent != null)
        {
            if (currentParent.TryResolveInstance<TService>(out instance))
            {
                _logger.LogDebug("[SCOPE] Resolved instance {ServiceType} from parent scope {ParentId}",
                    serviceType.Name, currentParent.ScopeId);
                return true;
            }
            currentParent = currentParent.Parent;
        }

        instance = null;
        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _logger.LogInformation("[SCOPE] Disposing scope {ScopeId} (Tag: {ScopeTag})",
            ScopeId, ScopeTag);

        // Dispose all children first
        foreach (var child in _children)
        {
            child.Dispose();
        }
        _children.Clear();

        // Dispose scoped instances that implement IDisposable
        foreach (var instance in _scopedInstances.Values)
        {
            if (instance is IDisposable disposable && instance != this)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SCOPE] Error disposing instance {InstanceType} in scope {ScopeId}",
                        instance.GetType().Name, ScopeId);
                }
            }
        }
        _scopedInstances.Clear();

        // Dispose service scope
        ServiceScope?.Dispose();

        _disposed = true;

        _logger.LogInformation("[SCOPE] Scope {ScopeId} disposed", ScopeId);
    }
}

/// <summary>
/// Factory for creating scope contexts
/// </summary>
public class ScopeContextFactory : IScopeContextFactory
{
    private readonly IServiceProvider _rootServiceProvider;
    private readonly ILogger<ScopeContext> _logger;

    public ScopeContextFactory(
        IServiceProvider rootServiceProvider,
        ILogger<ScopeContext> logger)
    {
        _rootServiceProvider = rootServiceProvider;
        _logger = logger;
    }

    public IScopeContext CreateRootScope(string scopeTag)
    {
        _logger.LogInformation("[SCOPE_FACTORY] Creating root scope with tag {ScopeTag}", scopeTag);

        var serviceScope = _rootServiceProvider.CreateScope();
        var scopeLogger = serviceScope.ServiceProvider.GetRequiredService<ILogger<ScopeContext>>();

        return new ScopeContext(
            serviceScope,
            scopeTag,
            parent: null,
            scopeLogger);
    }

    public IScopeContext CreateChildScope(IScopeContext parent, string scopeTag)
    {
        _logger.LogInformation("[SCOPE_FACTORY] Creating child scope from {ParentId} with tag {ScopeTag}",
            parent.ScopeId, scopeTag);

        return parent.CreateChild(scopeTag);
    }
}
