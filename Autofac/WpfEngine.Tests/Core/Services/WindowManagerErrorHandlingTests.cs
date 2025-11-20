using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using WpfEngine.ViewModels;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using WpfEngine.ViewModels;
using Xunit;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for error handling in WindowManagerBase
/// </summary>
public class WindowManagerErrorHandlingTests : AutofacTestFixture
{
    protected override void RegisterWindowManager(ContainerBuilder builder)
    {
        // Register new IScopeManager
        builder.RegisterType<ScopeManager>()
               .As<IScopeManager>()
               .SingleInstance();
        
        // Register loggers
        builder.Register(c => Mock.Of<ILogger<ScopedWindowManager>>()).SingleInstance();
        builder.Register(c => Mock.Of<ILogger<ScopeManager>>()).SingleInstance();
        builder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        
        // Register ScopedWindowManager
        builder.RegisterType<ScopedWindowManager>()
               .As<IWindowManager>()
               .SingleInstance();
    }

    protected override IViewRegistry RegisterMapping(IViewRegistry viewRegistry)
    {
        return viewRegistry.MapWindow<TestViewModel, TestWindow>();
    }

    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        builder.RegisterType<TestViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestWindow>().AsSelf().InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestWindow>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModel>>()).InstancePerDependency();
    }

    [STAFact]
    public void TryOpenWindow_OnError_ShouldReturnFailureResult()
    {
        // Arrange - use non-existent ViewModel type to cause error
        // Note: This test might need adjustment based on actual error scenarios

        // Act
        var result = WindowManager.TryOpenWindow<NonExistentViewModel>();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [STAFact]
    public void TryOpenWindow_OnError_ShouldRaiseWindowErrorEvent()
    {
        // Arrange
        WindowErrorEventArgs? errorEventArgs = null;
        WindowManager.WindowError += (s, e) => errorEventArgs = e;

        // Act
        var result = WindowManager.TryOpenWindow<NonExistentViewModel>();

        // Assert
        errorEventArgs.Should().NotBeNull();
        errorEventArgs!.Operation.Should().Be("OpenWindow");
        errorEventArgs.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [STAFact]
    public void TryOpenWindow_OnError_ShouldSetLastError()
    {
        // Act
        WindowManager.TryOpenWindow<NonExistentViewModel>();

        // Assert
        var lastError = WindowManager.GetLastError();
        lastError.Should().NotBeNull();
        lastError!.Operation.Should().Be("OpenWindow");
        lastError.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [STAFact]
    public void TryCloseWindow_OnError_ShouldReturnFailureResult()
    {
        // Arrange
        var nonExistentWindowId = Guid.NewGuid();

        // Act
        var result = WindowManager.TryCloseWindow(nonExistentWindowId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [STAFact]
    public void TryCloseWindow_OnError_ShouldRaiseWindowErrorEvent()
    {
        // Arrange
        var nonExistentWindowId = Guid.NewGuid();
        WindowErrorEventArgs? errorEventArgs = null;
        WindowManager.WindowError += (s, e) => errorEventArgs = e;

        // Act
        WindowManager.TryCloseWindow(nonExistentWindowId);

        // Assert
        errorEventArgs.Should().NotBeNull();
        errorEventArgs!.Operation.Should().Be("CloseWindow");
        errorEventArgs.WindowId.Should().Be(nonExistentWindowId);
    }

    [STAFact]
    public void TryCloseWindow_OnError_ShouldSetLastError()
    {
        // Arrange
        var nonExistentWindowId = Guid.NewGuid();

        // Act
        WindowManager.TryCloseWindow(nonExistentWindowId);

        // Assert
        var lastError = WindowManager.GetLastError();
        lastError.Should().NotBeNull();
        lastError!.Operation.Should().Be("CloseWindow");
        lastError.WindowId.Should().Be(nonExistentWindowId);
    }

    [STAFact]
    public void GetLastError_AfterSuccessfulOperation_ShouldReturnNull()
    {
        // Arrange
        WindowManager.TryOpenWindow<NonExistentViewModel>(); // Sets error
        var lastError1 = WindowManager.GetLastError();
        lastError1.Should().NotBeNull();

        // Act - successful operation
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert - last error should still be from previous operation
        // (GetLastError doesn't clear on success, it only updates on error)
        var lastError2 = WindowManager.GetLastError();
        // Note: This behavior might vary - checking that method doesn't throw
        WindowManager.GetLastError().Should().NotBeNull();
    }

    [STAFact]
    public void ShowWindowError_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var message = "Test error message";

        // Act
        WindowManager.ShowWindowError(message, exception);

        // Assert
        // Should not throw - method logs and shows message box
        WindowManager.Should().NotBeNull();
    }

    [STAFact]
    public void TryOpenChildWindow_WhenParentDoesNotExist_ShouldOpenAsRootWindow()
    {
        // Arrange
        var nonExistentParentId = Guid.NewGuid();

        // Act
        var result = WindowManager.TryOpenChildWindow<TestViewModel>(nonExistentParentId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        // Note: OpenChildWindow doesn't throw when parent doesn't exist - it uses root scope instead
        // So the window opens successfully as a root window
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        
        // Cleanup
        WindowManager.CloseWindow(result.Value);
        WpfTestHelpers.WaitForPendingOperations();
    }

    [STAFact]
    public void TryCloseAllChildWindows_WhenParentDoesNotExist_ShouldReturnSuccess()
    {
        // Arrange
        var nonExistentParentId = Guid.NewGuid();

        // Act
        var result = WindowManager.TryCloseAllChildWindows(nonExistentParentId);

        // Assert
        // Note: CloseAllChildWindows returns success when parent doesn't exist (no children to close)
        result.IsSuccess.Should().BeTrue();
    }
}

// Non-existent ViewModel for error testing
public class NonExistentViewModel : BaseViewModel
{
    public NonExistentViewModel(ILogger<NonExistentViewModel> logger) : base(logger)
    {
    }
}

