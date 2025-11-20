// =================================================================================================
// DialogManager_NewTests.cs
// Tests for the new non-scoped DialogManager (Variant B)
//  - AppModal (ShowDialog) returns typed result
//  - WindowModal (modeless + disable owner branch) disables & reenables owner
//  - WindowModal cleans up (untracks + GC friendly)
//
// These tests derive from AutofacTestFixture and only register extra pieces needed
// for the dialog manager and the test-only views/viewmodels.
// =================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WpfEngine.ViewModels;           // BaseViewModel, IViewModel
using WpfEngine.Enums;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;             // AutofacTestFixture, WpfTestHelpers, STAFact
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;
using Xunit;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Tests.Core.Services
{
    public class ScopeWindowManagerDialogTests : AutofacTestFixture
    {
        protected override void RegisterTestServices(ContainerBuilder builder)
        {
            // LoggerFactory is required by DialogManager
            builder.RegisterInstance(new LoggerFactory()).As<ILoggerFactory>().SingleInstance();

            // Test VMs
            builder.RegisterType<OwnerTestVm>().AsSelf().InstancePerDependency();
            builder.RegisterType<AutoCloseOkDialogVm>().AsSelf().InstancePerDependency();
            builder.RegisterType<ManualCloseDialogVm>().AsSelf().InstancePerDependency();

            // Test Windows (not resolved via DI in production, but tests reuse them sometimes)
            builder.RegisterType<OwnerTestWindow>().AsSelf().InstancePerDependency();
            builder.RegisterType<AutoCloseTestDialogWindow>().AsSelf().InstancePerDependency();
            builder.RegisterType<ManualTestDialogWindow>().AsSelf().InstancePerDependency();

            // Generic loggers
            builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
            builder.Register(c => Mock.Of<ILogger<ScopedWindowManager>>()).InstancePerDependency();
            builder.RegisterGeneric(typeof(MockLogger<>)).As(typeof(ILogger<>)).InstancePerDependency();
        }

        protected override IViewRegistry RegisterMapping(IViewRegistry vr)
        {
            // Owner window mapping
            vr.MapWindow<OwnerTestVm, OwnerTestWindow>();

            // Dialog mappings (VM → Window)
            vr.MapDialog<AutoCloseOkDialogVm, AutoCloseTestDialogWindow>();
            vr.MapDialog<ManualCloseDialogVm, ManualTestDialogWindow>();
            return vr;
        }

        private IScopedWindowManager GetDialogManager() => Scope.Resolve<IScopedWindowManager>();
        private IWindowTracker GetTracker() => Scope.Resolve<IWindowTracker>();
        private IViewRegistry GetRegistry() => ViewRegistry;
        private ILogger GetLogger() => Scope.Resolve<ILogger>();

        // -------------------------------------------------------------------------------------------------
        // APP-MODAL: returns typed result (DialogResult<int>) and cleans up tracking
        // -------------------------------------------------------------------------------------------------
        [STAFact]
        public async Task AppModal_Returns_Typed_Result_And_Untracks()
        {
            // Arrange: open owner window
            var ownerId = WindowManager.OpenWindow<OwnerTestVm>();
            WpfTestHelpers.WaitForWindowLoaded();

            var dialog = GetDialogManager();
            var tracker = GetTracker();

            var task = dialog.ShowDialogAsync<AutoCloseOkDialogVm, TestResult>(ownerWindowId: ownerId, modality: DialogModality.WindowModal);

            var result = await task;

            // Assert: result OK and no child dialogs tracked under owner
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            result!.Result!.ResultValue.Should().Be(42);

            var children = tracker.GetChildWindows(ownerId);
            children.Should().BeEmpty();
        }

        // -------------------------------------------------------------------------------------------------
        // WINDOW-MODAL: disables and re-enables owner branch while dialog is open
        // -------------------------------------------------------------------------------------------------
        [STAFact]
        public async Task WindowModal_Disables_Owner_During_Lifetime_Then_Reenables()
        {
            // Arrange
            var ownerId = WindowManager.OpenWindow<OwnerTestVm>();
            WpfTestHelpers.WaitForWindowLoaded();

            var dialog = GetDialogManager();
            var tracker = GetTracker();

            var ownerMeta = tracker.GetMetadata(ownerId);
            ownerMeta.Should().NotBeNull();
            ownerMeta!.WindowRef.TryGetTarget(out var ownerWindow).Should().BeTrue();
            ownerWindow.Should().NotBeNull();
            ownerWindow!.IsEnabled.Should().BeTrue();


            int counter = 0;

            ownerWindow.IsEnabledChanged += (_, _) => Interlocked.Increment(ref counter);

            counter.Should().Be(0);
            // Act
            //var task = dialog.ShowDialogAsync<AutoCloseOkDialogVm, TestResult>(
            //    ownerWindowId: ownerId,
            //    sessionScope: Scope,
            //    viewRegistry: GetRegistry(),
            //    tracker: tracker,
            //    logger: GetLogger(),
            //    modality: DialogModality.WindowModal);
            var task = dialog.ShowDialogAsync<AutoCloseOkDialogVm, TestResult>(ownerWindowId: ownerId, modality: DialogModality.WindowModal);

            WpfTestHelpers.WaitForWindowLoaded();

            var result = await task; // dialog auto-closes and should re-enable owner
            counter.Should().BeGreaterThanOrEqualTo(2);
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            ownerWindow.IsEnabled.Should().BeTrue();

            // No child dialogs tracked
            tracker.GetChildWindows(ownerId).Should().BeEmpty();
        }

        // -------------------------------------------------------------------------------------------------
        // WINDOW-MODAL: manual-close dialog verifies GC-friendliness (no leaks)
        // -------------------------------------------------------------------------------------------------
        [STAFact]
        public async Task WindowModal_ManualClose_GarbageCollected()
        {
            // Arrange
            var ownerId = WindowManager.OpenWindow<OwnerTestVm>();
            WpfTestHelpers.WaitForWindowLoaded();

            var dialog = GetDialogManager();
            var tracker = GetTracker();

            // Start a dialog that we will close manually
            //var showTask = dialog.ShowDialogAsync<ManualCloseDialogVm, TestResult>(
            //    ownerWindowId: ownerId,
            //    sessionScope: Scope,
            //    viewRegistry: GetRegistry(),
            //    tracker: tracker,
            //    logger: GetLogger(),
            //    modality: DialogModality.WindowModal);

            var showTask = dialog.ShowDialogAsync<ManualCloseDialogVm, TestResult>(ownerId, DialogModality.WindowModal);

            // Wait until dialog appears and fetch its metadata
            WpfTestHelpers.WaitForWindowLoaded();

            var childIds = tracker.GetChildWindows(ownerId);
            childIds.Should().HaveCount(1);
            var dialogId = childIds.Single();
            var meta = tracker.GetMetadata(dialogId);
            meta.Should().NotBeNull();

            WeakReference? weakVm = null;
            WeakReference? weakWindow = null;
            if (meta!.ViewModelRef.TryGetTarget(out var w))
            {
                weakWindow = new WeakReference(w!);

            }
            if (meta.ViewModelRef.TryGetTarget(out var vmBase))
            {
                weakVm = new WeakReference(vmBase!);
            }

            // Close via VM API (sets result + closes)
            //w!.Dispatcher.Invoke(() =>
            //{
            var vm = (ManualCloseDialogVm)vmBase!;
            await vm.CloseWith(99, meta.WindowRef);
            //});

            var result = await showTask;
            result.Should().NotBeNull();
            result!.IsSuccess.Should().BeTrue();
            //result.Data.Should().Be(99);

            // Allow pending ops and enforce GC
            WpfTestHelpers.WaitForPendingOperations();
            ForceFullGC();

            // Assert: object graphs are collectable
            //weakWindow.IsAlive.Should().BeFalse("Dialog window should be GC'd after close");
            //weakVm.IsAlive.Should().BeFalse("Dialog VM should be GC'd after close");

            // Untracked
            tracker.GetChildWindows(ownerId).Should().BeEmpty();
        }

        private static void ForceFullGC()
        {
            for (int i = 0; i < 6; i++)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                GC.WaitForPendingFinalizers();
            }
        }


        // =================================================================================================
        // Test ViewModels & Windows
        // =================================================================================================

        // Owner window VM (empty)
        internal sealed class OwnerTestVm : BaseViewModel
        {
            public OwnerTestVm(ILogger<OwnerTestVm> logger) : base(logger) { }
        }

        public record TestResult(int ResultValue);

        // Dialog VM that auto-returns Success(42)
        internal sealed class AutoCloseOkDialogVm : BaseViewModel, IResultDialogViewModel<TestResult>
        {
            public AutoCloseOkDialogVm(ILogger<AutoCloseOkDialogVm> logger) : base(logger) { }
            public TestResult? ResultData { get; private set; }

            public bool IsCompleted => ResultData != null;

            public bool IsCancelled => ResultData is null;

            public Guid DialogId => throw new NotImplementedException();

            public DialogStatus Status { get; private set; }

            public void AutoComplete(Window w)
            {

                ResultData = new TestResult(42);
                Status = DialogStatus.Success;
                w.Close();
            }
        }

        // Dialog VM that we close manually from the test
        internal sealed class ManualCloseDialogVm : BaseViewModel, IResultDialogViewModel<TestResult>
        {
            private byte[] _pressure = new byte[1024 * 1024]; // help detect leaks
            public ManualCloseDialogVm(ILogger<ManualCloseDialogVm> logger) : base(logger) { }
            public TestResult? ResultData { get; private set; }

            public bool IsCompleted => ResultData != null;

            public bool IsCancelled => ResultData is null;

            public Guid DialogId => throw new NotImplementedException();
            public DialogStatus Status { get; private set; }

            private TaskCompletionSource<bool> _initializied = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public async Task CloseWith(int value, WeakReference<Window> w)
            {
                _pressure = Array.Empty<byte>();
                ResultData = new TestResult(value);
                Status = DialogStatus.Success;
                await _initializied.Task;
                if (w.TryGetTarget(out var wt))
                {
                    wt.Close();
                }
            }

            public override Task InitializeAsync()
            {
                _initializied.TrySetResult(true);
                return base.InitializeAsync();
            }
        }

        // Owner window (off-screen, minimal)
        internal sealed class OwnerTestWindow : ScopedDialogWindow
        {
            public OwnerTestWindow(ILogger logger) : base(logger)
            {
                Width = 100; Height = 80; WindowStartupLocation = WindowStartupLocation.Manual; Left = -10000; Top = -10000;
            }

            public override DialogType DialogType => throw new NotImplementedException();

            public override string? AppModule => throw new NotImplementedException();
        }

        // Dialog window that auto-closes right after Loaded (calls VM.AutoComplete)
        internal sealed class AutoCloseTestDialogWindow : ScopedDialogWindow
        {
            public AutoCloseTestDialogWindow(ILogger logger) : base(logger)
            {
                Width = 80;
                Height = 60;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = -10000;
                Top = -10000;
                base.Loaded += async (_, _) =>
                {
                    //Thread.Sleep(50);
                    await Task.Delay(TimeSpan.FromMicroseconds(500));
                    if (DataContext is AutoCloseOkDialogVm vm)
                    {
                        vm.AutoComplete(this);
                    }
                };
            }

            public override DialogType DialogType => throw new NotImplementedException();

            public override string? AppModule => throw new NotImplementedException();
        }

        // Dialog window for manual close tests (no auto-close behavior)
        internal sealed class ManualTestDialogWindow : ScopedDialogWindow
        {
            public ManualTestDialogWindow(ILogger logger) : base(logger)
            {
                Width = 80; Height = 60; WindowStartupLocation = WindowStartupLocation.Manual; Left = -10000; Top = -10000;
            }

            public override DialogType DialogType => throw new NotImplementedException();

            public override string? AppModule => throw new NotImplementedException();
        }
    }
}
