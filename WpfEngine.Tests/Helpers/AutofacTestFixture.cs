using Autofac;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Core.Services;
using WpfEngine.Core.Services.Autofac;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;

namespace WpfEngine.Tests.Helpers;

/// <summary>
/// Base test fixture for Autofac-based tests
/// Provides common container setup and cleanup
/// </summary>
public abstract class AutofacTestFixture : IDisposable
{
    protected IContainer Container { get; private set; }
    protected ILifetimeScope Scope { get; private set; }

    protected AutofacTestFixture()
    {
        Container = BuildContainer();
        Scope = Container.BeginLifetimeScope();
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
               .AsSelf()
               .As<IViewRegistry>()
               .SingleInstance();

        // ViewLocator
        builder.RegisterType<ViewLocatorService>()
               .As<IViewLocatorService>()
               .InstancePerLifetimeScope();

        // ViewModelFactory
        builder.RegisterType<ViewModelFactory>()
               .As<IViewModelFactory>()
               .InstancePerLifetimeScope();

        // ContentManager - per window scope
        builder.RegisterType<ContentManager>()
               .As<IContentManager>()
               .InstancePerMatchingLifetimeScope((ILifetimeScope scope, Autofac.Core.IComponentRegistration request) =>
               {
                   var tag = scope.Tag?.ToString() ?? "";
                   return tag.StartsWith("Window:");
               });

        // Register generic ILogger<T> - returns mocked logger for any type
        builder.RegisterGeneric(typeof(Mock<>)).InstancePerDependency();
        builder.RegisterGeneric(typeof(MockLogger<>))
               .As(typeof(ILogger<>))
               .SingleInstance();
    }

    /// <summary>
    /// Register test-specific services
    /// Override in derived classes to add test ViewModels, Views, etc.
    /// </summary>
    protected virtual void RegisterTestServices(ContainerBuilder builder)
    {
        // Override in derived classes
    }

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
        Scope?.Dispose();
        Container?.Dispose();
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

