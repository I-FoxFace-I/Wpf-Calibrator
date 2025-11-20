using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.ViewModels;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Services;
using WpfEngine.Tests.Helpers;
using WpfEngine.Views;
using WpfEngine.Views.Windows;
using Xunit;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for WindowManager (refactored from GlobalWindowService)
/// </summary>
public class WindowManagerTests : AutofacTestFixture
{
    public WindowManagerTests() : base()
    {

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


    [STAFact]
    public void Constructor_InitializesSuccessfully()
    {
        // Assert
        WindowManager.Should().NotBeNull();
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

        // Assert - Allow some time for async close
        //System.Threading.Thread.Sleep(100);
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
        //System.Threading.Thread.Sleep(100);
        WpfTestHelpers.WaitForPendingOperations();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.WindowId.Should().Be(windowId);
        eventArgs.ViewModelType.Should().Be(typeof(TestViewModel));
    }

    [STAFact]
    public void CloseAllChildren_ClosesAllChildWindows()
    {
        var rootId = WindowManager.OpenWindow<TestViewModel>();
        //WpfTestHelpers.WaitForWindowLoaded();

        // Arrange
        var parentId = WindowManager.OpenChildWindow<TestViewModel>(rootId);

        //WpfTestHelpers.WaitForWindowLoaded();

        var child1Id = WindowManager.OpenChildWindow<TestViewModel>(parentId);

        //WpfTestHelpers.WaitForWindowLoaded();

        var child2Id = WindowManager.OpenChildWindow<TestViewModel>(parentId);

        //WpfTestHelpers.WaitForWindowLoaded();


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

    public override void Dispose()
    {
        base.Dispose();
        Container?.Dispose();
    }
}

// ========== TEST TYPES ==========

public class TestViewModel : BaseViewModel
{
    public TestViewModel(ILogger<TestViewModel> logger) : base(logger)
    {
    }
}

public class TestParentViewModel : BaseViewModel
{
    public TestParentViewModel(ILogger<TestViewModel> logger) : base(logger)
    {
    }
}

public class TestWindowParameters : IDisposable
{
    private bool _disposed;
    private readonly Guid _assignedWindowId;
    private ILifetimeScope? _viewModelResolutionScope;

    public Guid AssignedWindowId => _assignedWindowId;
    public ILifetimeScope ViewModelResolutionScope => _viewModelResolutionScope ?? throw new InvalidOperationException("Disposed");

    public TestWindowParameters(Guid assignedWindowId, ILifetimeScope viewModelResolutionScope)
    {
        _assignedWindowId = assignedWindowId;
        _viewModelResolutionScope = viewModelResolutionScope;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _viewModelResolutionScope?.Dispose();
        _viewModelResolutionScope = null;
        GC.SuppressFinalize(this);
    }
}

public class TestParentWindow : BaseWindow, IScopedView, IDisposable
{

    private TestWindowParameters? _parameters;
    public TestParentWindow(ILogger logger, TestWindowParameters parameters) : base(logger)
    {
        _parameters = parameters;
    }
    
    public Guid AssignedWindowId { get; set; }

    public ILifetimeScope ViewModelResolutionScope { get; set; } //=> _parameters!.ViewModelResolutionScope;

    //ILifetimeScope IScopedView.ViewModelResolutionScope { get => ViewModelResolutionScope; set => throw new NotImplementedException(); }

    public void Dispose()
    {
        _parameters?.Dispose();
        _parameters = null;
    }
}

public class TestWindow : ScopedWindow
{
    public TestWindow(ILogger logger) : base(logger)
    {
        // Minimal test window
        Width = 100;
        Height = 100;
        WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
        Left = -10000; // Off-screen
        Top = -10000;
        //WindowId = Guid.NewGuid();
    }

    //public Guid WindowId {  get; private set; }
}

