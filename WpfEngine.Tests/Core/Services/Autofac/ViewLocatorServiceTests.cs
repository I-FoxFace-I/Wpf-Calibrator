using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Windows;
using System.Windows.Controls;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Services;
using WpfEngine.Tests.Helpers;
using Xunit;

namespace WpfEngine.Tests.Core.Services.Autofac;

/// <summary>
/// Tests for ViewLocatorService - configuration-based view resolution
/// </summary>
public class ViewLocatorServiceTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    private readonly ViewRegistry _registry;
    private readonly ViewLocatorService _viewLocator;

    public ViewLocatorServiceTests()
    {
        var builder = new ContainerBuilder();

        // Register loggers
        builder.Register(c => new Mock<ILogger<ViewRegistry>>().Object).As<ILogger<ViewRegistry>>();
        builder.Register(c => new Mock<ILogger<ViewLocatorService>>().Object).As<ILogger<ViewLocatorService>>();
        builder.Register(c => new Mock<ILogger<TestWindow>>().Object).As<ILogger<TestWindow>>();
        builder.Register(c => new Mock<ILogger<TestControl>>().Object).As<ILogger<TestControl>>();

        // Register ViewRegistry
        builder.RegisterType<ViewRegistry>()
               .AsSelf()
               .As<IViewRegistry>()
               .SingleInstance();

        // Register test views
        builder.RegisterType<TestWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestControl>().AsSelf().InstancePerDependency();

        _container = builder.Build();
        _scope = _container.BeginLifetimeScope();

        _registry = _container.Resolve<ViewRegistry>();
        _viewLocator = new ViewLocatorService(_scope, _registry, Mock.Of<ILogger<ViewLocatorService>>());
    }

    [STAFact]
    public void ResolveView_WithRegisteredMapping_ReturnsView()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Act
        var view = _viewLocator.ResolveView<TestViewModel>();

        // Assert
        view.Should().NotBeNull();
        view.Should().BeAssignableTo<IView>();
        view.Should().BeOfType<TestWindow>();
    }

    [STAFact]
    public void ResolveView_NonGeneric_ReturnsView()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Act
        var view = _viewLocator.ResolveView(typeof(TestViewModel));

        // Assert
        view.Should().NotBeNull();
        view.Should().BeOfType<TestWindow>();
    }

    [Fact]
    public void ResolveView_WithoutMapping_ThrowsException()
    {
        // Act
        var act = () => _viewLocator.ResolveView<TestViewModel>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("No view mapping found for ViewModel: TestViewModel*");
    }

    [STAFact]
    public void ResolveView_WithControlMapping_ReturnsControl()
    {
        // Arrange
        _registry.MapControl<TestViewModel, TestControl>();

        // Act
        var view = _viewLocator.ResolveView<TestViewModel>();

        // Assert
        view.Should().BeOfType<TestControl>();
    }

    [Fact]
    public void HasMapping_WithRegisteredMapping_ReturnsTrue()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Act
        var hasMapping = _viewLocator.HasMapping<TestViewModel>();

        // Assert
        hasMapping.Should().BeTrue();
    }

    [Fact]
    public void HasMapping_WithoutMapping_ReturnsFalse()
    {
        // Act
        var hasMapping = _viewLocator.HasMapping<TestViewModel>();

        // Assert
        hasMapping.Should().BeFalse();
    }

    [Fact]
    public void HasMapping_NonGeneric_ReturnsCorrectValue()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Act
        var hasMapped = _viewLocator.HasMapping(typeof(TestViewModel));
        var hasUnmapped = _viewLocator.HasMapping(typeof(TestViewModel2));

        // Assert
        hasMapped.Should().BeTrue();
        hasUnmapped.Should().BeFalse();
    }

    [STAFact]
    public void ResolveView_CreatesNewInstanceEachTime()
    {
        // Arrange
        _registry.MapWindow<TestViewModel, TestWindow>();

        // Act
        var view1 = _viewLocator.ResolveView<TestViewModel>();
        var view2 = _viewLocator.ResolveView<TestViewModel>();

        // Assert
        view1.Should().NotBeSameAs(view2);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _container?.Dispose();
    }

    // ========== TEST TYPES ==========

    public class TestViewModel : IViewModel
    {
        public Guid Id => Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public Task InitializeAsync() => Task.CompletedTask;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestViewModel2 : IViewModel
    {
        public Guid Id => Guid.NewGuid();
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public Task InitializeAsync() => Task.CompletedTask;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public class TestWindow : Window, IWindowView
    {
        private readonly Guid _windowId = Guid.NewGuid();

        public TestWindow(ILogger<TestWindow> logger)
        {
        }

        public Guid WindowId => _windowId;
    }

    public class TestControl : UserControl, IControlView
    {
        public TestControl(ILogger<TestControl> logger)
        {
        }
    }
}

