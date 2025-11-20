using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.ViewModels;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Sessions;
using WpfEngine.Services;
using WpfEngine.Services.Sessions.Implementation;
using WpfEngine.Tests.Helpers;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;
using Xunit;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Tests.Core.Sessions;

/// <summary>
/// Tests for auto-close functionality of SessionBuilder using AutofacTestFixture
/// </summary>
public class SessionBuilderAutoCloseTests : AutofacTestFixture
{
    protected override IViewRegistry RegisterMapping(IViewRegistry viewRegistry)
    {
        // Register test view mappings
        return viewRegistry
            .MapWindow<TestViewModel, TestWindow>()
            .MapWindow<TestViewModelWithParameters, TestWindowWithParameters>();
    }

    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register test ViewModels


        builder.RegisterType<TestViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestViewModelWithParameters>().AsSelf().InstancePerDependency();
        
        // Register test windows
        builder.RegisterType<TestWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestWindowWithParameters>().AsSelf().InstancePerDependency();

        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestViewModelWithParameters>>()).InstancePerDependency();
    }

    [STAFact]
    public void AutoCloseWhenEmpty_WhenEnabled_ShouldCloseSessionAfterAllWindowsClosed()
    {
        // Arrange
        var scopeManager = Resolve<IScopeManager>();
        var windowManager = WindowManager;
        
        var builder = new SessionBuilder(Scope, ScopeTag.Workflow(), scopeManager);
        
        // Act - build session with AutoCloseWhenEmpty and open window
        var session = builder
            .AutoCloseWhenEmpty()
            .OpenWindow<TestViewModel>();
        
        // Assert - session should be active
        session.IsActive.Should().BeTrue();
        session.WindowCount.Should().Be(1);
        
        // Get the window ID from window tracker
        var windows = WindowTracker.OpenWindows;
        windows.Should().HaveCount(1);
        var windowId = windows.First();
        
        // Close the window
        windowManager.CloseWindow(windowId);
        
        // Session should be closed automatically
        session.IsActive.Should().BeFalse();
        session.WindowCount.Should().Be(0);
        
        // Cleanup
        session.Dispose();
    }

    [STAFact]
    public void AutoCloseWhenEmpty_WhenNotEnabled_ShouldNotCloseSessionAfterAllWindowsClosed()
    {
        // Arrange
        var scopeManager = Resolve<IScopeManager>();
        var windowManager = WindowManager;
        
        var builder = new SessionBuilder(Scope, ScopeTag.Workflow(), scopeManager);
        
        // Act - build session WITHOUT AutoCloseWhenEmpty and open window
        var session = builder.OpenWindow<TestViewModel>();
        
        // Assert - session should be active
        session.IsActive.Should().BeTrue();
        session.WindowCount.Should().Be(1);
        
        // Get the window ID from window tracker
        var windows = WindowTracker.OpenWindows;
        windows.Should().HaveCount(1);
        var windowId = windows.First();
        
        // Close the window
        windowManager.CloseWindow(windowId);
        
        // Session should still be active (not auto-closed)
        session.IsActive.Should().BeTrue();
        session.WindowCount.Should().Be(0);
        
        // Cleanup
        session.Dispose();
    }

    [STAFact]
    public void AutoCloseWhenEmpty_WithMultipleWindows_ShouldOnlyCloseWhenAllWindowsClosed()
    {
        // Arrange
        var scopeManager = Resolve<IScopeManager>();
        var windowManager = WindowManager;
        
        var builder = new SessionBuilder(Scope, ScopeTag.Workflow(), scopeManager);
        
        // Act - build session with AutoCloseWhenEmpty
        var session = builder.AutoCloseWhenEmpty().Build();
        
        // Open first window
        var windowId1 = session.OpenWindow<TestViewModel>();
        
        // Open second window
        var windowId2 = session.OpenWindow<TestViewModel>();
        
        session.WindowCount.Should().Be(2);
        session.IsActive.Should().BeTrue();
        
        // Close first window
        windowManager.CloseWindow(windowId1);
        
        // Session should still be active (one window remaining)
        session.WindowCount.Should().Be(1);
        session.IsActive.Should().BeTrue();
        
        // Close second window
        windowManager.CloseWindow(windowId2);
        
        // Now session should be closed (all windows closed)
        session.WindowCount.Should().Be(0);
        session.IsActive.Should().BeFalse();
        
        // Cleanup
        session.Dispose();
    }

    [STAFact]
    public void AutoCloseWhenEmpty_ShouldOnlyTrackWindowsFromSameSession()
    {
        // Arrange
        var scopeManager = Resolve<IScopeManager>();
        var windowManager = WindowManager;
        
        var builder1 = new SessionBuilder(Scope, ScopeTag.Workflow(), scopeManager);
        var builder2 = new SessionBuilder(Scope, ScopeTag.Workflow(), scopeManager);
        
        // Act - build two sessions with AutoCloseWhenEmpty
        var session1 = builder1.AutoCloseWhenEmpty().Build();
        var session2 = builder2.AutoCloseWhenEmpty().Build();
        
        // Open window in session1
        var session1WindowId = session1.OpenWindow<TestViewModel>();
        
        // Open window in session2
        var session2WindowId = session2.OpenWindow<TestViewModel>();
        
        session1.WindowCount.Should().Be(1);
        session2.WindowCount.Should().Be(1);
        
        // Close window from session2
        windowManager.CloseWindow(session2WindowId);
        
        // Session1 should still be active with 1 window
        session1.WindowCount.Should().Be(1);
        session1.IsActive.Should().BeTrue();
        
        // Session2 should be closed (all its windows closed)
        session2.WindowCount.Should().Be(0);
        session2.IsActive.Should().BeFalse();
        
        // Close window from session1
        windowManager.CloseWindow(session1WindowId);
        
        // Now session1 should also be closed
        session1.WindowCount.Should().Be(0);
        session1.IsActive.Should().BeFalse();
        
        // Cleanup
        session1.Dispose();
        session2.Dispose();
    }

    [STAFact]
    public void OpenWindow_WithAutoCloseWhenEmpty_ShouldBuildAndTrackWindow()
    {
        // Arrange
        var scopeManager = Resolve<IScopeManager>();
        
        var builder = new SessionBuilder(Scope, ScopeTag.Workflow(), scopeManager);
        
        // Act
        var session = builder
            .AutoCloseWhenEmpty()
            .OpenWindow<TestViewModel>();
        
        // Assert
        session.Should().NotBeNull();
        session.IsActive.Should().BeTrue();
        session.WindowCount.Should().Be(1);
        
        // Verify window is tracked
        var windows = WindowTracker.OpenWindows;
        windows.Should().HaveCount(1);
        
        // Cleanup
        session.Dispose();
    }

    // Test ViewModels
    public class TestViewModel : BaseViewModel
    {
        public TestViewModel(ILogger<TestViewModel> logger) : base(logger)
        {
        }
    }

    public class TestViewModelWithParameters : BaseViewModel, IViewModel<TestParameters>
    {
        public TestParameters? Parameter { get; private set; }
        
        public TestViewModelWithParameters(ILogger<TestViewModelWithParameters> logger) : base(logger)
        {
        }
        
        public Task InitializeAsync(TestParameters parameter)
        {
            Parameter = parameter;
            return Task.CompletedTask;
        }
    }

    public class TestParameters : IViewModelParameters
    {
        public string Value { get; set; } = string.Empty;
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }

    // Test Windows
    public class TestWindow : ScopedWindow
    {
        public TestWindow(ILogger logger) : base(logger)
        {
            Width = 100;
            Height = 80;
            WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            Left = -10000;
            Top = -10000;
        }
    }

    public class TestWindowWithParameters : ScopedWindow
    {
        public TestWindowWithParameters(ILogger logger) : base(logger)
        {
            Width = 100;
            Height = 80;
            WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            Left = -10000;
            Top = -10000;
        }
    }
}


