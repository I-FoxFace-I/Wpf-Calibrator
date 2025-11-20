using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using Xunit;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for ScopedWindowManager (new IScopeManager-based implementation)
/// </summary>
public class ScopedWindowManagerTests : AutofacTestFixture
{
    public ScopedWindowManagerTests() : base()
    {
    }

    /// <summary>
    /// Override to use ScopedWindowManager instead of WindowManager
    /// </summary>
    protected override void RegisterWindowManager(ContainerBuilder builder)
    {
        // Register new IScopeManager
        builder.RegisterType<ScopeManager>()
               .As<IScopeManager>()
               .SingleInstance();
        
        // Register loggers for new components
        builder.Register(c => Mock.Of<ILogger<ScopedWindowManager>>()).SingleInstance();
        builder.Register(c => Mock.Of<ILogger<ScopeManager>>()).SingleInstance();
        builder.Register(c => Mock.Of<ILogger<ScopeSession>>()).InstancePerDependency();
        
        // Register ScopedWindowManager instead of WindowManager
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
        // Register test ViewModel and Window
        builder.RegisterType<TestViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<TestWindow>()
               .AsSelf()
               .InstancePerDependency();

        builder.Register(c => Mock.Of<ILogger<TestWindow>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModel>>()).InstancePerDependency();
    }

    // ========== TESTS - Same as WindowManagerTests ==========

    [STAFact]
    public void Constructor_InitializesSuccessfully()
    {
        // Assert
        WindowManager.Should().NotBeNull();
        WindowManager.Should().BeOfType<ScopedWindowManager>();
    }

    [STAFact]
    public void OpenWindow_CreatesNewWindow_ReturnsValidId()
    {
        // Act
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();
        
        // Assert
        windowId.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(windowId).Should().BeTrue();
    }

    [STAFact]
    public void OpenWindow_RaisesWindowOpenedEvent()
    {
        // Arrange
        WindowOpenedEventArgs? eventArgs = null;
        WindowManager.WindowOpened += (s, e) => eventArgs = e;

        // Act
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.WindowId.Should().Be(windowId);
        eventArgs.ViewModelType.Should().Be(typeof(TestViewModel));
        eventArgs.ParentWindowId.Should().BeNull();
    }

    [STAFact]
    public void OpenChildWindow_FromParent_ReturnsValidId()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var childId = WindowManager.OpenChildWindow<TestViewModel>(parentId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        childId.Should().NotBeEmpty();
        childId.Should().NotBe(parentId);
        WindowManager.IsWindowOpen(childId).Should().BeTrue();
    }

    [STAFact]
    public void GetParentWindowId_ReturnsCorrectParent()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();
        var childId = WindowManager.OpenChildWindow<TestViewModel>(parentId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var retrievedParentId = WindowManager.GetParentWindowId(childId);

        // Assert
        retrievedParentId.Should().Be(parentId);
    }

    [STAFact]
    public void GetChildWindowIds_ReturnsAllChildren()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();
        var child1Id = WindowManager.OpenChildWindow<TestViewModel>(parentId);
        WpfTestHelpers.WaitForWindowLoaded();
        var child2Id = WindowManager.OpenChildWindow<TestViewModel>(parentId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var childIds = WindowManager.GetChildWindowIds(parentId);

        // Assert
        childIds.Should().HaveCount(2);
        childIds.Should().Contain(child1Id);
        childIds.Should().Contain(child2Id);
    }

    [STAFact]
    public void Close_RemovesWindowFromTracking()
    {
        // Arrange
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();
        WindowManager.IsWindowOpen(windowId).Should().BeTrue();

        // Act
        WindowManager.CloseWindow(windowId);

        // Assert
        WpfTestHelpers.WaitForPendingOperations();
        WindowManager.IsWindowOpen(windowId).Should().BeFalse();
    }

    [STAFact]
    public void Close_RaisesWindowClosedEvent()
    {
        // Arrange
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        WindowClosedEventArgs? eventArgs = null;
        WindowManager.WindowClosed += (s, e) => eventArgs = e;

        // Act
        WindowManager.CloseWindow(windowId);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.WindowId.Should().Be(windowId);
        eventArgs.ViewModelType.Should().Be(typeof(TestViewModel));
    }

