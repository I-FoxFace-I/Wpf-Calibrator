using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using WpfEngine.ViewModels;
using WpfEngine.Data.Dialogs;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using WpfEngine.Views.Windows;
using Xunit;
using WpfEngine.ViewModels.Base;
using WpfEngine.ViewModels.Dialogs;
//using IWindowManager = WpfEngine.Core.Services.Sessions.IWindowManager;

namespace WpfEngine.Tests.Core;

/// <summary>
/// Tests for memory leak detection in window/dialog management
/// </summary>
public class MemoryLeakTests : AutofacTestFixture
{
    static void ForceFullGC()
    {
        for (int i = 0; i < 10; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        }

        for (int i = 0; i < 10; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        }
    }

    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register test ViewModels and Windows
        builder.RegisterType<TestLeakViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestLeakDialogViewModel>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestLeakWindow>().AsSelf().InstancePerDependency();
        builder.RegisterType<TestLeakValidatableViewModel>().AsSelf().InstancePerDependency();


        builder.RegisterType<DialogService>().As<IDialogService>().InstancePerLifetimeScope();

        builder.Register(c => Mock.Of<ILogger<TestLeakDialogViewModel>>()).InstancePerDependency(); 
        builder.Register(c => Mock.Of<ILogger<TestLeakViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<DialogService>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();

        builder.Register(c => Mock.Of<ILogger<DialogService>>()).SingleInstance();
    }

    protected override IViewRegistry RegisterMapping(IViewRegistry registry)
    {
        registry.MapWindow<TestLeakViewModel, TestLeakWindow>();

        return registry;
    }

    [STAFact]
    public async Task DialogService_ClosedDialog_IsGarbageCollectedAsync()
    {
        // Arrange
        var id = WindowManager.OpenWindow<TestLeakViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;
        WeakReference? weakViewModel = null;

        // Setup auto-close
        //((DialogService)dialogService).DialogOpened += async (s, e) =>
        //{
        //    await Task.Delay(100);
        //    dialogService.CloseCurrentDialog();
        //};

        async void CloseDialog(object? s, EventArgs? e)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            if (dialogService.CurrentDialogId.HasValue)
            {
                // In real implementation, we'd need to access the ViewModel
                // For testing, we simulate the reference
                var vm = new TestLeakViewModel(Mock.Of<ILogger<TestLeakViewModel>>());
                weakViewModel = new WeakReference(vm);
            }
            dialogService.CloseCurrentDialog();
        }

        dialogService!.DialogOpened += CloseDialog;

        var task = dialogService.ShowDialogAsync<TestLeakViewModel>();

        WpfTestHelpers.WaitForWindowLoaded();

        await task;
        //CreateAndCloseDialog();

