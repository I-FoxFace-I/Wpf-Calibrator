//using System;
//using Microsoft.Extensions.DependencyInjection;

//namespace WpfEngine.Core.Scopes;

///// <summary>
///// Represents a scope context with hierarchy support
///// Each window/workflow has its own scope context
///// </summary>
//public interface IScopeContext : IDisposable
//{
//    /// <summary>
//    /// Unique identifier for this scope
//    /// </summary>
//    Guid ScopeId { get; }

//    /// <summary>
//    /// Scope tag/name for debugging
//    /// </summary>
//    string ScopeTag { get; }

//    /// <summary>
//    /// Parent scope context (null for root)
//    /// </summary>
//    IScopeContext? Parent { get; }

//    /// <summary>
//    /// Service scope for this context
//    /// </summary>
//    IServiceScope ServiceScope { get; }

//    /// <summary>
//    /// Service provider for this scope
//    /// </summary>
//    IServiceProvider ServiceProvider { get; }

//    /// <summary>
//    /// Creates child scope context
//    /// </summary>
//    IScopeContext CreateChild(string childTag);

//    /// <summary>
//    /// Registers scope-specific service instance
//    /// </summary>
//    void RegisterInstance<TService>(TService instance) where TService : class;

//    /// <summary>
//    /// Tries to resolve scope-specific instance
//    /// </summary>
//    bool TryResolveInstance<TService>(out TService? instance) where TService : class;

//    /// <summary>
//    /// Checks if this scope is disposed
//    /// </summary>
//    bool IsDisposed { get; }
//}

///// <summary>
///// Factory for creating scope contexts
///// </summary>
//public interface IScopeContextFactory
//{
//    /// <summary>
//    /// Creates root scope context
//    /// </summary>
//    IScopeContext CreateRootScope(string scopeTag);

//    /// <summary>
//    /// Creates child scope context from parent
//    /// </summary>
//    IScopeContext CreateChildScope(IScopeContext parent, string scopeTag);
//}

///// <summary>
///// Scope module - registers services for specific scope
///// Similar to Autofac modules but for Microsoft DI
///// </summary>
//public interface IScopeModule
//{
//    /// <summary>
//    /// Module name for debugging
//    /// </summary>
//    string ModuleName { get; }

//    /// <summary>
//    /// Registers services into scope
//    /// </summary>
//    void ConfigureServices(IServiceCollection services);

//    /// <summary>
//    /// Called when scope is created
//    /// Can register scope-specific instances
//    /// </summary>
//    void OnScopeCreated(IScopeContext scopeContext);

//    /// <summary>
//    /// Called when scope is disposed
//    /// </summary>
//    void OnScopeDisposed(IScopeContext scopeContext);
//}

///// <summary>
///// Base implementation of scope module
///// </summary>
//public abstract class ScopeModuleBase : IScopeModule
//{
//    public abstract string ModuleName { get; }

//    public abstract void ConfigureServices(IServiceCollection services);

//    public virtual void OnScopeCreated(IScopeContext scopeContext)
//    {
//        // Override if needed
//    }

//    public virtual void OnScopeDisposed(IScopeContext scopeContext)
//    {
//        // Override if needed
//    }
//}

///// <summary>
///// Scope module collection
///// </summary>
//public interface IScopeModuleCollection
//{
//    /// <summary>
//    /// Registers scope module
//    /// </summary>
//    void RegisterModule(IScopeModule module);

//    /// <summary>
//    /// Gets all registered modules
//    /// </summary>
//    IScopeModule[] GetModules();

//    /// <summary>
//    /// Applies all modules to service collection
//    /// </summary>
//    void ApplyModules(IServiceCollection services);
//}
