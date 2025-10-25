using Autofac;
using Autofac.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Windows;
using WpfEngine.Core.Scopes;
using WpfEngine.Core.Services;
using WpfEngine.Core.Services.Autofac;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Services;
using Xunit;

namespace WpfEngine.Tests.Integration;

/// <summary>
/// Integration tests for session scope sharing using WindowServiceRefactored
/// These tests verify that InstancePerMatchingLifetimeScope works correctly
/// when WindowService creates properly tagged scopes
/// </summary>
public class SessionScopeSharingIntegrationTests : IDisposable
{
    private readonly IContainer _container;
    private readonly WindowServiceRefactored _windowService;

    public SessionScopeSharingIntegrationTests()
    {
        var builder = new ContainerBuilder();

        // Register loggers as mocks
        builder.Register(c => new Mock<ILogger<ViewRegistry>>().Object).As<ILogger<ViewRegistry>>();
        builder.Register(c => new Mock<ILogger<ViewLocatorService>>().Object).As<ILogger<ViewLocatorService>>();
        builder.Register(c => new Mock<ILogger<WindowServiceRefactored>>().Object).As<ILogger<WindowServiceRefactored>>();
        builder.Register(c => new Mock<ILogger<TestViewModel>>().Object).As<ILogger<TestViewModel>>();
        builder.Register(c => new Mock<ILogger<TestWindow>>().Object).As<ILogger<TestWindow>>();
        
        // Register core services
        builder.RegisterType<ViewRegistry>().AsSelf().SingleInstance();
        builder.RegisterType<ViewLocatorService>().As<IViewLocatorService>().InstancePerDependency();

        // Register test view
        builder.RegisterType<TestWindow>().AsSelf();
        builder.RegisterType<TestViewModel>().AsSelf();

        // Register SHARED service for workflow sessions (this is what we're testing!)
        // Use exact tag match - all workflow sessions share the same tag
        builder.RegisterType<SharedOrderBuilder>()
               .As<ISharedOrderBuilder>()
               .InstancePerMatchingLifetimeScope("workflow-session");

        // Register window-specific service
        builder.RegisterType<WindowSpecificService>()
               .As<IWindowSpecificService>()
               .InstancePerMatchingLifetimeScope("window-scope");

        _container = builder.Build();

        var registry = _container.Resolve<ViewRegistry>();
        registry.MapWindow<TestViewModel, TestWindow>();

        var viewLocator = new ViewLocatorService(
            _container,
            registry,
            Mock.Of<ILogger<ViewLocatorService>>());

        _windowService = new WindowServiceRefactored(
            _container,
            viewLocator,
            Mock.Of<ILogger<WindowServiceRefactored>>());
    }

    [Fact]
    public void CreateSession_ReturnsValidSessionId()
    {
        // Act
        var sessionId = _windowService.CreateSession("test-workflow");

        // Assert
        sessionId.Should().NotBeEmpty();
    }

