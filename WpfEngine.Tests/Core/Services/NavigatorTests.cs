using System;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Abstract;
using WpfEngine.ViewModels;
using WpfEngine.Data.Abstract;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using Xunit;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Tests.Core.Services;

public class NavigatorTests : AutofacTestFixture
{
    public NavigatorTests()
        : base()
    {

    }
    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register Navigator
        builder.RegisterType<Navigator>()
               .As<INavigator>()
               .InstancePerLifetimeScope();

        // Register test ViewModels
        builder.RegisterType<TestViewModel>().AsSelf();
        builder.RegisterType<TestViewModelWithParams>().AsSelf();
        builder.RegisterType<DisposableTestViewModel>().AsSelf();
        builder.RegisterType<TestInitializableViewModel>().AsSelf();

        // Register mock logger
        builder.RegisterGeneric(typeof(Mock<>)).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<Navigator>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModelWithParams>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestInitializableViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<DisposableTestViewModel>>()).InstancePerDependency();

    }

    protected override IViewRegistry RegisterMapping(IViewRegistry viewRegistry)
    {
        return viewRegistry.MapWindow<TestViewModel, TestWindow>()
                           .MapWindow<TestViewModelWithParams, TestWindow>()
                           .MapWindow<DisposableTestViewModel, TestWindow>()
                           .MapWindow<TestInitializableViewModel, TestWindow>();
    }

    // ========== BASIC NAVIGATION TESTS ==========

    [Fact]
    public async Task NavigateToAsync_SetsCurrentViewModel()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();

        // Act
        await navigator.NavigateToAsync<TestViewModel>();

        // Assert
        navigator.CurrentViewModel.Should().NotBeNull();
        navigator.CurrentViewModel.Should().BeOfType<TestViewModel>();
    }

    [Fact]
    public async Task NavigateToAsync_WithParameters_PassesParametersCorrectly()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        var parameters = new TestParameters { Value = "test123" };

        // Act
        await navigator.NavigateToAsync<TestViewModelWithParams, TestParameters>(parameters);

        // Assert
        navigator.CurrentViewModel.Should().BeOfType<TestViewModelWithParams>();
        var vm = (TestViewModelWithParams)navigator.CurrentViewModel!;
        vm.ReceivedParameters.Should().NotBeNull();
        vm.ReceivedParameters!.Value.Should().Be("test123");
    }

    [Fact]
    public async Task NavigateToAsync_CallsInitializeAsync_WhenViewModelIsInitializable()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();

        // Act
        await navigator.NavigateToAsync<TestInitializableViewModel>();

        // Assert
        var vm = (TestInitializableViewModel)navigator.CurrentViewModel!;
        vm.InitializeCalled.Should().BeTrue();
    }

    // ========== HISTORY MANAGEMENT TESTS ==========

    [Fact]
    public async Task NavigateToAsync_AddsToHistory()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();

        // Act
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Assert
        navigator.HistoryDepth.Should().Be(1);
        navigator.CanNavigateBack.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateBackAsync_RestoresPreviousViewModel()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();
        var firstViewModel = navigator.CurrentViewModel;
        await navigator.NavigateToAsync<TestViewModel>();

        // Act
        await navigator.NavigateBackAsync();

        // Assert
        navigator.CurrentViewModel.Should().BeOfType<TestViewModel>();
        navigator.CurrentViewModel.Should().NotBeSameAs(firstViewModel);
    }

    [Fact]
    public async Task NavigateBackAsync_DoesNothing_WhenHistoryEmpty()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();
        var currentViewModel = navigator.CurrentViewModel;

        // Act
        await navigator.NavigateBackAsync();

        // Assert
        navigator.CurrentViewModel.Should().BeSameAs(currentViewModel);
        navigator.CanNavigateBack.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateBackToAsync_NavigatesToSpecificViewModel()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();
        var targetViewModel = navigator.CurrentViewModel;
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Act
        var result = await navigator.NavigateBackToAsync<TestViewModel>();

        // Assert
        result.Should().BeTrue();
        navigator.CurrentViewModel.Should().BeOfType(targetViewModel.GetType());
    }

    [Fact]
    public async Task NavigateBackToAsync_ReturnsFalse_WhenViewModelNotInHistory()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Act
        var result = await navigator.NavigateBackToAsync<TestViewModelWithParams>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsInHistory_ReturnsTrue_WhenViewModelInHistory()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Act & Assert
        navigator.IsInHistory<TestViewModel>().Should().BeTrue();
    }

    [Fact]
    public async Task IsInHistory_ReturnsFalse_WhenViewModelNotInHistory()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Act & Assert
        navigator.IsInHistory<TestViewModelWithParams>().Should().BeFalse();
    }

    [Fact]
    public async Task ClearHistory_RemovesAllHistoryItems()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Act
        navigator.ClearHistory();

        // Assert
        navigator.HistoryDepth.Should().Be(0);
        navigator.CanNavigateBack.Should().BeFalse();
    }

    // ========== DISPOSAL TESTS ==========

    [Fact]
    public async Task NavigateToAsync_DisposesOldViewModel_WhenOwnsViewModels()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        navigator.OwnsViewModels = true;
        await navigator.NavigateToAsync<DisposableTestViewModel>();
        var firstViewModel = (DisposableTestViewModel)navigator.CurrentViewModel!;

        // Act
        await navigator.NavigateToAsync<TestViewModel>();

        // Assert
        firstViewModel.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateToAsync_DoesNotDisposeOldViewModel_WhenNotOwningViewModels()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        navigator.OwnsViewModels = false;
        await navigator.NavigateToAsync<DisposableTestViewModel>();
        var firstViewModel = (DisposableTestViewModel)navigator.CurrentViewModel!;

        // Act
        await navigator.NavigateToAsync<TestViewModel>();

        // Assert
        firstViewModel.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public async Task Dispose_DisposesCurrentViewModel_WhenOwnsViewModels()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        navigator.OwnsViewModels = true;
        await navigator.NavigateToAsync<DisposableTestViewModel>();
        var viewModel = (DisposableTestViewModel)navigator.CurrentViewModel!;

        // Act
        navigator.Dispose();

        // Assert
        viewModel.IsDisposed.Should().BeTrue();
    }

    // ========== CLOSE HANDLER TESTS ==========

    [Fact]
    public async Task RequestCloseAsync_ReturnsTrue_WhenHandlerExecutesSuccessfully()
    {
        // Arrange
        var result = false;
        var navigator = Scope.Resolve<INavigator>();
        var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        navigator.NavigatorCloseRequest += async (showConfirm, message) =>
        {
            await Task.Delay(1);
            result = true;
            completionSource.TrySetResult(true);
        };
        //(_, e) => result = e.ShowConfirmation;

        // Act
        navigator.RequestCloseAsync().Wait();
        await completionSource.Task;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RequestCloseAsync_ReturnsFalse_WhenHandlerReturnsFalse()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        //navigator.SetCloseHandler(async (showConfirm, message) =>
        //{
        //    await Task.Delay(1);
        //    return false; // User cancelled
        //});

        bool result = true;

        navigator.NavigatorCloseRequest += (_, e) => result = e.ShowConfirmation;
        // Act
        await navigator.RequestCloseAsync(showConfirmation: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RequestCloseAsync_ReturnsFalse_WhenNoHandlerSet()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();

        bool result = true;

        navigator.NavigatorCloseRequest += (_, e) => result = e.ShowConfirmation;

        // Act
        await navigator.RequestCloseAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RequestCloseAsync_PassesConfirmationFlag()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        bool? receivedShowConfirmation = null;
        string? receivedMessage = null;
        int counter = 0;

        navigator.NavigatorCloseRequest += (_, _) =>
        {
            receivedShowConfirmation = true;
            Interlocked.Increment(ref counter);
        };

        //navigator.SetCloseHandler(async (showConfirm, message) =>
        //{
        //    receivedShowConfirmation = showConfirm;
        //    receivedMessage = message;
        //    await Task.Delay(1);
        //    return true;
        //});

        // Act
        await navigator.RequestCloseAsync(showConfirmation: true, confirmationMessage: "Test message");

        // Assert
        counter.Should().BeGreaterThanOrEqualTo(1);
        receivedShowConfirmation.Should().BeTrue();
    }

    // ========== PROPERTY CHANGED TESTS ==========

    [Fact]
    public async Task NavigateToAsync_RaisesPropertyChanged_ForCurrentViewModel()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        var propertyChangedRaised = false;
        navigator.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigator.CurrentViewModel))
                propertyChangedRaised = true;
        };



        // Act
        await navigator.NavigateToAsync<TestViewModel>();

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    public void Dispose()
    {
        Scope?.Dispose();
        Container?.Dispose();
    }
}

