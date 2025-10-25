using Autofac;
using Autofac.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Services.Autofac;
using Xunit;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for NavigationService - ViewModel navigation using Autofac
/// </summary>
public class NavigationServiceTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    private readonly NavigationService _navigationService;

    public NavigationServiceTests()
    {
        var builder = new ContainerBuilder();

        // Register loggers
        builder.Register(c => new Mock<ILogger<NavigationService>>().Object).As<ILogger<NavigationService>>();
        builder.Register(c => new Mock<ILogger<TestViewModel1>>().Object).As<ILogger<TestViewModel1>>();
        builder.Register(c => new Mock<ILogger<TestViewModel2>>().Object).As<ILogger<TestViewModel2>>();
        builder.Register(c => new Mock<ILogger<TestViewModelWithParams>>().Object).As<ILogger<TestViewModelWithParams>>();

        // Register test ViewModels
        builder.RegisterType<TestViewModel1>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestViewModel2>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestViewModelWithParams>().AsSelf().InstancePerDependency();

        _container = builder.Build();
        _scope = _container.BeginLifetimeScope("test-window");

        // Create ViewModelFactory and NavigationService
        var viewModelFactory = new ViewModelFactory(_scope, Mock.Of<ILogger<ViewModelFactory>>());
        
        _navigationService = new NavigationService(
            viewModelFactory,
            Mock.Of<ILogger<NavigationService>>());
    }

    [Fact]
    public async Task NavigateToAsync_SetsCurrentViewModel()
    {
        // Act
        await _navigationService.NavigateToAsync<TestViewModel1>();

        // Assert
        _navigationService.CurrentViewModel.Should().NotBeNull();
        _navigationService.CurrentViewModel.Should().BeOfType<TestViewModel1>();
    }

    [Fact]
    public async Task NavigateToAsync_InitializesViewModel()
    {
        // Act
        await _navigationService.NavigateToAsync<TestViewModel1>();

        // Assert
        var vm = _navigationService.CurrentViewModel as TestViewModel1;
        vm!.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateToAsync_PushesCurrentToHistory()
    {
        // Arrange
        await _navigationService.NavigateToAsync<TestViewModel1>();
        _navigationService.CanNavigateBack.Should().BeFalse();

        // Act
        await _navigationService.NavigateToAsync<TestViewModel2>();

        // Assert
        _navigationService.CanNavigateBack.Should().BeTrue();
        _navigationService.HistoryDepth.Should().Be(1);
    }

    [Fact]
    public async Task NavigateToAsync_DisposesPreviousViewModel()
    {
        // Arrange
        await _navigationService.NavigateToAsync<TestViewModel1>();
        var firstVm = _navigationService.CurrentViewModel as TestViewModel1;

        // Act
        await _navigationService.NavigateToAsync<TestViewModel2>();

        // Assert
        firstVm!.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateToAsync_WithOptions_PassesOptionsViaTypedParameter()
    {
        // Arrange
        var options = new TestVmParams { Value = "test123" };

        // Act
        await _navigationService.NavigateToAsync<TestViewModelWithParams, TestVmParams>(options);

        // Assert
        var vm = _navigationService.CurrentViewModel as TestViewModelWithParams;
        vm!.ReceivedParams.Should().BeSameAs(options);
        vm.ReceivedParams.Value.Should().Be("test123");
    }

    [Fact]
    public async Task NavigateToAsync_WithOptions_InitializesWithOptions()
    {
        // Arrange
        var options = new TestVmParams { Value = "init-test" };

        // Act
        await _navigationService.NavigateToAsync<TestViewModelWithParams, TestVmParams>(options);

        // Assert
        var vm = _navigationService.CurrentViewModel as TestViewModelWithParams;
        vm!.IsInitializedWithParams.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateBackAsync_RestoresPreviousViewModel()
    {
        // Arrange
        await _navigationService.NavigateToAsync<TestViewModel1>();
        var firstVm = _navigationService.CurrentViewModel;
        await _navigationService.NavigateToAsync<TestViewModel2>();

        // Act
        await _navigationService.NavigateBackAsync();

        // Assert
        _navigationService.CurrentViewModel.Should().BeSameAs(firstVm);
        _navigationService.HistoryDepth.Should().Be(0);
    }

    [Fact]
    public async Task NavigateBackAsync_WhenHistoryEmpty_DoesNothing()
    {
        // Arrange
        await _navigationService.NavigateToAsync<TestViewModel1>();
        var currentVm = _navigationService.CurrentViewModel;

        // Act
        await _navigationService.NavigateBackAsync();

        // Assert
        _navigationService.CurrentViewModel.Should().BeSameAs(currentVm);
        _navigationService.CanNavigateBack.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateBackAsync_ReInitializesPreviousViewModel()
    {
        // Arrange
        await _navigationService.NavigateToAsync<TestViewModel1>();
        var firstVm = _navigationService.CurrentViewModel as TestViewModel1;
        firstVm!.IsInitialized = false; // Reset flag
        
        await _navigationService.NavigateToAsync<TestViewModel2>();

        // Act
        await _navigationService.NavigateBackAsync();

        // Assert
        firstVm.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task ClearHistory_DisposesAllHistoryItems()
    {
        // Arrange
        var disposedVms = new List<TestViewModel1>();

        await _navigationService.NavigateToAsync<TestViewModel1>();
        disposedVms.Add(_navigationService.CurrentViewModel as TestViewModel1);

        await _navigationService.NavigateToAsync<TestViewModel1>();
        disposedVms.Add(_navigationService.CurrentViewModel as TestViewModel1);

        await _navigationService.NavigateToAsync<TestViewModel1>();

        // Act
        _navigationService.ClearHistory();

        // Assert
        _navigationService.HistoryDepth.Should().Be(0);
        _navigationService.CanNavigateBack.Should().BeFalse();
        disposedVms.ForEach(vm => vm!.IsDisposed.Should().BeTrue());
    }

    [Fact]
    public void RequestWindowClose_RaisesEvent()
    {
        // Arrange
        WindowCloseRequestedEventArgs? capturedArgs = null;
        _navigationService.WindowCloseRequested += (s, e) => capturedArgs = e;

        // Act
        _navigationService.RequestWindowClose(showConfirmation: true, confirmationMessage: "Test");

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.ShowConfirmation.Should().BeTrue();
        capturedArgs.ConfirmationMessage.Should().Be("Test");
    }

    [Fact]
    public async Task PropertyChanged_RaisedWhenCurrentViewModelChanges()
    {
        // Arrange
        var propertyChangedCount = 0;
        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
                propertyChangedCount++;
        };

        // Act
        await _navigationService.NavigateToAsync<TestViewModel1>();
        await _navigationService.NavigateToAsync<TestViewModel2>();

        // Assert
        propertyChangedCount.Should().Be(2);
    }

    [Fact]
    public async Task PropertyChanged_RaisedForCanNavigateBack()
    {
        // Arrange
        var canNavigateBackChangedCount = 0;
        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigationService.CanNavigateBack))
                canNavigateBackChangedCount++;
        };

        // Act
        await _navigationService.NavigateToAsync<TestViewModel1>();
        await _navigationService.NavigateToAsync<TestViewModel2>();
        await _navigationService.NavigateBackAsync();

        // Assert
        canNavigateBackChangedCount.Should().BeGreaterThan(0);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _container?.Dispose();
    }

    // ========== TEST TYPES ==========

    public class TestViewModel1 : IViewModel, IInitializable, IDisposable
    {
        public TestViewModel1(ILogger<TestViewModel1> logger)
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public bool IsInitialized { get; set; }
        public bool IsDisposed { get; private set; }

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public void Dispose() => IsDisposed = true;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestViewModel2 : IViewModel, IInitializable
    {
        public TestViewModel2(ILogger<TestViewModel2> logger)
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }

        public Task InitializeAsync() => Task.CompletedTask;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestVmParams : IVmParameters
    {
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public string Value { get; init; } = string.Empty;
    }

    public class TestViewModelWithParams : IViewModel, IInitializable<TestVmParams>
    {
        public TestViewModelWithParams(
            ILogger<TestViewModelWithParams> logger,
            TestVmParams parameters)
        {
            ReceivedParams = parameters;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public TestVmParams ReceivedParams { get; }
        public bool IsInitializedWithParams { get; private set; }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task InitializeAsync(TestVmParams parameter)
        {
            IsInitializedWithParams = true;
            return Task.CompletedTask;
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}

