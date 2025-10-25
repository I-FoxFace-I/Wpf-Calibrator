using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Services.WindowTracking;
using Xunit;

namespace WpfEngine.Tests.Core.ViewModels;

/// <summary>
/// Tests for ShellViewModel base class
/// </summary>
public class ShellViewModelTests : IDisposable
{
    private readonly Mock<IContentManager> _contentManagerMock;
    private readonly Mock<IWindowService> _windowServiceMock;
    private readonly Mock<ILogger<TestShellViewModel>> _loggerMock;
    private readonly TestShellViewModel _shellViewModel;

    public ShellViewModelTests()
    {
        _contentManagerMock = new Mock<IContentManager>();
        _windowServiceMock = new Mock<IWindowService>();
        _loggerMock = new Mock<ILogger<TestShellViewModel>>();

        _shellViewModel = new TestShellViewModel(
            _contentManagerMock.Object,
            _windowServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void CurrentContent_ReturnsContentManagerCurrent()
    {
        // Arrange
        var expectedContent = new object();
        _contentManagerMock.Setup(m => m.CurrentContent).Returns(expectedContent);

        // Act
        var content = _shellViewModel.CurrentContent;

        // Assert
        content.Should().BeSameAs(expectedContent);
    }

    [Fact]
    public void PropertyChanged_RaisedWhenContentChanges()
    {
        // Arrange
        var propertyChangedCount = 0;
        _shellViewModel.PropertyChangedTest += (s, e) =>
        {
            if (e.PropertyName == nameof(ShellViewModel.CurrentContent))
                propertyChangedCount++;
        };

        // Act
        _contentManagerMock.Raise(m => m.PropertyChanged += null,
            new System.ComponentModel.PropertyChangedEventArgs(nameof(IContentManager.CurrentContent)));

        // Assert
        propertyChangedCount.Should().Be(1);
    }

    [Fact]
    public void OnShellCloseRequested_WithoutConfirmation_ClosesShell()
    {
        // Arrange
        var closeArgs = new ShellCloseRequestedEventArgs(showConfirmation: false);

        // Act
        _contentManagerMock.Raise(m => m.ShellCloseRequested += null, closeArgs);

        // Assert
        _windowServiceMock.Verify(m => m.Close(It.IsAny<VmKey>()), Times.Once);
    }

    [Fact]
    public void Dispose_UnsubscribesFromEvents()
    {
        // Act
        _shellViewModel.Dispose();

        // Assert - Should not throw if events are raised after dispose
        var act = () => _contentManagerMock.Raise(m => m.ShellCloseRequested += null,
            new ShellCloseRequestedEventArgs());

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ClearsContentHistory()
    {
        // Act
        _shellViewModel.Dispose();

        // Assert
        _contentManagerMock.Verify(m => m.ClearHistory(), Times.Once);
    }

    public void Dispose()
    {
        _shellViewModel?.Dispose();
    }

    // ========== TEST TYPES ==========

    public class TestShellViewModel : ShellViewModel
    {
        public TestShellViewModel(
            IContentManager contentManager,
            IWindowService windowService,
            ILogger<TestShellViewModel> logger)
            : base(contentManager, windowService, logger)
        {
        }

        public override Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        // Expose PropertyChanged for testing
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChangedTest
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }
    }
}

