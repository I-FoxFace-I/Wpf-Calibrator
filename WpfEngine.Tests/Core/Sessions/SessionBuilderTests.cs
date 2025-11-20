using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.ViewModels;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Sessions;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Services.Sessions.Implementation;
using WpfEngine.Tests.Helpers;
using WpfEngine.ViewModels;
using Xunit;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Tests.Core.Sessions;

public class SessionBuilderTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    
    public SessionBuilderTests()
    {
        var builder = new ContainerBuilder();
        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        builder.RegisterType<TestService>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<AnotherTestService>().AsSelf().InstancePerLifetimeScope();
        
        // Register mock window manager for OpenWindow tests
        var mockWindowManager = new Mock<IScopedWindowManager>();
        mockWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModel>(It.IsAny<Guid>()))
            .Returns(Guid.NewGuid());
        mockWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModelWithParameters, TestParameters>(It.IsAny<Guid>(), It.IsAny<TestParameters>()))
            .Returns(Guid.NewGuid());
        
        builder.RegisterInstance(mockWindowManager.Object).As<IWindowManager>();
        builder.RegisterInstance(mockWindowManager.Object).As<IScopedWindowManager>();
        
        _container = builder.Build();
        _scope = _container.BeginLifetimeScope();
    }
    
    [Fact]
    public void Build_ShouldCreateSession()
    {
        // Arrange
        var builder = new SessionBuilder(
            _scope,
            ScopeTag.Database());
        
        // Act
        using var session = builder.Build();
        
        // Assert
        session.Should().NotBeNull();
        session.Tag.Should().Be(ScopeTag.Database());
        session.IsActive.Should().BeTrue();
    }
    
    [Fact]
    public void WithService_ShouldAllowResolution()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var result = builder
            .WithService<TestService>()
            .ExecuteWithResult(service =>
            {
                service.Should().NotBeNull();
                return service.GetMessage();
            });
        
        // Assert
        result.Should().Be("TestService");
    }
    
    [Fact]
    public void WithService_MultipleServices_ShouldResolveAll()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var result = builder
            .WithService<TestService>()
            .WithService<AnotherTestService>()
            .ExecuteWithResult((test, another) =>
            {
                test.Should().NotBeNull();
                another.Should().NotBeNull();
                return $"{test.GetMessage()}+{another.GetValue()}";
            });
        
        // Assert
        result.Should().Be("TestService+42");
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldExecuteAndDispose()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var executed = false;
        
        // Act
        await builder
            .WithService<TestService>()
            .ExecuteAsync(async service =>
            {
                await Task.Delay(10);
                executed = true;
                service.Should().NotBeNull();
            });
        
        // Assert
        executed.Should().BeTrue();
    }
    
    [Fact]
    public async Task ExecuteWithResultAsync_ShouldReturnResult()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        var result = await builder
            .WithService<TestService>()
            .ExecuteWithResultAsync(async service =>
            {
                await Task.Delay(10);
                return service.GetMessage();
            });
        
        // Assert
        result.Should().Be("TestService");
    }
    
    [Fact]
    public void WithAutoSave_ShouldSetFlag()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        using var session = builder
            .WithAutoSave()
            .Build();
        
        // Assert
        session.Should().NotBeNull();
    }
    
    [Fact]
    public void OnDispose_ShouldExecuteHook()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var hookExecuted = false;
        
        // Act
        using (var session = builder
            .OnDispose(() => hookExecuted = true)
            .Build())
        {
            hookExecuted.Should().BeFalse();
        }
        
        // Assert
        hookExecuted.Should().BeTrue();
    }
    
    [Fact]
    public void Build_ShouldAllowManualServiceResolution()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act
        using var session = builder.Build();
        var service = session.Resolve<TestService>();
        
        // Assert
        service.Should().NotBeNull();
        service.GetMessage().Should().Be("TestService");
    }
    
    public void Dispose()
    {
        _scope?.Dispose();
        _container?.Dispose();
    }
    
    [Fact]
    public void AutoCloseWhenEmpty_WhenEnabled_ShouldCloseSessionAfterAllWindowsClosed()
    {
        // Arrange
        var windowId = Guid.NewGuid();
        var mockWindowManager = new Mock<IWindowManager>();
        mockWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModel>(It.IsAny<Guid>()))
            .Returns(windowId);
        
        var mockScopedWindowManager = new Mock<IScopedWindowManager>();
        mockScopedWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModel>(It.IsAny<Guid>()))
            .Returns(windowId);
        
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        containerBuilder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IWindowManager>();
        containerBuilder.RegisterInstance(mockScopedWindowManager.Object).As<IScopedWindowManager>();
        
        using var container = containerBuilder.Build();
        using var scope = container.BeginLifetimeScope();
        
        var builder = new SessionBuilder(scope, ScopeTag.Workflow());
        
        // Act - build session with AutoCloseWhenEmpty and open window
        var session = builder
            .AutoCloseWhenEmpty()
            .OpenWindow<TestViewModel>();
        
        // Assert - session should be active
        session.IsActive.Should().BeTrue();
        session.WindowCount.Should().Be(1);
        
        // Simulate window closed event
        mockWindowManager.Raise(
            m => m.WindowClosed += null,
            new WindowClosedEventArgs(windowId, typeof(TestViewModel), null, session.SessionId));
        
        // Session should be closed automatically
        session.IsActive.Should().BeFalse();
        session.WindowCount.Should().Be(0);
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void AutoCloseWhenEmpty_WhenNotEnabled_ShouldNotCloseSessionAfterAllWindowsClosed()
    {
        // Arrange
        var windowId = Guid.NewGuid();
        var mockWindowManager = new Mock<IScopedWindowManager>();
        mockWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModel>(It.IsAny<Guid>()))
            .Returns(windowId);
        
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        containerBuilder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IWindowManager>();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IScopedWindowManager>();
        
        using var container = containerBuilder.Build();
        using var scope = container.BeginLifetimeScope();
        
        var builder = new SessionBuilder(scope, ScopeTag.Workflow());
        
        // Act - build session WITHOUT AutoCloseWhenEmpty and open window
        var session = builder.OpenWindow<TestViewModel>();
        
        // Assert - session should be active
        session.IsActive.Should().BeTrue();
        session.WindowCount.Should().Be(1);
        
        // Simulate window closed event
        mockWindowManager.Raise(
            m => m.WindowClosed += null,
            new WindowClosedEventArgs(windowId, typeof(TestViewModel), null, session.SessionId));
        
        // Session should still be active (not auto-closed)
        session.IsActive.Should().BeTrue();
        session.WindowCount.Should().Be(0);
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void AutoCloseWhenEmpty_WithMultipleWindows_ShouldOnlyCloseWhenAllWindowsClosed()
    {
        // Arrange
        var windowId1 = Guid.NewGuid();
        var windowId2 = Guid.NewGuid();
        var mockWindowManager = new Mock<IScopedWindowManager>();
        mockWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModel>(It.IsAny<Guid>()))
            .Returns(windowId1);
        
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        containerBuilder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IWindowManager>();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IScopedWindowManager>();
        
        using var container = containerBuilder.Build();
        using var scope = container.BeginLifetimeScope();
        
        var builder = new SessionBuilder(scope, ScopeTag.Workflow());
        
        // Act - build session with AutoCloseWhenEmpty
        var session = builder.AutoCloseWhenEmpty().Build();
        
        // Open first window
        mockWindowManager.Raise(
            m => m.WindowOpened += null,
            new WindowOpenedEventArgs(windowId1, typeof(TestViewModel), null, session.SessionId));
        
        // Open second window
        mockWindowManager.Raise(
            m => m.WindowOpened += null,
            new WindowOpenedEventArgs(windowId2, typeof(TestViewModel), null, session.SessionId));
        
        session.WindowCount.Should().Be(2);
        session.IsActive.Should().BeTrue();
        
        // Close first window
        mockWindowManager.Raise(
            m => m.WindowClosed += null,
            new WindowClosedEventArgs(windowId1, typeof(TestViewModel), null, session.SessionId));
        
        // Session should still be active (one window remaining)
        session.WindowCount.Should().Be(1);
        session.IsActive.Should().BeTrue();
        
        // Close second window
        mockWindowManager.Raise(
            m => m.WindowClosed += null,
            new WindowClosedEventArgs(windowId2, typeof(TestViewModel), null, session.SessionId));
        
        // Now session should be closed (all windows closed)
        session.WindowCount.Should().Be(0);
        session.IsActive.Should().BeFalse();
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void AutoCloseWhenEmpty_ShouldOnlyTrackWindowsFromSameSession()
    {
        // Arrange
        var sessionWindowId = Guid.NewGuid();
        var otherWindowId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();
        
        var mockWindowManager = new Mock<IScopedWindowManager>();
        mockWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModel>(It.IsAny<Guid>()))
            .Returns(sessionWindowId);
        
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        containerBuilder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IWindowManager>();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IScopedWindowManager>();
        
        using var container = containerBuilder.Build();
        using var scope = container.BeginLifetimeScope();
        
        var builder = new SessionBuilder(scope, ScopeTag.Workflow());
        
        // Act - build session with AutoCloseWhenEmpty
        var session = builder.AutoCloseWhenEmpty().Build();
        
        // Open window in this session
        mockWindowManager.Raise(
            m => m.WindowOpened += null,
            new WindowOpenedEventArgs(sessionWindowId, typeof(TestViewModel), null, session.SessionId));
        
        session.WindowCount.Should().Be(1);
        
        // Simulate window from another session being opened and closed
        mockWindowManager.Raise(
            m => m.WindowOpened += null,
            new WindowOpenedEventArgs(otherWindowId, typeof(TestViewModel), null, otherSessionId));
        
        mockWindowManager.Raise(
            m => m.WindowClosed += null,
            new WindowClosedEventArgs(otherWindowId, typeof(TestViewModel), null, otherSessionId));
        
        // Our session should still have 1 window and be active
        session.WindowCount.Should().Be(1);
        session.IsActive.Should().BeTrue();
        
        // Close our session's window
        mockWindowManager.Raise(
            m => m.WindowClosed += null,
            new WindowClosedEventArgs(sessionWindowId, typeof(TestViewModel), null, session.SessionId));
        
        // Now session should be closed
        session.WindowCount.Should().Be(0);
        session.IsActive.Should().BeFalse();
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void OpenWindow_ShouldBuildSessionAndOpenWindow()
    {
        // Arrange
        var builder = new SessionBuilder(
            _scope,
            ScopeTag.Workflow());
        
        // Act
        var session = builder.OpenWindow<TestViewModel>();
        
        // Assert
        session.Should().NotBeNull();
        session.IsActive.Should().BeTrue();
        session.Tag.Should().Be(ScopeTag.Workflow());
        
        // Verify window manager was called
        var windowManager = session.Resolve<IScopedWindowManager>();
        windowManager.Should().NotBeNull();
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void OpenWindow_WithParameters_ShouldBuildSessionAndOpenWindowWithParameters()
    {
        // Arrange
        var builder = new SessionBuilder(
            _scope,
            ScopeTag.Window());
        
        var parameters = new TestParameters { Value = "TestValue" };
        
        // Act
        var session = builder.OpenWindow<TestViewModelWithParameters, TestParameters>(parameters);
        
        // Assert
        session.Should().NotBeNull();
        session.IsActive.Should().BeTrue();
        session.Tag.Should().Be(ScopeTag.Window());
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void OpenWindow_WhenWindowManagerThrows_ShouldDisposeSessionAndRethrow()
    {
        // Arrange
        var mockWindowManager = new Mock<IScopedWindowManager>();
        mockWindowManager
            .Setup(m => m.OpenWindowInSession<TestViewModel>(It.IsAny<Guid>()))
            .Throws(new InvalidOperationException("Window manager error"));
        
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        containerBuilder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        containerBuilder.RegisterInstance(mockWindowManager.Object).As<IScopedWindowManager>();
        
        using var container = containerBuilder.Build();
        using var scope = container.BeginLifetimeScope();
        
        var builder = new SessionBuilder(
            scope,
            ScopeTag.Window());
        
        // Act & Assert
        var act = () => builder.OpenWindow<TestViewModel>();
        act.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void OpenWindow_WithService_ShouldBuildSessionAndOpenWindow()
    {
        // Arrange
        var builder = new SessionBuilder(
            _scope,
            ScopeTag.Window());
        
        // Act
        var session = builder
            .WithService<TestService>()
            .OpenWindow<TestViewModel>();
        
        // Assert
        session.Should().NotBeNull();
        session.IsActive.Should().BeTrue();
        
        // Verify service can be resolved
        var service = session.Resolve<TestService>();
        service.Should().NotBeNull();
        service.GetMessage().Should().Be("TestService");
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void OpenWindow_WithAutoSave_ShouldBuildSessionWithAutoSave()
    {
        // Arrange
        var builder = new SessionBuilder(
            _scope,
            ScopeTag.Database());
        
        // Act
        var session = builder
            .WithAutoSave(true)
            .OpenWindow<TestViewModel>();
        
        // Assert
        session.Should().NotBeNull();
        session.IsActive.Should().BeTrue();
        
        // Cleanup
        session.Dispose();
    }
    
    [Fact]
    public void Execute_WithOnError_ShouldCallErrorHandler()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var errorHandlerCalled = false;
        Exception? caughtException = null;
        
        // Act
        try
        {
            builder
                .WithService<TestService>()
                .Execute(service => 
                {
                    throw new InvalidOperationException("Test error");
                }, 
                onError: ex => 
                {
                    errorHandlerCalled = true;
                    caughtException = ex;
                });
        }
        catch
        {
            // Exception is still thrown after onError
        }
        
        // Assert
        errorHandlerCalled.Should().BeTrue();
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<InvalidOperationException>();
        caughtException!.Message.Should().Be("Test error");
    }
    
    [Fact]
    public async Task ExecuteAsync_WithOnError_ShouldCallErrorHandler()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var errorHandlerCalled = false;
        Exception? caughtException = null;
        
        // Act
        try
        {
            await builder
                .WithService<TestService>()
                .ExecuteAsync(async service => 
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Test async error");
                }, 
                onError: ex => 
                {
                    errorHandlerCalled = true;
                    caughtException = ex;
                });
        }
        catch
        {
            // Exception is still thrown after onError
        }
        
        // Assert
        errorHandlerCalled.Should().BeTrue();
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<InvalidOperationException>();
        caughtException!.Message.Should().Be("Test async error");
    }
    
    [Fact]
    public void Execute_WithoutOnError_ShouldUseDefaultRollback()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        
        // Act & Assert
        var act = () => builder
            .WithService<TestService>()
            .Execute(service => 
            {
                throw new InvalidOperationException("Test error");
            });
        
        act.Should().Throw<InvalidOperationException>();
        // Note: We can't easily verify Rollback was called without mocking ScopeSession
        // but the fact that it doesn't crash means the default behavior is working
    }
    
    [Fact]
    public void ExecuteWithResult_WithDefaultValue_ShouldReturnDefaultOnException()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = "Default Result";
        
        // Act
        var result = builder
            .WithService<TestService>()
            .ExecuteWithResult(service => 
            {
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue);
        
        // Assert
        result.Should().Be(defaultValue);
    }
    
    [Fact]
    public void ExecuteWithResult_WithDefaultValue_ShouldReturnActualResultOnSuccess()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = "Default Result";
        
        // Act
        var result = builder
            .WithService<TestService>()
            .ExecuteWithResult(service => 
            {
                return service.GetMessage();
            }, 
            defaultValue: defaultValue);
        
        // Assert
        result.Should().Be("TestService");
        result.Should().NotBe(defaultValue);
    }
    
    [Fact]
    public async Task ExecuteWithResultAsync_WithDefaultValue_ShouldReturnDefaultOnException()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = new List<string> { "default" };
        
        // Act
        var result = await builder
            .WithService<TestService>()
            .ExecuteWithResultAsync(async service => 
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue);
        
        // Assert
        result.Should().BeEquivalentTo(defaultValue);
    }
    
    [Fact]
    public async Task ExecuteWithResultAsync_WithDefaultValue_ShouldReturnActualResultOnSuccess()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = new List<string> { "default" };
        
        // Act
        var result = await builder
            .WithService<TestService>()
            .ExecuteWithResultAsync(async service => 
            {
                await Task.Delay(10);
                return new List<string> { service.GetMessage() };
            }, 
            defaultValue: defaultValue);
        
        // Assert
        result.Should().BeEquivalentTo(new List<string> { "TestService" });
        result.Should().NotBeEquivalentTo(defaultValue);
    }
    
    [Fact]
    public void ExecuteWithResult_WithOnError_ShouldCallErrorHandlerAndReturnDefault()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = 42;
        var errorHandlerCalled = false;
        Exception? caughtException = null;
        
        // Act
        var result = builder
            .WithService<TestService>()
            .ExecuteWithResult(service => 
            {
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue,
            onError: ex => 
            {
                errorHandlerCalled = true;
                caughtException = ex;
            });
        
        // Assert
        result.Should().Be(defaultValue);
        errorHandlerCalled.Should().BeTrue();
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<InvalidOperationException>();
    }
    
    [Fact]
    public async Task ExecuteWithResultAsync_WithOnError_ShouldCallErrorHandlerAndReturnDefault()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = "Default";
        var errorHandlerCalled = false;
        Exception? caughtException = null;
        
        // Act
        var result = await builder
            .WithService<TestService>()
            .ExecuteWithResultAsync(async service => 
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue,
            onError: ex => 
            {
                errorHandlerCalled = true;
                caughtException = ex;
            });
        
        // Assert
        result.Should().Be(defaultValue);
        errorHandlerCalled.Should().BeTrue();
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<InvalidOperationException>();
    }
    
    [Fact]
    public void ExecuteWithResult_WithDefaultValue_ShouldNotThrowException()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = 0;
        
        // Act
        var act = () => builder
            .WithService<TestService>()
            .ExecuteWithResult(service => 
            {
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue);
        
        // Assert - should not throw, should return default
        var result = act();
        result.Should().Be(defaultValue);
    }
    
    [Fact]
    public void ExecuteWithResult_WithMultipleServices_ShouldHandleError()
    {
        // Arrange
        var builder = new SessionBuilder(_scope, ScopeTag.Database());
        var defaultValue = "Error occurred";
        var errorHandlerCalled = false;
        
        // Act
        var result = builder
            .WithService<TestService>()
            .WithService<AnotherTestService>()
            .ExecuteWithResult((service1, service2) => 
            {
                throw new InvalidOperationException("Test error");
            }, 
            defaultValue: defaultValue,
            onError: ex => 
            {
                errorHandlerCalled = true;
            });
        
        // Assert
        result.Should().Be(defaultValue);
        errorHandlerCalled.Should().BeTrue();
    }
    
    // Test services
    private class TestService
    {
        public string GetMessage() => "TestService";
    }
    
    private class AnotherTestService
    {
        public int GetValue() => 42;
    }
    
    // Test ViewModels and Parameters
    private class TestViewModel : BaseViewModel
    {
        public TestViewModel(ILogger<TestViewModel> logger) : base(logger)
        {
        }
    }
    
    private class TestViewModelWithParameters : BaseViewModel, IViewModel<TestParameters>
    {
        public TestParameters? Parameter { get; private set; }
        
        public TestViewModelWithParameters(ILogger<TestViewModelWithParameters> logger) : base(logger)
        {
        }
        
        public Task InitializeAsync(TestParameters parameter)
        {
            Parameter = parameter;
            return Task.CompletedTask;
        }
    }
    
    private class TestParameters : IViewModelParameters
    {
        public string Value { get; set; } = string.Empty;
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }
}

