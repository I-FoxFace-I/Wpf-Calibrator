using Autofac;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.ViewModels;
using WpfEngine.Tests.Core.Services;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;


namespace WpfEngine.Tests.Helpers;

/// <summary>
/// Base test fixture for Autofac-based tests
/// Provides common container setup and cleanup
/// </summary>
public abstract class AutofacTestFixture : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    private readonly IViewRegistry _viewRegistry;
    private readonly IWindowManager _windowManager;
    private readonly IWindowTracker _windowTracker;
    protected IContainer Container => _container;
    protected ILifetimeScope Scope => _scope;
    protected IViewRegistry ViewRegistry => _viewRegistry;
    protected IWindowTracker WindowTracker => _windowTracker;
    protected IWindowManager WindowManager => _windowManager;

    protected AutofacTestFixture()
    {
        _container = BuildContainer();
        _viewRegistry = BuildViewRegistery();
        _scope = _container.BeginLifetimeScope("root");
        _windowTracker = _scope.Resolve<IWindowTracker>();
        _windowManager = _scope.Resolve<IWindowManager>();
        
    }

    /// <summary>
    /// Build Autofac container with common registrations
    /// Override to customize for specific tests
    /// </summary>
    protected virtual IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();

        // Register core services
        RegisterCoreServices(builder);

        // Register test-specific services
        RegisterTestServices(builder);

        return builder.Build();
    }

    /// <summary>
    /// Register core WpfEngine services
    /// </summary>
    protected virtual void RegisterCoreServices(ContainerBuilder builder)
    {
        // ViewRegistry
        builder.RegisterType<ViewRegistry>()
               .As<IViewRegistry>()
               .SingleInstance();

        // WindowManager - virtual method to allow custom implementations
        RegisterWindowManager(builder);

        builder.RegisterType<WindowContext>()
               .As<IWindowContext>()
               .InstancePerLifetimeScope();

        builder.RegisterType<WindowTracker>()
                   .As<IWindowTracker>()
                   .SingleInstance();

        // ISessionManager removed - use IScopeManager instead

        // ContentManager - per window scope
        builder.RegisterType<Navigator>()
               .As<INavigator>()
               .InstancePerLifetimeScope();
        builder.RegisterType<ContentManager>()
               .As<IContentManager>()
               .InstancePerLifetimeScope();

        builder.RegisterType<DialogHost>()
               .As<IDialogHost>()
               .InstancePerLifetimeScope();

        // Generic loggers
        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ContentManager>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<DialogHost>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ViewRegistry>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<Navigator>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<WindowContext>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ScopedWindowManager>>()).SingleInstance();
        builder.Register(c => Mock.Of<ILogger<WindowTracker>>()).SingleInstance();
        builder.Register(_ => Mock.Of<ILogger<WindowTracker>>()).InstancePerDependency();
        builder.Register(_ => Mock.Of<ILogger<ScopedWindowManager>>()).InstancePerDependency();
        
        // Loggers for new scope system
        builder.Register(_ => Mock.Of<ILogger<ScopedWindowManager>>()).SingleInstance();
        builder.Register(_ => Mock.Of<ILogger<ScopeManager>>()).SingleInstance();
        builder.Register(_ => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();

        //.InstancePerMatchingLifetimeScope((ILifetimeScope scope, Autofac.Core.IComponentRegistration request) =>
        //{
        //    var tag = scope.Tag?.ToString() ?? "";
        //    return tag.StartsWith("Window:");
        //});

        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();

        //Register generic ILogger<T> - returns mocked logger for any type
        builder.RegisterGeneric(typeof(Mock<>)).InstancePerDependency();
        builder.RegisterGeneric(typeof(MockLogger<>))
               .As(typeof(ILogger<>))
               .SingleInstance();
    }

    protected virtual IViewRegistry BuildViewRegistery()
    {

        if(!_container.TryResolve<IViewRegistry>(out var instance))
        {
            throw new InvalidOperationException("Cant resolve IViewRegistry");
        }
        else if(instance is null)
        {
            throw new ArgumentNullException("Resolve IViewRegistry is null");
        }

        return RegisterMapping(instance);

    }

    protected abstract IViewRegistry RegisterMapping(IViewRegistry viewRegistry);

    /// <summary>
    /// Register WindowManager implementation
    /// Override to use different WindowManager implementation (e.g., ScopedWindowManager)
    /// </summary>
    protected virtual void RegisterWindowManager(ContainerBuilder builder)
    {
        RegisterScopedWindowManager(builder);
        // Default: ScopedWindowManager with IScopeManager (replaces legacy WindowManager)
    }

    /// <summary>
    /// Override to use ScopedWindowManager instead of WindowManager
    /// </summary>
    protected void RegisterScopedWindowManager(ContainerBuilder builder)
    {
        // Register new IScopeManager
        builder.RegisterType<ScopeManager>()
               .As<IScopeManager>()
               .SingleInstance();
        
        // Register loggers for new components
        builder.Register(c => Mock.Of<ILogger<ScopedWindowManager>>()).SingleInstance();
        builder.Register(c => Mock.Of<ILogger<ScopeManager>>()).SingleInstance();
        builder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        
        // Register ScopedWindowManager instead of WindowManager
        builder.RegisterType<ScopedWindowManager>()
               .As<IScopedWindowManager>()
               .As<IWindowManager>()
               .SingleInstance();
    }

    /// <summary>
    /// Register test-specific services
    /// Override in derived classes to add test ViewModels, Views, etc.
    /// </summary>
    protected abstract void RegisterTestServices(ContainerBuilder builder);

    

    /// <summary>
    /// Creates a scoped lifetime for testing
    /// </summary>
    protected ILifetimeScope CreateScope(string tag)
    {
        return Scope.BeginLifetimeScope(tag);
    }

    /// <summary>
    /// Resolves service from current scope
    /// </summary>
    protected T Resolve<T>() where T : notnull
    {
        return Scope.Resolve<T>();
    }

    public virtual void Dispose()
    {
        _scope.Dispose();
        _container.Dispose();
    }
}

/// <summary>
/// Mock logger implementation for testing
/// </summary>
public class MockLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

/// <summary>
/// Extension methods for test assertions
/// </summary>
public static class TestExtensions
{
    /// <summary>
    /// Executes async method and waits for completion
    /// Useful for testing async void methods
    /// </summary>
    public static async Task<T> CaptureEventAsync<T>(
        Action subscribeAction,
        Action triggerAction,
        Func<Task<T>> getResultFunc,
        int timeoutMs = 1000)
    {
        var tcs = new TaskCompletionSource<T>();

        subscribeAction();

        triggerAction();

        var timeoutTask = Task.Delay(timeoutMs);
        var completedTask = await Task.WhenAny(getResultFunc(), timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException($"Event was not raised within {timeoutMs}ms");
        }

        return await getResultFunc();
    }
}

