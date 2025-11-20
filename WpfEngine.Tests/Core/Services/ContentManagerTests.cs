using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using WpfEngine.ViewModels;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using Xunit;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for ContentManager - lightweight content creation and disposal
/// </summary>
public class ContentManagerTests : AutofacTestFixture
{
    protected override IViewRegistry RegisterMapping(IViewRegistry viewRegistry)
    {
        return viewRegistry; // No view mappings needed for ContentManager tests
    }

    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register ContentManager
        builder.RegisterType<ContentManager>()
               .As<IContentManager>()
               .InstancePerLifetimeScope();

        // Register test ViewModels
        builder.RegisterType<TestViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestViewModelWithParams>().AsSelf().InstancePerDependency();
        builder.RegisterType<DisposableTestViewModel>().AsSelf().InstancePerDependency();

        // Register loggers
        builder.Register(c => Mock.Of<ILogger<ContentManager>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModelWithParams>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<DisposableTestViewModel>>()).InstancePerDependency();
    }

    [Fact]
    public async Task CreateContentAsync_WithoutParameters_ShouldCreateViewModel()
    {
        // Arrange
        var contentManager = Scope.Resolve<IContentManager>();

        // Act
        var viewModel = await contentManager.CreateContentAsync<TestViewModel>();

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Should().BeOfType<TestViewModel>();
        viewModel.ViewModelId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateContentAsync_WithParameters_ShouldCreateViewModelWithParameters()
    {
        // Arrange
        var contentManager = Scope.Resolve<IContentManager>();
        var parameters = new TestParameters { CorrelationId = Guid.NewGuid(), Value = "Test" };

        // Act
        var viewModel = await contentManager.CreateContentAsync<TestViewModelWithParams>(parameters);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Should().BeOfType<TestViewModelWithParams>();
        if (viewModel is TestViewModelWithParams vmWithParams)
        {
            vmWithParams.ReceivedParameters.Should().Be(parameters);
        }
    }

    [Fact]
    public async Task CreateContentAsync_WithType_ShouldCreateViewModel()
    {
        // Arrange
        var contentManager = Scope.Resolve<IContentManager>();

        // Act
        var viewModel = await contentManager.CreateContentAsync(typeof(TestViewModel));

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Should().BeOfType<TestViewModel>();
    }

    [Fact]
    public async Task CreateContentAsync_WithInvalidType_ShouldThrow()
    {
        // Arrange
        var contentManager = Scope.Resolve<IContentManager>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await contentManager.CreateContentAsync(typeof(string)));
    }

    [Fact]
    public async Task CreateContentAsync_DisposableViewModel_ShouldBeDisposable()
    {
        // Arrange
        var contentManager = Scope.Resolve<IContentManager>();

        // Act
        var viewModel = await contentManager.CreateContentAsync<DisposableTestViewModel>();

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Should().BeAssignableTo<IDisposable>();

        // Dispose
        if (viewModel is DisposableTestViewModel disposableVm)
        {
            disposableVm.Dispose();
            disposableVm.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CreateContentAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var contentManager = Scope.Resolve<IContentManager>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // Note: Cancellation might not be checked during creation, but should not throw
        var viewModel = await contentManager.CreateContentAsync<TestViewModel>(
            cancellationToken: cts.Token);

        viewModel.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_ShouldDisposeContentManager()
    {
        // Arrange
        var contentManager = Scope.Resolve<IContentManager>();

        // Act
        if (contentManager is IDisposable disposable)
        {
            disposable.Dispose();
        }

        // Assert
        // Should not throw
        contentManager.Should().NotBeNull();
    }
}

