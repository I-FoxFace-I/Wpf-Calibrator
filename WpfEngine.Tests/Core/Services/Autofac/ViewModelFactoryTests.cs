using Autofac;
using Autofac.Core;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Services.Autofac;
using Xunit;

namespace WpfEngine.Tests.Core.Services.Autofac;

/// <summary>
/// Tests for ViewModelFactory - Autofac TypedParameter injection
/// </summary>
public class ViewModelFactoryTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    private readonly ViewModelFactory _factory;

    public ViewModelFactoryTests()
    {
        var builder = new ContainerBuilder();

        // Register loggers
        builder.Register(c => new Mock<ILogger<ViewModelFactory>>().Object).As<ILogger<ViewModelFactory>>();
        builder.Register(c => new Mock<ILogger<TestViewModel>>().Object).As<ILogger<TestViewModel>>();
        builder.Register(c => new Mock<ILogger<TestViewModelWithParams>>().Object).As<ILogger<TestViewModelWithParams>>();
        builder.Register(c => new Mock<ILogger<TestViewModelWithDependency>>().Object).As<ILogger<TestViewModelWithDependency>>();

        // Register test ViewModels
        builder.RegisterType<TestViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestViewModelWithParams>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestViewModelWithDependency>().AsSelf().InstancePerDependency();

        // Register test service
        builder.RegisterType<TestService>().As<ITestService>().SingleInstance();

        _container = builder.Build();
        _scope = _container.BeginLifetimeScope();

        _factory = new ViewModelFactory(_scope, Mock.Of<ILogger<ViewModelFactory>>());
    }

    [Fact]
    public void Create_WithoutParameters_ResolvesViewModel()
    {
        // Act
        var vm = _factory.Create<TestViewModel>();

        // Assert
        vm.Should().NotBeNull();
        vm.Should().BeOfType<TestViewModel>();
        vm.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_InjectsDependencies()
    {
        // Act
        var vm = _factory.Create<TestViewModelWithDependency>();

        // Assert
        vm.Should().NotBeNull();
        vm.InjectedService.Should().NotBeNull();
        vm.InjectedService.Should().BeAssignableTo<ITestService>();
    }

    [Fact]
    public void Create_WithOptions_PassesOptionsViaTypedParameter()
    {
        // Arrange
        var options = new TestVmParams { Value = "test123" };

        // Act
        var vm = _factory.Create<TestViewModelWithParams, TestVmParams>(options);

        // Assert
        vm.Should().NotBeNull();
        vm.ReceivedParams.Should().BeSameAs(options);
        vm.ReceivedParams.Value.Should().Be("test123");
    }

    [Fact]
    public void Create_MultipleTimes_CreatesNewInstances()
    {
        // Act
        var vm1 = _factory.Create<TestViewModel>();
        var vm2 = _factory.Create<TestViewModel>();

        // Assert
        vm1.Should().NotBeSameAs(vm2);
        vm1.Id.Should().NotBe(vm2.Id);
    }

    [Fact]
    public void Create_WithOptions_MultipleTimes_CreatesIndependentInstances()
    {
        // Arrange
        var options1 = new TestVmParams { Value = "first" };
        var options2 = new TestVmParams { Value = "second" };

        // Act
        var vm1 = _factory.Create<TestViewModelWithParams, TestVmParams>(options1);
        var vm2 = _factory.Create<TestViewModelWithParams, TestVmParams>(options2);

        // Assert
        vm1.Should().NotBeSameAs(vm2);
        vm1.ReceivedParams.Value.Should().Be("first");
        vm2.ReceivedParams.Value.Should().Be("second");
    }

    [Fact]
    public void Create_SharesSingletonDependencies()
    {
        // Act
        var vm1 = _factory.Create<TestViewModelWithDependency>();
        var vm2 = _factory.Create<TestViewModelWithDependency>();

        // Assert
        vm1.InjectedService.Should().BeSameAs(vm2.InjectedService);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _container?.Dispose();
    }

    // ========== TEST TYPES ==========

    public class TestViewModel : IViewModel
    {
        private readonly ILogger<TestViewModel> _logger;

        public TestViewModel(ILogger<TestViewModel> logger)
        {
            _logger = logger;
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

    public class TestViewModelWithParams : IViewModel
    {
        private readonly ILogger<TestViewModelWithParams> _logger;

        public TestViewModelWithParams(
            ILogger<TestViewModelWithParams> logger,
            TestVmParams parameters)
        {
            _logger = logger;
            ReceivedParams = parameters;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public TestVmParams ReceivedParams { get; }

        public Task InitializeAsync() => Task.CompletedTask;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }

    public interface ITestService
    {
        string GetValue();
    }

    public class TestService : ITestService
    {
        public string GetValue() => "TestValue";
    }

    public class TestViewModelWithDependency : IViewModel
    {
        private readonly ILogger<TestViewModelWithDependency> _logger;

        public TestViewModelWithDependency(
            ILogger<TestViewModelWithDependency> logger,
            ITestService testService)
        {
            _logger = logger;
            InjectedService = testService;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public string? DisplayName { get; set; }
        public bool IsBusy { get; set; }
        public string? BusyMessage { get; set; }
        public ITestService InjectedService { get; }

        public Task InitializeAsync() => Task.CompletedTask;
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}

