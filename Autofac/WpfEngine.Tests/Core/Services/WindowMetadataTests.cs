using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Windows;
using Xunit;
using WpfEngine.Tests.Helpers;
using Moq;
using Castle.Core.Logging;
using WpfEngine.ViewModels;
using WpfEngine.Services.Autofac;
using WpfEngine.Enums;
using WpfEngine.Services.Metadata;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for WindowMetadata - lifecycle management and state tracking
/// </summary>
public class WindowMetadataTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultState()
    {
        // Act
        var metadata = new WindowMetadata
        {
            WindowId = Guid.NewGuid(),
            ViewModelType = typeof(TestViewModel)
        };

        // Assert
        metadata.WindowId.Should().NotBeEmpty();
        metadata.Lifecycle.Should().Be(WindowLifecycleState.Creating);
        metadata.CreatedThreadId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void IsOpened_WhenCreating_ShouldReturnFalse()
    {
        // Arrange
        var metadata = new WindowMetadata
        {
            Lifecycle = WindowLifecycleState.Creating
        };

        // Act
        var isOpened = metadata.IsOpened();

        // Assert
        isOpened.Should().BeFalse();
    }

    [Fact]
    public void IsOpened_WhenOpen_ShouldReturnTrue()
    {
        // Arrange
        var metadata = new WindowMetadata
        {
            Lifecycle = WindowLifecycleState.Open
        };

        // Act
        var isOpened = metadata.IsOpened();

        // Assert
        isOpened.Should().BeTrue();
    }

    [Fact]
    public void IsOpened_WhenClosed_ShouldReturnFalse()
    {
        // Arrange
        var metadata = new WindowMetadata();
        metadata.SetClosed();

        // Act
        var isOpened = metadata.IsOpened();

        // Assert
        isOpened.Should().BeFalse();
    }

    [Fact]
    public void SetClosed_ShouldUpdateState()
    {
        // Arrange
        var metadata = new WindowMetadata
        {
            Lifecycle = WindowLifecycleState.Open
        };

        // Act
        metadata.SetClosed();

        // Assert
        metadata.Lifecycle.Should().Be(WindowLifecycleState.Closed);
    }

    [Fact]
    public async Task SetClosed_ShouldCompleteClosedTask()
    {
        // Arrange
        var metadata = new WindowMetadata();
        var closedTask = metadata.ClosedTask;

        // Act
        metadata.SetClosed();

        // Assert
        closedTask.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task ClosedTask_ShouldWaitForClose()
    {
        // Arrange
        var metadata = new WindowMetadata();
        var closedTask = metadata.ClosedTask;

        // Act
        var task = Task.Run(async () =>
        {
            await Task.Delay(50);
            metadata.SetClosed();
        });

        // Assert
        await Task.WhenAny(closedTask, Task.Delay(1000));
        closedTask.IsCompleted.Should().BeTrue();
    }

    [STAFact]
    public void WithWindow_WhenWindowAlive_ShouldExecuteAction()
    {
        // Arrange
        var window = new System.Windows.Window();
        var metadata = new WindowMetadata
        {
            WindowRef = new WeakReference<Window>(window)
        };
        var executed = false;

        // Act
        var result = metadata.WithWindow(w => executed = true);

        // Assert
        result.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public void WithWindow_WhenWindowDead_ShouldReturnFalse()
    {
        // Arrange
        var metadata = new WindowMetadata
        {
            WindowRef = new WeakReference<Window>(null!)
        };

        // Act
        var result = metadata.WithWindow(w => { });

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void WithViewModel_WhenViewModelAlive_ShouldExecuteAction()
    {
        // Arrange
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<TestViewModel>.Instance;
        var viewModel = new TestViewModel(logger);
        var metadata = new WindowMetadata
        {
            ViewModelRef = new WeakReference<IViewModel>(viewModel)
        };
        var executed = false;

        // Act
        var result = metadata.WithViewModel(vm => executed = true);

        // Assert
        result.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [STAFact]
    public void DisposeScopeSafely_WithHandle_ShouldDisposeHandle()
    {
        // Arrange
        var scope = new Autofac.ContainerBuilder().Build().BeginLifetimeScope();
        var window = new System.Windows.Window();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<TestViewModel>.Instance;
        var viewModel = new TestViewModel(logger);
        var handle = new WindowHandle(Guid.NewGuid(), scope, window, viewModel, null);
        var metadata = new WindowMetadata
        {
            Handle = handle
        };
        var scopeDisposed = false;

        // Act
        metadata.DisposeScopeSafely();

        // Check if scope is disposed by checking if handle is null
        // (scope disposal is handled by WindowHandle)
        scopeDisposed = metadata.Handle == null;

        // Assert
        metadata.Handle.Should().BeNull();
        metadata.Lifecycle.Should().Be(WindowLifecycleState.Closed);
        scopeDisposed.Should().BeTrue();
    }

    [Fact]
    public void DisposeScopeSafely_WithoutHandle_ShouldNotThrow()
    {
        // Arrange
        var metadata = new WindowMetadata();

        // Act
        var action = () => metadata.DisposeScopeSafely();

        // Assert
        action.Should().NotThrow();
    }

    [STAFact]
    public void Dispose_ShouldClearReferences()
    {
        // Arrange
        var window = new TestWindow(new Mock<Microsoft.Extensions.Logging.ILogger>().Object);
        var viewModel = new TestViewModel(new MockLogger<TestViewModel>());
        var metadata = new WindowMetadata
        {
            WindowRef = new WeakReference<Window>(window),
            ViewModelRef = new WeakReference<IViewModel>(viewModel)
        };

        // Act
        metadata.Dispose();

        // Assert
        metadata.WindowRef.Should().BeNull();
        metadata.ViewModelRef.Should().BeNull();
    }
}

