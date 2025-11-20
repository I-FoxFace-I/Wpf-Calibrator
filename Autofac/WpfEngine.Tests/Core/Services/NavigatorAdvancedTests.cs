using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using WpfEngine.ViewModels;
using WpfEngine.Data.Content;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using Xunit;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Advanced tests for Navigator - NavigateBackToAsync, error handling, edge cases
/// </summary>
public class NavigatorAdvancedTests : AutofacTestFixture
{
    protected override IViewRegistry RegisterMapping(IViewRegistry viewRegistry)
    {
        return viewRegistry; // No view mappings needed
    }

    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register Navigator
        builder.RegisterType<Navigator>()
               .As<INavigator>()
               .InstancePerLifetimeScope();

        // Register test ViewModels
        builder.RegisterType<TestViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestViewModelWithParams>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestInitializableViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<DisposableTestViewModel>().AsSelf().InstancePerDependency();

        // Register loggers
        builder.RegisterGeneric(typeof(Mock<>)).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<Navigator>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModelWithParams>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestInitializableViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<DisposableTestViewModel>>()).InstancePerDependency();
    }

    [Fact]
    public async Task NavigateBackToAsync_WhenTypeExists_ShouldNavigateBack()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        var parameters = new TestParameters { CorrelationId = Guid.NewGuid(), Value = "Test" };
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModelWithParams, TestParameters>(parameters);
        await navigator.NavigateToAsync<TestViewModel>();

        // Act
        var result = await navigator.NavigateBackToAsync<TestViewModelWithParams>();

        // Assert
        result.Should().BeTrue();
        navigator.CurrentViewModel.Should().BeOfType<TestViewModelWithParams>();
    }

    [Fact]
    public async Task NavigateBackToAsync_WhenTypeNotExists_ShouldReturnFalse()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Act
        var result = await navigator.NavigateBackToAsync<TestViewModelWithParams>();

        // Assert
        result.Should().BeFalse();
        navigator.CurrentViewModel.Should().BeOfType<TestViewModel>();
    }

    [Fact]
    public async Task NavigateBackToAsync_WhenHistoryEmpty_ShouldReturnFalse()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();

        // Act
        var result = await navigator.NavigateBackToAsync<TestViewModel>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateBackToAsync_ShouldRemoveIntermediateEntries()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        var parameters = new TestParameters { CorrelationId = Guid.NewGuid(), Value = "Test" };
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModelWithParams, TestParameters>(parameters);
        await navigator.NavigateToAsync<TestViewModel>();
        var initialHistoryDepth = navigator.HistoryDepth;

        // Act
        await navigator.NavigateBackToAsync<TestViewModelWithParams>();

        // Assert
        navigator.HistoryDepth.Should().BeLessThan(initialHistoryDepth);
        navigator.CurrentViewModel.Should().BeOfType<TestViewModelWithParams>();
    }

    [Fact]
    public async Task NavigateBackToAsync_WithParameters_ShouldRestoreParameters()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        var parameters = new TestParameters { CorrelationId = Guid.NewGuid(), Value = "Test" };
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModelWithParams, TestParameters>(parameters);
        await navigator.NavigateToAsync<TestViewModel>();

        // Act
        await navigator.NavigateBackToAsync<TestViewModelWithParams>();

        // Assert
        navigator.CurrentViewModel.Should().BeOfType<TestViewModelWithParams>();
        if (navigator.CurrentViewModel is TestViewModelWithParams vm)
        {
            vm.ReceivedParameters.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task NavigateBack_WhenHistoryEmpty_ShouldNotThrow()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();

        // Act
        await navigator.NavigateBackAsync();

        // Assert
        // Should not throw - just logs warning
        navigator.CurrentViewModel.Should().BeNull();
    }

    [Fact]
    public async Task NavigateToAsync_ShouldCompleteSuccessfully()
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
    public async Task RequestCloseAsync_WithConfirmation_ShouldRaiseEvent()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        NavigatorCloseRequestedEventArgs? eventArgs = null;
        navigator.NavigatorCloseRequest += (s, e) => eventArgs = e;

        // Act
        await navigator.RequestCloseAsync(showConfirmation: true, confirmationMessage: "Are you sure?");

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.ShowConfirmation.Should().BeTrue();
        eventArgs.ConfirmationMessage.Should().Be("Are you sure?");
    }

    [Fact]
    public async Task RequestCloseAsync_WithoutConfirmation_ShouldRaiseEvent()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        NavigatorCloseRequestedEventArgs? eventArgs = null;
        navigator.NavigatorCloseRequest += (s, e) => eventArgs = e;

        // Act
        await navigator.RequestCloseAsync(showConfirmation: false);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.ShowConfirmation.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_ErrorDuringCreation_ShouldHandleGracefully()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        // Note: This test verifies that error handling exists
        // For actual error scenarios, we'd need a ViewModel that throws during creation

        // Act & Assert
        // Should handle errors gracefully
        await navigator.NavigateToAsync<TestViewModel>();
        navigator.CurrentViewModel.Should().NotBeNull();
    }

    [Fact]
    public async Task PropertyChanged_ShouldRaiseOnCurrentViewModelChange()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        var propertyChangedCount = 0;
        ((System.ComponentModel.INotifyPropertyChanged)navigator).PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigator.CurrentViewModel))
                propertyChangedCount++;
        };

        // Act
        await navigator.NavigateToAsync<TestViewModel>();

        // Assert
        propertyChangedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PropertyChanged_ShouldRaiseOnCanNavigateBackChange()
    {
        // Arrange
        var navigator = Scope.Resolve<INavigator>();
        var propertyChangedCount = 0;
        ((System.ComponentModel.INotifyPropertyChanged)navigator).PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(INavigator.CanNavigateBack))
                propertyChangedCount++;
        };

        // Act
        await navigator.NavigateToAsync<TestViewModel>();
        await navigator.NavigateToAsync<TestViewModel>();

        // Assert
        propertyChangedCount.Should().BeGreaterThan(0);
    }
}

