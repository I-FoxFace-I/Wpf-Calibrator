using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Core.Services;
using WpfEngine.Core.Services.Autofac;
using WpfEngine.Services.WindowTracking;
using Xunit;

namespace WpfEngine.Tests.Core.Services.Autofac;

/// <summary>
/// Advanced tests for WindowServiceRefactored
/// Tests focusing on edge cases, lifecycle, and cleanup
/// </summary>
public class WindowServiceAdvancedTests : IDisposable
{
    private readonly IContainer _container;
    private readonly Mock<IViewLocatorService> _viewLocatorMock;
    private readonly WindowServiceRefactored _windowService;

    public WindowServiceAdvancedTests()
    {
        var builder = new ContainerBuilder();
        _container = builder.Build();

        _viewLocatorMock = new Mock<IViewLocatorService>();
        _windowService = new WindowServiceRefactored(
            _container,
            _viewLocatorMock.Object,
            Mock.Of<ILogger<WindowServiceRefactored>>());
    }

    [Fact]
    public void CreateSession_GeneratesUniqueIds()
    {
        // Act
        var session1 = _windowService.CreateSession("session-1");
        var session2 = _windowService.CreateSession("session-2");
        var session3 = _windowService.CreateSession("session-1"); // Same name, different ID

        // Assert
        session1.Should().NotBe(session2);
        session2.Should().NotBe(session3);
        session1.Should().NotBe(session3);
    }

    [Fact]
    public void CloseSession_WithNonExistentId_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = () => _windowService.CloseSession(nonExistentId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CloseSession_CanBeCalledMultipleTimes()
    {
        // Arrange
        var sessionId = _windowService.CreateSession("test-session");

        // Act
        _windowService.CloseSession(sessionId);
        var act = () => _windowService.CloseSession(sessionId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void OpenWindowInSession_WithClosedSession_ThrowsException()
    {
        // Arrange
        var sessionId = _windowService.CreateSession("test-session");
        _windowService.CloseSession(sessionId);

        // Act
        var act = () => _windowService.OpenWindowInSession<TestViewModel>(sessionId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage($"Session {sessionId} not found");
    }

    [Fact]
    public void Close_WithNonExistentWindowId_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = () => _windowService.Close(nonExistentId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Close_WithNonExistentVmKey_DoesNotThrow()
    {
        // Arrange
        var nonExistentKey = new VmKey(Guid.NewGuid(), typeof(TestViewModel));

        // Act
        var act = () => _windowService.Close(nonExistentKey);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WindowClosed_Event_CanBeSubscribedAndUnsubscribed()
    {
        // Arrange
        var eventFiredCount = 0;
        EventHandler<WindowClosedEventArgs> handler = (s, e) => eventFiredCount++;

        // Act - Subscribe
        _windowService.WindowClosed += handler;
        
        // Act - Unsubscribe
        _windowService.WindowClosed -= handler;

        // Assert - Should not throw
        eventFiredCount.Should().Be(0);
    }

    public void Dispose()
    {
        _container?.Dispose();
    }

    // ========== TEST TYPES ==========

    public class TestViewModel : WpfEngine.Core.ViewModels.IViewModel
    {
        public Guid Id => Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public Task InitializeAsync() => Task.CompletedTask;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}