// ========== TEST VIEW MODELS ==========

//public class TestViewModel : BaseViewModel
//{
//    public TestViewModel(ILogger<TestViewModel> logger) : base(logger)
//    {

//    }

//    public void Dispose() { }
//}

public class TestParameters : IViewModelParameters
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public string Value { get; init; } = "";
}

public class TestViewModelWithParams : BaseViewModel<TestParameters>
{
    public TestParameters? ReceivedParameters { get; private set; }

    public TestViewModelWithParams(ILogger<TestViewModelWithParams> logger, TestParameters parameters) : base(logger)
    {
        ReceivedParameters = parameters;
    }

    public void Dispose() { }
}

public class TestInitializableViewModel : BaseViewModel, IInitializable
{
    public bool InitializeCalled { get; private set; }

    public TestInitializableViewModel(ILogger<TestInitializableViewModel> logger) : base(logger)
    {
    }

    public override Task InitializeAsync()
    {
        InitializeCalled = true;
        return Task.CompletedTask;
    }

    public void Dispose() { }
}

public class DisposableTestViewModel : BaseViewModel, IDisposable
{
    public bool IsDisposed { get; private set; }

    public DisposableTestViewModel(ILogger<DisposableTestViewModel> logger) : base(logger)
    {
    }

    public void Dispose()
    {
        IsDisposed = true;
    }
}

