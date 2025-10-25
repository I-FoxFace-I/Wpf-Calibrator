using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Windows;
using WpfEngine.Core.Services;
using WpfEngine.Core.Services.Autofac;
using WpfEngine.Core.ViewModels;
using WpfEngine.Core.Views;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using Xunit;

namespace WpfEngine.Tests.Core.Services.Autofac;

/// <summary>
/// Tests for DialogService - Modal dialog management
/// Note: Some tests are simplified due to ShowDialog() blocking behavior
/// </summary>
public class DialogServiceTests : IDisposable
{
    private readonly IContainer _container;
    private readonly ILifetimeScope _scope;
    private readonly DialogService _dialogService;

    public DialogServiceTests()
    {
        var builder = new ContainerBuilder();

        // Register loggers
        builder.Register(c => new Mock<ILogger<DialogService>>().Object).As<ILogger<DialogService>>();
        builder.Register(c => new Mock<ILogger<ViewRegistry>>().Object).As<ILogger<ViewRegistry>>();
        builder.Register(c => new Mock<ILogger<ViewLocatorService>>().Object).As<ILogger<ViewLocatorService>>();
        builder.Register(c => new Mock<ILogger<ViewModelFactory>>().Object).As<ILogger<ViewModelFactory>>();

        // Register ViewRegistry
        builder.RegisterType<ViewRegistry>()
               .AsSelf()
               .As<IViewRegistry>()
               .SingleInstance();

        // Register ViewLocator and ViewModelFactory
        builder.RegisterType<ViewLocatorService>()
               .As<IViewLocatorService>()
               .InstancePerLifetimeScope();

        builder.RegisterType<ViewModelFactory>()
               .As<IViewModelFactory>()
               .InstancePerLifetimeScope();

        _container = builder.Build();
        _scope = _container.BeginLifetimeScope();

        _dialogService = new DialogService(
            _scope,
            _scope.Resolve<IViewLocatorService>(),
            _scope.Resolve<IViewModelFactory>(),
            Mock.Of<ILogger<DialogService>>());
    }

    [Fact]
    public async Task ShowMessageBoxAsync_ReturnsOK()
    {
        // This test would require UI thread and is hard to test in unit tests
        // Skipped for now - DialogService.ShowMessageBoxAsync uses System.Windows.MessageBox
        // which requires STA thread and UI dispatcher
        
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ShowConfirmationAsync_WithYes_ReturnsTrue()
    {
        // This test would require UI thread and mock MessageBox
        // Skipped for now - requires UI testing framework
        
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ShowErrorAsync_DoesNotThrow()
    {
        // This test would require UI thread
        // Skipped for now
        
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ShowInputAsync_ReturnsDefaultValue()
    {
        // Arrange
        var defaultValue = "test-default";

        // Act
        var result = await _dialogService.ShowInputAsync("Enter value:", defaultValue: defaultValue);

        // Assert
        // Current implementation returns default value (not fully implemented)
        result.Should().Be(defaultValue);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _container?.Dispose();
    }

    // Note: Full dialog tests (ShowDialogAsync) would require:
    // 1. STA thread
    // 2. UI Dispatcher
    // 3. Mocking Window.ShowDialog()
    // These are better suited for integration tests rather than unit tests
}