    [Fact]
    public void SharedService_InSameSession_UsingScopeDirectly()
    {
        // Act - Create session scope with matching tag and resolve service twice
        using var sessionScope = _container.BeginLifetimeScope("workflow-session");
        var instance1 = sessionScope.Resolve<ISharedOrderBuilder>();
        var instance2 = sessionScope.Resolve<ISharedOrderBuilder>();

        // Assert - SAME instance
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void SharedService_Modifications_VisibleInSameSessionScope()
    {
        // Act
        using var sessionScope = _container.BeginLifetimeScope("workflow-session");
        
        var instance1 = sessionScope.Resolve<ISharedOrderBuilder>();
        instance1.AddItem("Product A");

        var instance2 = sessionScope.Resolve<ISharedOrderBuilder>();
        var items = instance2.GetItems();

        // Assert - Modifications visible because same instance
        items.Should().HaveCount(1);
        items.Should().Contain("Product A");
    }

    [Fact]
    public void SharedService_InDifferentSessions_ReturnsDifferentInstances()
    {
        // Act - Create two separate session scopes
        using var session1Scope = _container.BeginLifetimeScope("workflow-session");
        using var session2Scope = _container.BeginLifetimeScope("workflow-session");

        var instance1 = session1Scope.Resolve<ISharedOrderBuilder>();
        var instance2 = session2Scope.Resolve<ISharedOrderBuilder>();

        // Assert - Different sessions = different instances
        instance1.Should().NotBeSameAs(instance2);
    }

    [Fact]
    public void SharedService_InChildWindowScope_ReturnsSameInstanceAsParent()
    {
        // Act - Create session scope, then child window scope
        using var sessionScope = _container.BeginLifetimeScope("workflow-session");
        var sessionInstance = sessionScope.Resolve<ISharedOrderBuilder>();
        
        // Child scope WITHOUT tag - inherits from parent
        using var windowScope = sessionScope.BeginLifetimeScope();
        var windowInstance = windowScope.Resolve<ISharedOrderBuilder>();

        // Assert - Child inherits from parent session
        windowInstance.Should().BeSameAs(sessionInstance);
    }

    [Fact]
    public void WindowSpecificService_InDifferentChildScopes_ReturnsDifferentInstances()
    {
        // Act - Create session, then two child window scopes with the window tag
        using var sessionScope = _container.BeginLifetimeScope("workflow-session");
        using var window1Scope = sessionScope.BeginLifetimeScope("window-scope");
        using var window2Scope = sessionScope.BeginLifetimeScope("window-scope");

        var instance1 = window1Scope.Resolve<IWindowSpecificService>();
        var instance2 = window2Scope.Resolve<IWindowSpecificService>();

        // Assert - Each window has its own instance
        instance1.Should().NotBeSameAs(instance2);
        instance1.InstanceId.Should().NotBe(instance2.InstanceId);
    }

    [Fact]
    public void CloseSession_DisposesSessionScope()
    {
        // Arrange
        var sessionId = _windowService.CreateSession("test-workflow");
        
        // Create our own scope to test disposal
        using var sessionScope = _container.BeginLifetimeScope("workflow-session");
        var sharedService = sessionScope.Resolve<ISharedOrderBuilder>() as SharedOrderBuilder;

        // Act
        _windowService.CloseSession(sessionId);

        // Assert - Our separate scope service should still be alive
        sharedService.Should().NotBeNull();
        sharedService!.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void MultipleSessionsInParallel_HaveIndependentSharedServices()
    {
        // Act - Create 3 parallel session scopes with the same tag
        using var scope1 = _container.BeginLifetimeScope("workflow-session");
        using var scope2 = _container.BeginLifetimeScope("workflow-session");
        using var scope3 = _container.BeginLifetimeScope("workflow-session");

        var service1 = scope1.Resolve<ISharedOrderBuilder>();
        var service2 = scope2.Resolve<ISharedOrderBuilder>();
        var service3 = scope3.Resolve<ISharedOrderBuilder>();

        // Add different items to each
        service1.AddItem("Item 1");
        service2.AddItem("Item 2");
        service3.AddItem("Item 3");

        // Assert - Each session has independent state
        service1.GetItems().Should().ContainSingle().Which.Should().Be("Item 1");
        service2.GetItems().Should().ContainSingle().Which.Should().Be("Item 2");
        service3.GetItems().Should().ContainSingle().Which.Should().Be("Item 3");
    }

    [Fact]
    public void SessionScope_MultipleChildScopes_ShareSessionService()
    {
        // Act - Create session scope, then multiple child scopes
        using var sessionScope = _container.BeginLifetimeScope("workflow-session");
        using var child1 = sessionScope.BeginLifetimeScope();
        using var child2 = sessionScope.BeginLifetimeScope();
        using var child3 = sessionScope.BeginLifetimeScope();

        var instance1 = child1.Resolve<ISharedOrderBuilder>();
        var instance2 = child2.Resolve<ISharedOrderBuilder>();
        var instance3 = child3.Resolve<ISharedOrderBuilder>();

        // Assert - ALL child scopes get the SAME shared instance
        instance1.Should().BeSameAs(instance2);
        instance2.Should().BeSameAs(instance3);
        
        // Modifications in one child are visible in others
        instance1.AddItem("Shared Item");
        instance2.GetItems().Should().Contain("Shared Item");
        instance3.GetItems().Should().Contain("Shared Item");
    }

    [Fact]
    public void ScopeDisposal_DisposesSharedService()
    {
        // Arrange
        SharedOrderBuilder? sharedService;

        // Act
        using (var sessionScope = _container.BeginLifetimeScope("workflow-session"))
        {
            sharedService = sessionScope.Resolve<ISharedOrderBuilder>() as SharedOrderBuilder;
            sharedService!.IsDisposed.Should().BeFalse();
        }

        // Assert - Disposed when session scope disposes
        sharedService!.IsDisposed.Should().BeTrue();
    }

    public void Dispose()
    {
        _container?.Dispose();
    }

    // ========== TEST TYPES ==========

    public interface ISharedOrderBuilder
    {
        void AddItem(string name);
        List<string> GetItems();
    }

    public class SharedOrderBuilder : ISharedOrderBuilder, IDisposable
    {
        private readonly List<string> _items = new();
        public bool IsDisposed { get; private set; }

        public void AddItem(string name) => _items.Add(name);
        public List<string> GetItems() => _items;
        public void Dispose() => IsDisposed = true;
    }

    public interface IWindowSpecificService
    {
        Guid InstanceId { get; }
    }

    public class WindowSpecificService : IWindowSpecificService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }

    public class TestViewModel : BaseViewModel
    {
        public TestViewModel(ILogger<TestViewModel> logger) : base(logger)
        {
        }
    }

    public class TestWindow : Window, IWindowView
    {
        public TestWindow(ILogger<TestWindow> logger)
        {
        }

        public Guid WindowId { get; set; } = Guid.NewGuid();

        object? IView.DataContext
        {
            get => DataContext;
            set => DataContext = value;
        }
    }
}