    [STAFact]
    public void CloseAllChildren_ClosesAllChildWindows()
    {
        // Arrange
        var rootId = WindowManager.OpenWindow<TestViewModel>();
        var parentId = WindowManager.OpenChildWindow<TestViewModel>(rootId);
        var child1Id = WindowManager.OpenChildWindow<TestViewModel>(parentId);
        var child2Id = WindowManager.OpenChildWindow<TestViewModel>(parentId);

        // Act
        WindowManager.CloseAllChildWindows(parentId);
        System.Threading.Thread.Sleep(100);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        WindowManager.IsWindowOpen(child1Id).Should().BeFalse();
        WindowManager.IsWindowOpen(child2Id).Should().BeFalse();
        WindowManager.IsWindowOpen(parentId).Should().BeTrue(); // Parent still open
    }

    [STAFact]
    public void GetWindowViewModelType_ReturnsCorrectType()
    {
        // Arrange
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var vmType = WindowManager.GetViewModelType(windowId);

        // Assert
        vmType.Should().Be(typeof(TestViewModel));
    }

    [STAFact]
    public void GetWindowScope_ReturnsValidScope()
    {
        // Arrange
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var scope = WindowTracker.GetWindowScope(windowId);

        // Assert
        scope.Should().NotBeNull();
    }

    [STAFact]
    public void TryOpenWindow_OnSuccess_ReturnsSuccessResult()
    {
        // Act
        var result = WindowManager.TryOpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(result.Value).Should().BeTrue();
    }

