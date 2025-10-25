using Autofac;
using Autofac.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Core.Services;
using WpfEngine.Core.Services.Autofac;
using WpfEngine.Core.ViewModels;
using Xunit;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for ContentManager - shell content navigation
/// </summary>
public class ContentManagerTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    private readonly ContentManager _contentManager;
    private readonly Mock<ILogger<ContentManager>> _loggerMock;

    public ContentManagerTests()
    {
        var builder = new ContainerBuilder();

        // Register test ViewModels
        builder.RegisterType<TestViewModel1>().AsSelf();
        builder.RegisterType<TestViewModel2>().AsSelf();
        builder.RegisterType<TestViewModelWithParams>().AsSelf();

        _container = builder.Build();
        _scope = _container.BeginLifetimeScope("test-window");

        _loggerMock = new Mock<ILogger<ContentManager>>();
        _contentManager = new ContentManager(_scope, _loggerMock.Object);
    }

    [Fact]
    public async Task NavigateToAsync_SetsCurrentContent()
    {
        // Act
        await _contentManager.NavigateToAsync<TestViewModel1>();

        // Assert
        _contentManager.CurrentContent.Should().NotBeNull();
        _contentManager.CurrentContent.Should().BeOfType<TestViewModel1>();
    }

    [Fact]
    public async Task NavigateToAsync_InitializesViewModel()
    {
        // Act
        await _contentManager.NavigateToAsync<TestViewModel1>();

        // Assert
        var vm = _contentManager.CurrentContent as TestViewModel1;
        vm!.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateToAsync_PushesCurrentToHistory()
    {
        // Arrange
        await _contentManager.NavigateToAsync<TestViewModel1>();
        _contentManager.CanNavigateBack.Should().BeFalse();

        // Act
        await _contentManager.NavigateToAsync<TestViewModel2>();

        // Assert
        _contentManager.CanNavigateBack.Should().BeTrue();
        _contentManager.HistoryDepth.Should().Be(1);
    }

    [Fact]
    public async Task NavigateToAsync_DisposesPreviousContent()
    {
        // Arrange
        await _contentManager.NavigateToAsync<TestViewModel1>();
        var firstVm = _contentManager.CurrentContent as TestViewModel1;

        // Act
        await _contentManager.NavigateToAsync<TestViewModel2>();

        // Assert
        firstVm!.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateToAsync_WithOptions_PassesOptionsToViewModel()
    {
        // Arrange
        var options = new TestViewModelParams { TestValue = "test123" };

        // Act
        await _contentManager.NavigateToAsync<TestViewModelWithParams, TestViewModelParams>(options);

        // Assert
        var vm = _contentManager.CurrentContent as TestViewModelWithParams;
        vm!.ReceivedOptions.Should().BeSameAs(options);
        vm.ReceivedOptions.TestValue.Should().Be("test123");
    }

    [Fact]
    public async Task NavigateBackAsync_RestoresPreviousContent()
    {
        // Arrange
        await _contentManager.NavigateToAsync<TestViewModel1>();
        var firstVm = _contentManager.CurrentContent;
        await _contentManager.NavigateToAsync<TestViewModel2>();

        // Act
        await _contentManager.NavigateBackAsync();

        // Assert
        _contentManager.CurrentContent.Should().BeSameAs(firstVm);
    }

    [Fact]
    public async Task NavigateBackAsync_WhenHistoryEmpty_DoesNothing()
    {
        // Arrange
        await _contentManager.NavigateToAsync<TestViewModel1>();
        var currentVm = _contentManager.CurrentContent;

        // Act
        await _contentManager.NavigateBackAsync();

        // Assert
        _contentManager.CurrentContent.Should().BeSameAs(currentVm);
    }

    [Fact]
    public async Task ClearHistory_DisposesAllHistoryItems()
    {
        // Arrange
        var disposedVms = new List<TestViewModel1>();

        await _contentManager.NavigateToAsync<TestViewModel1>();
        disposedVms.Add(_contentManager.CurrentContent as TestViewModel1);

        await _contentManager.NavigateToAsync<TestViewModel1>();
        disposedVms.Add(_contentManager.CurrentContent as TestViewModel1);

        await _contentManager.NavigateToAsync<TestViewModel1>();

        // Act
        _contentManager.ClearHistory();

        // Assert
        _contentManager.HistoryDepth.Should().Be(0);
        _contentManager.CanNavigateBack.Should().BeFalse();
        disposedVms.ForEach(vm => vm!.IsDisposed.Should().BeTrue());
    }

    [Fact]
    public void RequestShellClose_RaisesEvent()
    {
        // Arrange
        ShellCloseRequestedEventArgs? capturedArgs = null;
        _contentManager.ShellCloseRequested += (s, e) => capturedArgs = e;

        // Act
        _contentManager.RequestShellClose(showConfirmation: true, confirmationMessage: "Test message");

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.ShowConfirmation.Should().BeTrue();
        capturedArgs.ConfirmationMessage.Should().Be("Test message");
    }

    [Fact]
    public async Task PropertyChanged_IsRaisedWhenCurrentContentChanges()
    {
        // Arrange
        var propertyChangedCount = 0;
        _contentManager.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IContentManager.CurrentContent))
                propertyChangedCount++;
        };

        // Act
        await _contentManager.NavigateToAsync<TestViewModel1>();
        await _contentManager.NavigateToAsync<TestViewModel2>();

        // Assert
        propertyChangedCount.Should().Be(2);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _container?.Dispose();
    }

    // ========== TEST VIEW MODELS ==========

    public class TestViewModel1 : IViewModel, IInitializable, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public bool IsInitialized { get; private set; }
        public bool IsDisposed { get; private set; }

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestViewModel2 : IViewModel, IInitializable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }

        public Task InitializeAsync() => Task.CompletedTask;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestViewModelParams : IVmParameters
    {
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public string TestValue { get; init; } = string.Empty;
    }

    public class TestViewModelWithParams : IViewModel, IInitializable<TestViewModelParams>
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public TestViewModelParams? ReceivedOptions { get; private set; }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task InitializeAsync(TestViewModelParams parameter)
        {
            ReceivedOptions = parameter;
            return Task.CompletedTask;
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}