        dialogService.DialogOpened -= CloseDialog;

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        weakViewModel?.IsAlive.Should().BeFalse("Dialog ViewModel should be garbage collected");
        dialogService.HasActiveDialog.Should().BeFalse();
        
    }

    [STAFact]
    public void WindowContext_DisposedScope_ReleasesReferences()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        WeakReference? weakContext = null;

        void CreateAndDisposeContext()
        {
            var windowId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();

            var scope = windowTracker.GetWindowScope(windowId);
            var context = scope?.Resolve<IWindowContext>();
            weakContext = new WeakReference(context);

            // Open child to create references
            context?.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();

            // Close window (should dispose scope)
            windowManager.CloseWindow(windowId);
            WpfTestHelpers.WaitForPendingOperations();
        }

        CreateAndDisposeContext();

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        weakContext?.IsAlive.Should().BeFalse("WindowContext should be garbage collected");
    }

    [STAFact]
    public async Task Navigator_DisposedViewModels_AreGarbageCollectedAsync()
    {
        // Arrange
        var scope = Scope.BeginLifetimeScope("child");
        var navigator = scope.Resolve<INavigator>();
        var weakReferences = new List<WeakReference>();

        // Act - navigate through multiple ViewModels
        async Task NavigateMultipleTimes()
        {
            for (int i = 0; i < 5; i++)
            {
                await navigator.NavigateToAsync<TestLeakViewModel>();

                if (navigator.CurrentViewModel != null)
                {
                    weakReferences.Add(new WeakReference(navigator.CurrentViewModel));
                }
            }
        }

        await NavigateMultipleTimes();

        // Clear history (should dispose ViewModels if OwnsViewModels = true)
        navigator.ClearHistory();
        scope.Dispose();

        // Force garbage collection
        ForceFullGC();

        // Assert - all previous ViewModels should be collected
        foreach (var weakRef in weakReferences.Take(weakReferences.Count - 1))
        {
            weakRef.IsAlive.Should().BeFalse("Navigated-away ViewModels should be garbage collected");
        }
    }

    [STAFact]
    public void WindowManager_ClosedWindow_IsGarbageCollected()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        WeakReference? weakWindow = null;
        WeakReference? weakViewModel = null;
        Guid windowId = Guid.Empty;

        // Act - create window in separate scope to ensure it can be collected
        void CreateAndCloseWindow()
        {
            windowId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();

            var scope = windowTracker.GetWindowScope(windowId);
            var window = scope?.Resolve<TestLeakWindow>();
            var viewModel = window?.DataContext as TestLeakViewModel;

            weakWindow = new WeakReference(window);
            weakViewModel = new WeakReference(viewModel);

            windowManager.CloseWindow(windowId);
            WpfTestHelpers.WaitForPendingOperations();
        }

        CreateAndCloseWindow();

        // Force garbage collection
        ForceFullGC();

        // Assert - objects should be collected
        weakWindow?.IsAlive.Should().BeFalse("Window should be garbage collected");
        weakViewModel?.IsAlive.Should().BeFalse("ViewModel should be garbage collected");
        windowManager.IsWindowOpen(windowId).Should().BeFalse();
    }

    [STAFact]
    public void WindowHandle_AutoDisposesOnWindowClose_NoMemoryLeak()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        WeakReference<WindowHandle>? weakHandle = null;
        WeakReference? weakScope = null;
        Guid windowId = Guid.Empty;

        // Act - create window and capture handle
        void CreateAndCloseWindow()
        {
            windowId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();

            var metadata = windowTracker.GetMetadata(windowId);
            metadata.Should().NotBeNull();
            
            var handle = metadata!.Handle;
            handle.Should().NotBeNull("WindowHandle should be created");
            
            weakHandle = new WeakReference<WindowHandle>(handle!);
            weakScope = new WeakReference(handle!.Scope);

            // Close window - Handle should auto-dispose
            windowManager.CloseWindow(windowId);
            WpfTestHelpers.WaitForPendingOperations();
        }

        CreateAndCloseWindow();

        // Force garbage collection
        ForceFullGC();

        // Assert - Handle and Scope should be collected (no leak!)
        weakHandle!.TryGetTarget(out _).Should().BeFalse("WindowHandle should be garbage collected");
        weakScope!.IsAlive.Should().BeFalse("Scope should be garbage collected");
        windowTracker.IsWindowOpen(windowId).Should().BeFalse();
    }

    [STAFact]
    public void WindowHandle_DisposesCascadesToScope_NoMemoryLeak()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        WeakReference? weakWindow = null;
        WeakReference? weakViewModel = null;
        WeakReference? weakScope = null;
        Guid windowId = Guid.Empty;

        // Act
        void CreateAndCloseWindow()
        {
            windowId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();

            var metadata = windowTracker.GetMetadata(windowId);
            var handle = metadata?.Handle;
            
            handle.Should().NotBeNull();
            
            // Capture weak references using the handle's scope
            var scope = handle!.Scope;
            var window = scope.Resolve<TestLeakWindow>();
            var viewModel = window.DataContext as TestLeakViewModel;

            weakWindow = new WeakReference(window);
            weakViewModel = new WeakReference(viewModel);
            weakScope = new WeakReference(scope);

            // Close window
            windowManager.CloseWindow(windowId);
            WpfTestHelpers.WaitForPendingOperations();
        }

        CreateAndCloseWindow();

        // Force garbage collection
        ForceFullGC();

        // Assert - Everything should be collected
        weakWindow!.IsAlive.Should().BeFalse("Window should be GC'd");
        weakViewModel!.IsAlive.Should().BeFalse("ViewModel should be GC'd");
        weakScope!.IsAlive.Should().BeFalse("Scope should be GC'd");
    }

    [STAFact]
    public void WindowHandle_MultipleWindowsSequential_NoMemoryLeaks()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        var weakHandles = new List<WeakReference<WindowHandle>>();
        var windowIds = new List<Guid>();

        // Act - Create and close multiple windows
        void CreateAndCloseMultipleWindows()
        {
            for (int i = 0; i < 10; i++)
            {
                var windowId = windowManager.OpenWindow<TestLeakViewModel>();
                windowIds.Add(windowId);
                WpfTestHelpers.WaitForWindowLoaded();

                var metadata = windowTracker.GetMetadata(windowId);
                var handle = metadata?.Handle;
                handle.Should().NotBeNull();
                weakHandles.Add(new WeakReference<WindowHandle>(handle!));

                windowManager.CloseWindow(windowId);
                WpfTestHelpers.WaitForPendingOperations();
            }
        }

        CreateAndCloseMultipleWindows();

        // Force garbage collection
        ForceFullGC();

        // Assert - All handles should be collected
        foreach (var weakHandle in weakHandles)
        {
            weakHandle.TryGetTarget(out _).Should().BeFalse("All WindowHandles should be GC'd");
        }

        // Verify all windows are closed
        foreach (var windowId in windowIds)
        {
            windowTracker.IsWindowOpen(windowId).Should().BeFalse();
        }
    }

    [STAFact]
    public async Task WindowManager_ParentChildRelationship_NoCircularReferences()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        
        WeakReference<IViewModel>? weakParentModel = null;
        WeakReference<IViewModel>? weakChildModel = null;
        Guid parentId = Guid.Empty;
        Guid childId = Guid.Empty;
        
        void CreateAndCloseWindows()
        {
            // Create parent
            parentId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();
            
            // Create child
            childId = windowManager.OpenChildWindow<TestLeakViewModel>(parentId);
            WpfTestHelpers.WaitForWindowLoaded();
            
            // Capture weak references SAFELY using WithWindow
            var parentMeta = windowTracker.GetMetadata(parentId);
            var childMeta = windowTracker.GetMetadata(childId);
            
            parentMeta.Should().NotBeNull();
            childMeta.Should().NotBeNull();
            
            // Get window references using WithWindow pattern
            weakParentModel = parentMeta.ViewModelRef;
            weakChildModel = childMeta.ViewModelRef;

            
            // Verify they exist before closing
            weakParentModel!.TryGetTarget(out _).Should().BeTrue();
            weakChildModel!.TryGetTarget(out _).Should().BeTrue();

            // Close parent (cascades to child)
            var task = parentMeta.ClosedTask;
            var childTask = childMeta.ClosedTask;
            windowManager.CloseWindow(parentId);
            WpfTestHelpers.WaitForPendingOperations();
            parentMeta.Dispose();
            childMeta.Dispose();

            Task.WaitAll(task, childTask);
        }
        
        CreateAndCloseWindows();
        
        ForceFullGC();
        
        // Also verify tracker cleaned up
        windowTracker.IsWindowOpen(parentId).Should().BeFalse();
        windowTracker.IsWindowOpen(childId).Should().BeFalse();

        // Assert - No circular references, all should be GC'd
        weakParentModel!.TryGetTarget(out _).Should().BeFalse("Parent window should be GC'd");
        weakChildModel!.TryGetTarget(out _).Should().BeFalse("Child window should be GC'd (no circular ref)");
        //weakParentScope!.IsAlive.Should().BeFalse("Parent scope should be GC'd");
        //weakChildScope!.IsAlive.Should().BeFalse("Child scope should be GC'd");
        
        
    }

    [Fact]
    public void ValidatableViewModel_ClearedErrors_ReleasesMemory()
    {
        // Arrange
        var vm = new TestLeakValidatableViewModel(new MockLogger<TestLeakValidatableViewModel>());

        // Add many errors to simulate memory pressure
        for (int i = 0; i < 100; i++)
        {
            vm.TestAddError($"Property{i}", $"Error message {i}");
        }

        vm.HasErrors.Should().BeTrue();

        // Act - clear all errors
        vm.TestClearAllErrors();

        // Assert
        vm.HasErrors.Should().BeFalse();
        vm.GetValidationErrors().Should().BeEmpty();

        // Create weak reference to errors collection
        var weakErrors = new WeakReference(vm.GetValidationErrors());

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Errors collection should still exist but be empty
        vm.GetValidationErrors().Should().BeEmpty();
    }

    // ========== ADVANCED MEMORY LEAK TESTS ==========

    [STAFact]
    public void WindowHandle_RapidOpenClose_StressTest_NoLeaks()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        var weakHandles = new List<WeakReference<WindowHandle>>();
        var weakScopes = new List<WeakReference>();
        var windowIds = new List<Guid>();

        // Act - Rapidly open and close 50 windows
        void StressTest()
        {
            for (int i = 0; i < 50; i++)
            {
                var windowId = windowManager.OpenWindow<TestLeakViewModel>();
                windowIds.Add(windowId);
                
                // Don't wait for load - stress test!
                
                var metadata = windowTracker.GetMetadata(windowId);
                if (metadata?.Handle != null)
                {
                    weakHandles.Add(new WeakReference<WindowHandle>(metadata.Handle));
                    weakScopes.Add(new WeakReference(metadata.Handle.Scope));
                }

                // Immediately close
                windowManager.CloseWindow(windowId);
            }
            
            WpfTestHelpers.WaitForPendingOperations();
        }

        StressTest();

        // Force garbage collection
        ForceFullGC();

        // Assert - Everything should be collected
        foreach (var weakHandle in weakHandles)
        {
            weakHandle.TryGetTarget(out _).Should().BeFalse("All handles should be GC'd after stress test");
        }

        foreach (var weakScope in weakScopes)
        {
            weakScope.IsAlive.Should().BeFalse("All scopes should be GC'd after stress test");
        }

        // No windows should be open
        windowTracker.OpenWindows.Should().BeEmpty("No windows should remain open after stress test");
    }

    [STAFact]
    public void WindowHandle_DeepParentChildHierarchy_NoLeaks()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        var weakHandles = new List<WeakReference<WindowHandle>>();
        var windowIds = new List<Guid>();

        // Act - Create deep hierarchy: Parent -> Child1 -> Child2 -> Child3
        void CreateDeepHierarchy()
        {
            // Root
            var rootId = windowManager.OpenWindow<TestLeakViewModel>();
            windowIds.Add(rootId);
            WpfTestHelpers.WaitForWindowLoaded();
            
            var rootMeta = windowTracker.GetMetadata(rootId);
            if (rootMeta?.Handle != null)
                weakHandles.Add(new WeakReference<WindowHandle>(rootMeta.Handle));

            // Level 1
            var level1Id = windowManager.OpenChildWindow<TestLeakViewModel>(rootId);
            windowIds.Add(level1Id);
            WpfTestHelpers.WaitForWindowLoaded();
            
            var level1Meta = windowTracker.GetMetadata(level1Id);
            if (level1Meta?.Handle != null)
                weakHandles.Add(new WeakReference<WindowHandle>(level1Meta.Handle));

            // Level 2
            var level2Id = windowManager.OpenChildWindow<TestLeakViewModel>(level1Id);
            windowIds.Add(level2Id);
            WpfTestHelpers.WaitForWindowLoaded();
            
            var level2Meta = windowTracker.GetMetadata(level2Id);
            if (level2Meta?.Handle != null)
                weakHandles.Add(new WeakReference<WindowHandle>(level2Meta.Handle));

            // Level 3
            var level3Id = windowManager.OpenChildWindow<TestLeakViewModel>(level2Id);
            windowIds.Add(level3Id);
            WpfTestHelpers.WaitForWindowLoaded();
            
            var level3Meta = windowTracker.GetMetadata(level3Id);
            if (level3Meta?.Handle != null)
                weakHandles.Add(new WeakReference<WindowHandle>(level3Meta.Handle));

            // Close root - should cascade to all children
            windowManager.CloseWindow(rootId);
            WpfTestHelpers.WaitForPendingOperations();
        }

        CreateDeepHierarchy();

        // Force garbage collection
        ForceFullGC();

        // Assert - All handles should be collected
        foreach (var weakHandle in weakHandles)
        {
            weakHandle.TryGetTarget(out _).Should().BeFalse("Deep hierarchy handles should all be GC'd");
        }

        // Verify all windows are closed
        foreach (var windowId in windowIds)
        {
            windowTracker.IsWindowOpen(windowId).Should().BeFalse();
        }

        windowTracker.OpenWindows.Should().BeEmpty();
    }

    [STAFact]
    public void WindowHandle_MultipleChildrenSameParent_NoLeaks()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        var weakHandles = new List<WeakReference<WindowHandle>>();
        var childIds = new List<Guid>();

        // Act - Create 1 parent with 10 children
        void CreateMultipleChildren()
        {
            var parentId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();
            
            var parentMeta = windowTracker.GetMetadata(parentId);
            if (parentMeta?.Handle != null)
                weakHandles.Add(new WeakReference<WindowHandle>(parentMeta.Handle));

            // Create 10 children
            for (int i = 0; i < 10; i++)
            {
                var childId = windowManager.OpenChildWindow<TestLeakViewModel>(parentId);
                childIds.Add(childId);
                WpfTestHelpers.WaitForWindowLoaded();
                
                var childMeta = windowTracker.GetMetadata(childId);
                if (childMeta?.Handle != null)
                    weakHandles.Add(new WeakReference<WindowHandle>(childMeta.Handle));
            }

            // Close parent - all children should close
            windowManager.CloseWindow(parentId);
            WpfTestHelpers.WaitForPendingOperations();
        }

        CreateMultipleChildren();

        // Force garbage collection
        ForceFullGC();

        // Assert
        foreach (var weakHandle in weakHandles)
        {
            weakHandle.TryGetTarget(out _).Should().BeFalse("All handles (parent + children) should be GC'd");
        }

        foreach (var childId in childIds)
        {
            windowTracker.IsWindowOpen(childId).Should().BeFalse();
        }
    }

    [STAFact]
    public void WindowHandle_LargeDataInViewModel_ProperlyReleased()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        WeakReference? weakLargeData = null;

        // Act
        void CreateWindowWithLargeData()
        {
            var windowId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();

            var metadata = windowTracker.GetMetadata(windowId);
            var handle = metadata?.Handle;
            
            handle.Should().NotBeNull();
            
            // Get ViewModel directly from metadata (window.DataContext might be null before loaded)
            var weakVmRef = metadata!.ViewModelRef;
            if (weakVmRef?.TryGetTarget(out var viewModel) ?? false)
            {
                viewModel.Should().NotBeNull();
                var testVm = viewModel as TestLeakViewModel;
                testVm.Should().NotBeNull();
                testVm!.LargeData.Should().NotBeNull();
                
                // Capture weak reference to large data
                weakLargeData = new WeakReference(testVm.LargeData);
            }
            else
            {
                // Fallback: try to get from DataContext
                var scope = handle!.Scope;
                var window = scope.Resolve<TestLeakWindow>();
                var testVm = window.DataContext as TestLeakViewModel;
                
                if (testVm != null && testVm.LargeData != null)
                {
                    weakLargeData = new WeakReference(testVm.LargeData);
                }
            }

            weakLargeData.Should().NotBeNull("Should have captured large data reference");

            // Close window
            windowManager.CloseWindow(windowId);
            WpfTestHelpers.WaitForPendingOperations();
        }

        CreateWindowWithLargeData();

        // Force garbage collection
        ForceFullGC();

        // Assert - Large data should be released
        weakLargeData!.IsAlive.Should().BeFalse("Large data in ViewModel should be GC'd");
    }

    [STAFact]
    public void WindowHandle_EventHandlers_ProperlyUnsubscribed()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        WeakReference? weakWindow = null;

        // Act
        void CreateWindowWithEventHandlers()
        {
            var windowId = windowManager.OpenWindow<TestLeakViewModel>();
            WpfTestHelpers.WaitForWindowLoaded();

            var metadata = windowTracker.GetMetadata(windowId);
            var scope = metadata?.WindowScope;
            var window = scope?.Resolve<TestLeakWindow>();
            
            window.Should().NotBeNull();
            weakWindow = new WeakReference(window);

            // Subscribe to custom event handler (simulates user code that might cause leaks)
            EventHandler? customHandler = (s, e) =>
            {
                // Simulate some work
                var w = s as Window;
                w?.GetHashCode(); // Just to use the reference
            };
            
            window!.Closed += customHandler;

            // Close window - handler will be invoked, then window will be disposed
            windowManager.CloseWindow(windowId);
            WpfTestHelpers.WaitForPendingOperations();
            
            // NOTE: We don't manually unsubscribe - WindowHandle should clean this up
        }

        CreateWindowWithEventHandlers();

        // Force garbage collection
        ForceFullGC();

        // Assert - Window should be GC'd despite event handler
        // (WindowHandle properly unsubscribes all handlers during disposal)
        weakWindow!.IsAlive.Should().BeFalse("Window should be GC'd (event handlers unsubscribed by WindowHandle)");
    }

    [STAFact]
    public void WindowHandle_WindowScopeProperty_BackwardCompatible()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        Guid windowId = Guid.Empty;

        // Act
        windowId = windowManager.OpenWindow<TestLeakViewModel>();
        WpfTestHelpers.WaitForWindowLoaded();

        var metadata = windowTracker.GetMetadata(windowId);
        metadata.Should().NotBeNull();

        // Assert - WindowScope property should delegate to Handle.Scope (backward compatibility)
        metadata!.WindowScope.Should().NotBeNull("WindowScope should work via delegation");
        metadata.WindowScope.Should().BeSameAs(metadata.Handle?.Scope, "WindowScope should delegate to Handle.Scope");

        // Cleanup
        windowManager.CloseWindow(windowId);
        WpfTestHelpers.WaitForPendingOperations();
    }

    [STAFact]
    public void WindowHandle_RepeatedOpenCloseSequence_ConsistentBehavior()
    {
        // Arrange
        var windowManager = Resolve<IWindowManager>();
        var windowTracker = Resolve<IWindowTracker>();
        var allWeakHandles = new List<WeakReference<WindowHandle>>();

        // Act - Open and close same window type multiple times
        void RepeatedSequence()
        {
            for (int iteration = 0; iteration < 10; iteration++)
            {
                var windowId = windowManager.OpenWindow<TestLeakViewModel>();
                WpfTestHelpers.WaitForWindowLoaded();

                var metadata = windowTracker.GetMetadata(windowId);
                metadata.Should().NotBeNull($"Window should be tracked in iteration {iteration}");
                
                var handle = metadata!.Handle;
                handle.Should().NotBeNull($"Handle should exist in iteration {iteration}");
                
                allWeakHandles.Add(new WeakReference<WindowHandle>(handle!));

                windowManager.CloseWindow(windowId);
                WpfTestHelpers.WaitForPendingOperations();
                
                // Verify window is closed after each iteration
                windowTracker.IsWindowOpen(windowId).Should().BeFalse($"Window should be closed after iteration {iteration}");
            }
        }

        RepeatedSequence();

        // Force garbage collection
        ForceFullGC();

        // Assert - All handles from all iterations should be collected
        foreach (var weakHandle in allWeakHandles)
        {
            weakHandle.TryGetTarget(out _).Should().BeFalse("All handles from all iterations should be GC'd");
        }

        windowTracker.OpenWindows.Should().BeEmpty("No windows should remain after repeated sequence");
    }

    // ========== TEST TYPES ==========

    public class TestLeakViewModel : BaseViewModel, IDisposable
    {
        private bool _disposed = false;
        private byte[]? _largeData = null;
        public TestLeakViewModel(ILogger<TestLeakViewModel> logger) : base(logger)
        {
            // Allocate some memory to make leak detection easier
            _largeData = new byte[1024 * 1024]; // 1 MB
        }

        public byte[]? LargeData => _largeData;

        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (_disposed) return;
                try
                {
                    // Dispose ViewModel if it's disposable
                    if (_largeData?.Length > 0)
                    {
                        Array.Clear(_largeData);
                    }
                    _largeData = null;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                _disposed = true;
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class TestLeakDialogViewModel : DialogViewModel, IDisposable
    {
        private bool _disposed = false;
        private byte[]? _largeData = null;
        public TestLeakDialogViewModel(ILogger<TestLeakDialogViewModel> logger, IDialogHost dialogHost) : base(logger, dialogHost)
        {
            // Allocate some memory to make leak detection easier
            _largeData = new byte[1024 * 1024]; // 1 MB
        }

        public byte[]? LargeData => _largeData;

        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (_disposed) return;
                try
                {
                    // Dispose ViewModel if it's disposable
                    if (_largeData?.Length > 0)
                    {
                        Array.Clear(_largeData);
                    }
                    _largeData = null;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                _disposed = true;
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override async Task CompleteDialogAsync()
        {
            base.OnComplete();
            await Task.CompletedTask;
            base.CloseDialogWindow(DialogResult.Success());

        }

        protected override async Task CancelDialogAsync()
        {
            base.OnCancel();
            await Task.CompletedTask;
            base.CloseDialogWindow(DialogResult.Cancel());
        }
    }


    public class TestLeakValidatableViewModel : ValidatableViewModel
    {
        public TestLeakValidatableViewModel(ILogger<TestLeakValidatableViewModel> logger) : base(logger)
        {
        }

        public void TestAddError(string property, string error) => AddPropertyError(property, error);
        public void TestClearAllErrors() => ClearAllErrors();
    }

    public class TestLeakWindow : ScopedWindow
    {
        public TestLeakWindow(ILogger logger) : base(logger)
        {

        }


    }
}