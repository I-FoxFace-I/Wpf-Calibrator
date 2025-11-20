using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Moq;
using WpfEngine.ViewModels;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using WpfEngine.Views;
using Xunit;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for WindowContext (refactored from LocalWindowService)
/// </summary>
public class WindowContextTests : AutofacTestFixture
{
    private readonly Mock<ILogger<WindowContext>> _loggerMock;

    public WindowContextTests() : base()
    {
        _loggerMock = new Mock<ILogger<WindowContext>>();

    }

    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register test ViewModel
        builder.RegisterType<TestViewModel>()
               .AsSelf()
               .InstancePerDependency();

        builder.RegisterType<TestWindow>()
               .AsSelf()
               .InstancePerDependency();

        //builder.RegisterType<Func<TestParentWindow>>()
        //       .AsSelf()
        //       .InstancePerDependency();

        builder.Register(c => Mock.Of<ILogger<TestViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestWindow>>()).InstancePerDependency();
    }

    protected override IViewRegistry RegisterMapping(IViewRegistry viewRegistry)
    {
        return viewRegistry.MapWindow<TestViewModel, TestWindow>();
    }

    private TestWindowParameters GetWindowParameters(Guid parentId)
    {
        return new TestWindowParameters(parentId, GetScopeWithParentId(parentId));
    }

    private ILifetimeScope GetScopeWithParentId(Guid parentId)
    {
        return Scope.BeginLifetimeScope($"Parent-ID-{parentId}", builder =>
        {
            var scopeViewMock = new Mock<IScopedView>();

            scopeViewMock.SetupGet(x => x.AssignedWindowId).Returns(parentId);

            // Register this window instance in its scope
            // This allows IWindowContext to find us via IScopedView
            builder.RegisterInstance(scopeViewMock.Object as IScopedView)
                   .SingleInstance();

            // Hook for derived classes to register additional services
            ConfigureScope(builder);
        });
    }

    /// <summary>
    /// Override this to register additional services in window scope
    /// </summary>
    protected virtual void ConfigureScope(ContainerBuilder builder)
    {
        // Override in derived classes if needed
    }

    private IWindowContext GetTestWindowContext(Guid parentId)
    {
        var scope = WindowTracker.GetWindowScope(parentId) ?? Scope;

        scope.Should().NotBeNull();

        var context = scope!.Resolve<IWindowContext>();

        context.Should().NotBeNull();
        
        context.WindowId.Should().Be(parentId);

        return context;
    }

    [STAFact]
    public void Constructor_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        var scope = WindowTracker.GetWindowScope(parentId);
        var context = GetTestWindowContext(parentId);

        //var context = GetScopeWithParentId(Guid.NewGuid()).Resolve<IWindowContext>();

        // Assert
        context.Should().NotBeNull();
        context.WindowId.Should().NotBe(Guid.Empty); // Not set yet
    }

    [STAFact]
    public void OpenChild_WithoutWindowId_ThrowsException()
    {
        // Arrange


        // Act
        var act = () =>
        {
            var context = GetTestWindowContext(Guid.Empty);
            context.OpenWindow<TestViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();
        };

        // Assert
        act.Should().Throw();
    }

    [STAFact]
    public void OpenChild_WithValidWindowId_OpensChild()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        var scope = WindowTracker.GetWindowScope(parentId);
        var context = GetTestWindowContext(parentId);

        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var childId = context.OpenWindow<TestViewModel>();

        WpfTestHelpers.WaitForWindowLoaded();

        // Assert
        childId.Should().NotBeEmpty();
        WindowManager.IsWindowOpen(childId).Should().BeTrue();
        WindowManager.GetParentWindowId(childId).Should().Be(parentId);
    }

    [STAFact]
    public void CloseWindow_ClosesThisWindow()
    {
        // Arrange
        var windowId = WindowManager.OpenWindow<TestViewModel>();
        var context = GetTestWindowContext(windowId);

        WpfTestHelpers.WaitForWindowLoaded();


        // Act
        context.CloseWindow();

        System.Threading.Thread.Sleep(100);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        WindowManager.IsWindowOpen(windowId).Should().BeFalse();
    }

    [STAFact]
    public void CloseChildren_ClosesAllChildren()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        
        var context = GetTestWindowContext(parentId);

        //WpfTestHelpers.WaitForWindowLoaded();

        var child1Id = context.OpenWindow<TestViewModel>();

        //WpfTestHelpers.WaitForWindowLoaded();

        var child2Id = context.OpenWindow<TestViewModel>();

        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        context.CloseAllChildWindows();

        Task.Delay(1000).Wait();
        //System.Threading.Thread.Sleep(100);
        WpfTestHelpers.WaitForWindowClosed();

        // Assert
        WindowManager.IsWindowOpen(child1Id).Should().BeFalse();
        WindowManager.IsWindowOpen(child2Id).Should().BeFalse();
        WindowManager.IsWindowOpen(parentId).Should().BeTrue(); // Parent still open
    }

    [STAFact]
    public void GetChildIds_ReturnsTrackedChildren()
    {
        // Arrange
        var rootId = WindowManager.OpenWindow<TestViewModel>();

        WindowTracker.GetWindowScope(rootId);


        var parentId = WindowManager.OpenChildWindow<TestViewModel>(rootId);
        
        var scope = WindowTracker.GetWindowScope(parentId);
        
        var context = scope!.Resolve<IWindowContext>();

        context.WindowId.Should().Be(parentId);

        WpfTestHelpers.WaitForWindowLoaded();

        var child1Id = context.OpenWindow<TestViewModel>();

        WpfTestHelpers.WaitForWindowLoaded();

        var child2Id = context.OpenWindow<TestViewModel>();

        WpfTestHelpers.WaitForWindowLoaded();

        // Act
        var childIds = context.GetChildIds();

        // Assert
        childIds.Should().HaveCount(2);
        childIds.Should().Contain(child1Id);
        childIds.Should().Contain(child2Id);
    }

    [STAFact]
    public void HasChildren_ReturnsTrueWhenHasChildren()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        var context = GetTestWindowContext(parentId);

        WpfTestHelpers.WaitForWindowLoaded();

        // Act & Assert - Before opening child
        context.ChildWindowsCount.Should().Be(0);

        // Open child
        context.OpenWindow<TestViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        // Act & Assert - After opening child
        context.ChildWindowsCount.Should().BeGreaterThan(0);
    }

    [STAFact]
    public void ChildClosed_EventRaisedWhenChildCloses()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        var context = GetTestWindowContext(parentId);

        WpfTestHelpers.WaitForWindowLoaded();

        ChildWindowClosedEventArgs? eventArgs = null;
        context.ChildClosed += (s, e) => eventArgs = e;

        var childId = context.OpenWindow<TestViewModel>();

        // Act
        WindowManager.CloseWindow(childId);
        //System.Threading.Thread.Sleep(100);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.ChildWindowId.Should().Be(childId);
        eventArgs.ViewModelType.Should().Be(typeof(TestViewModel));
    }

    [STAFact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var parentId = WindowManager.OpenWindow<TestViewModel>();
        var context = GetTestWindowContext(parentId);

        WpfTestHelpers.WaitForWindowLoaded();

        context.OpenWindow<TestViewModel>();

        // Act
        context.Dispose();
        WpfTestHelpers.WaitForWindowLoaded();
        // Assert - Should not throw
        context.GetChildIds().Should().BeEmpty();
    }

    public void Dispose()
    {
        Container?.Dispose();
    }
}
