using Autofac;
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
using WpfEngine.Services.Autofac;
using Xunit;

namespace WpfEngine.Tests.Core.Services.Autofac;

/// <summary>
/// Tests for WindowServiceRefactored - hierarchical scope support
/// Note: These are integration tests that require full Autofac container setup
/// </summary>
public class WindowServiceRefactoredTests : IDisposable
{
    private readonly IContainer _container;
    private readonly WindowServiceRefactored _windowService;
    private readonly Mock<ILogger<WindowServiceRefactored>> _loggerMock;

    public WindowServiceRefactoredTests()
    {
        var builder = new ContainerBuilder();

        // Register loggers as mocks
        builder.Register(c => new Mock<ILogger<ViewRegistry>>().Object).As<ILogger<ViewRegistry>>();
        builder.Register(c => new Mock<ILogger<ViewLocatorService>>().Object).As<ILogger<ViewLocatorService>>();
        builder.Register(c => new Mock<ILogger<ViewModelFactory>>().Object).As<ILogger<ViewModelFactory>>();
        builder.Register(c => new Mock<ILogger<TestWindow>>().Object).As<ILogger<TestWindow>>();

        // Register ViewRegistry and test ViewModels
        builder.RegisterType<ViewRegistry>()
               .AsSelf()
               .As<IViewRegistry>()
               .SingleInstance();

        builder.RegisterType<TestViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestWindow>().AsSelf().InstancePerDependency();

        // Register ViewLocator
        builder.RegisterType<ViewLocatorService>()
               .As<IViewLocatorService>()
               .InstancePerLifetimeScope();

        _container = builder.Build();

        // Configure mappings
        var registry = _container.Resolve<IViewRegistry>();
        registry.MapWindow<TestViewModel, TestWindow>();

        _loggerMock = new Mock<ILogger<WindowServiceRefactored>>();

        var viewLocator = _container.Resolve<IViewLocatorService>();
        _windowService = new WindowServiceRefactored(_container, viewLocator, _loggerMock.Object);
    }

    [Fact]
    public void CreateSession_CreatesNewSession()
    {
        // Act
        var sessionId = _windowService.CreateSession("test-session");

        // Assert
        sessionId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateSession_CreatesDifferentSessionsWithDifferentIds()
    {
        // Act
        var sessionId1 = _windowService.CreateSession("test-session-1");
        var sessionId2 = _windowService.CreateSession("test-session-2");

        // Assert
        sessionId1.Should().NotBe(sessionId2);
    }

    [Fact]
    public void CloseSession_RemovesSession()
    {
        // Arrange
        var sessionId = _windowService.CreateSession("test-session");

        // Act
        _windowService.CloseSession(sessionId);

        // Assert
        // Trying to open window in closed session should throw
        var act = () => _windowService.OpenWindowInSession<TestViewModel>(sessionId);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage($"Session {sessionId} not found");
    }

    [Fact]
    public void OpenWindowInSession_ThrowsIfSessionNotFound()
    {
        // Arrange
        var nonExistentSessionId = Guid.NewGuid();

        // Act
        var act = () => _windowService.OpenWindowInSession<TestViewModel>(nonExistentSessionId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage($"Session {nonExistentSessionId} not found");
    }

    public void Dispose()
    {
        _container?.Dispose();
    }

    // ========== TEST TYPES ==========

    public class TestViewModel : IViewModel
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public Task InitializeAsync() => Task.CompletedTask;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestWindow : Window, IWindowView
    {
        private readonly Guid _windowId = Guid.NewGuid();
        
        public TestWindow(ILifetimeScope parentScope, ILogger<TestWindow> logger)
        {
            // Simulate ScopedWindow behavior
        }

        public Guid WindowId => _windowId;
    }
}