    [STAFact]
    public void TryCloseWindow_OnSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var result = WindowManager.TryCloseWindow(windowId);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        result.IsSuccess.Should().BeTrue();
        WindowManager.IsWindowOpen(windowId).Should().BeFalse();
    }

    // ========== SCOPED WINDOW MANAGER SPECIFIC TESTS ==========

    [STAFact]
    public void OpenWindowInSession_ShouldCreateWindowInSession()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session = scopeManager
            .CreateWorkflowSession("test-workflow")
            .Build();

        // Act
        var windowId = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        windowId.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(windowId).Should().BeTrue();
        
        var sessionWindows = WindowManager.GetSessionWindows(session.SessionId);
        sessionWindows.Should().Contain(windowId);
    }

    [STAFact]
    public void CloseAllSessionWindows_ShouldCloseAllWindowsInSession()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session = scopeManager
            .CreateWorkflowSession("test-workflow")
            .Build();

        var window1Id = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        var window2Id = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        WindowManager.CloseAllSessionWindows(session.SessionId);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        WindowManager.IsWindowOpen(window1Id).Should().BeFalse();
        WindowManager.IsWindowOpen(window2Id).Should().BeFalse();
    }

    [STAFact]
    public void Activate_ShouldActivateWindow()
    {
        // Arrange
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var activated = WindowManager.Activate(windowId);

        // Assert
        activated.Should().BeTrue();
    }

    // ========== ADDITIONAL SESSION-AWARE TESTS ==========

    [STAFact]
    public void OpenWindowInSession_WithParameters_ShouldCreateWindowInSession()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session = scopeManager
            .CreateWorkflowSession("test-workflow")
            .Build();

        var parameters = new WindowSessionTestParameters { CorrelationId = Guid.NewGuid(), Value = "Test" };

        // Act
        var windowId = WindowManager.OpenWindowInSession<TestViewModel, WindowSessionTestParameters>(session.SessionId, parameters);
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        windowId.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(windowId).Should().BeTrue();
        
        var sessionWindows = WindowManager.GetSessionWindows(session.SessionId);
        sessionWindows.Should().Contain(windowId);
    }

    [STAFact]
    public void OpenWindowInSession_WithNonExistentSession_ShouldThrow()
    {
        // Arrange
        var nonExistentSessionId = Guid.NewGuid();

        // Act & Assert
        var action = () => WindowManager.OpenWindowInSession<TestViewModel>(nonExistentSessionId);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"Session {nonExistentSessionId} not found");
    }

    [STAFact]
    public void TryCloseAllSessionWindows_ShouldReturnSuccess()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session = scopeManager
            .CreateWorkflowSession("test-workflow")
            .Build();

        var window1Id = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        var window2Id = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var result = WindowManager.TryCloseAllSessionWindows(session.SessionId);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        result.IsSuccess.Should().BeTrue();
        WindowManager.IsWindowOpen(window1Id).Should().BeFalse();
        WindowManager.IsWindowOpen(window2Id).Should().BeFalse();
    }

    [STAFact]
    public void TryCloseAllSessionWindows_WithNonExistentSession_ShouldReturnSuccess()
    {
        // Arrange
        // When session doesn't exist, GetSessionWindows returns empty list,
        // so no windows are closed, but no exception is thrown
        var nonExistentSessionId = Guid.NewGuid();

        // Act
        var result = WindowManager.TryCloseAllSessionWindows(nonExistentSessionId);

        // Assert
        // Success because no windows to close (empty list doesn't throw)
        result.IsSuccess.Should().BeTrue();
    }

    [STAFact]
    public void OpenChildWindow_InSession_ShouldInheritSession()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session = scopeManager
            .CreateWorkflowSession("test-workflow")
            .Build();

        var parentId = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var childId = WindowManager.OpenChildWindow<TestViewModel>(parentId);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        childId.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(childId).Should().BeTrue();
        
        var childSessionId = WindowTracker.WithMetadata(childId, metadata => metadata.SessionId, null);
        childSessionId.Should().Be(session.SessionId);
    }

    [STAFact]
    public void MultipleSessions_ShouldIsolateWindows()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session1 = scopeManager.CreateWorkflowSession("workflow-1").Build();
        using var session2 = scopeManager.CreateWorkflowSession("workflow-2").Build();

        // Act
        var window1Id = WindowManager.OpenWindowInSession<TestViewModel>(session1.SessionId);
        var window2Id = WindowManager.OpenWindowInSession<TestViewModel>(session2.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        var session1Windows = WindowManager.GetSessionWindows(session1.SessionId);
        var session2Windows = WindowManager.GetSessionWindows(session2.SessionId);
        
        session1Windows.Should().Contain(window1Id);
        session1Windows.Should().NotContain(window2Id);
        
        session2Windows.Should().Contain(window2Id);
        session2Windows.Should().NotContain(window1Id);
    }

    [STAFact]
    public void CloseAllSessionWindows_ShouldCloseOnlyWindowsInSession()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session1 = scopeManager.CreateWorkflowSession("workflow-1").Build();
        using var session2 = scopeManager.CreateWorkflowSession("workflow-2").Build();

        var rootWindowId = WindowManager.OpenWindow<TestViewModel>();
        var session1WindowId = WindowManager.OpenWindowInSession<TestViewModel>(session1.SessionId);
        var session2WindowId = WindowManager.OpenWindowInSession<TestViewModel>(session2.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        WindowManager.CloseAllSessionWindows(session1.SessionId);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        WindowManager.IsWindowOpen(session1WindowId).Should().BeFalse();
        WindowManager.IsWindowOpen(session2WindowId).Should().BeTrue(); // Other session still open
        WindowManager.IsWindowOpen(rootWindowId).Should().BeTrue(); // Root window still open
    }

    [STAFact]
    public void OpenWindowInSession_WithDatabaseSession_ShouldWork()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session = scopeManager
            .CreateDatabaseSession("test-db")
            .Build();

        // Act
        var windowId = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        windowId.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(windowId).Should().BeTrue();
    }

    [STAFact]
    public void OpenWindowInSession_WithCustomSession_ShouldWork()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        using var session = scopeManager
            .CreateCustomSession("custom-operation")
            .Build();

        // Act
        var windowId = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        windowId.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(windowId).Should().BeTrue();
    }

    [STAFact]
    public void SessionDisposal_ShouldCloseAllWindowsInSession()
    {
        // Arrange
        var scopeManager = Container.Resolve<IScopeManager>();
        var session = scopeManager
            .CreateWorkflowSession("test-workflow")
            .Build();

        var window1Id = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        var window2Id = WindowManager.OpenWindowInSession<TestViewModel>(session.SessionId);
        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        // Session.Close() should automatically close all windows in the session
        session.Close();
        WpfTestHelpers.WaitForPendingOperations();
        
        // Assert
        WindowManager.IsWindowOpen(window1Id).Should().BeFalse();
        WindowManager.IsWindowOpen(window2Id).Should().BeFalse();
    }

    public override void Dispose()
    {
        base.Dispose();
        Container?.Dispose();
    }
}

// ========== TEST TYPES ==========

public class WindowSessionTestParameters : IViewModelParameters
{
    public Guid CorrelationId { get; set; }
    public string Value { get; set; } = string.Empty;
}

