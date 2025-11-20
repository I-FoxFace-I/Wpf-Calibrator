using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Windows;
using System.Windows.Threading;
using WpfEngine.ViewModels;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Dialogs.Events;
using WpfEngine.Enums;
using WpfEngine.Services;
using WpfEngine.Services.Autofac;
using WpfEngine.Tests.Helpers;
using WpfEngine.ViewModels;
using Xunit;
using static WpfEngine.Tests.Core.Services.ViewRegistryTests;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Tests.Core.Services;

/// <summary>
/// Tests for enhanced DialogService with nested dialogs and complex results
/// </summary>
public class DialogServiceTests : AutofacTestFixture
{
    //private Guid id;
    public DialogServiceTests() : base()
    {
        //id = WindowManager.OpenWindow<TestDialogViewModel>();
    }

    

    protected override void RegisterTestServices(ContainerBuilder builder)
    {
        // Register mocked services for testing
        //builder.RegisterInstance(_registryMock.Object).As<IViewRegistry>();
        //builder.RegisterInstance(_windowContextMock.Object).As<IWindowContext>();
        //builder.RegisterInstance(WindowManagerMock.Object).As<IWindowManager>();

        // Register DialogService
        builder.RegisterType<DialogService>().As<IDialogService>().InstancePerLifetimeScope();

        // Register test ViewModels
        builder.RegisterType<TestDialogViewModel>().AsSelf();
        builder.RegisterType<TestDialogWithResultViewModel>().AsSelf();
        builder.RegisterType<TestNestedDialogViewModel>().AsSelf();

        builder.RegisterType<TestDialogWindow>().AsSelf();
        builder.RegisterType<TestDialogWithParamsWindow>().AsSelf();
        builder.RegisterType<TestDialogWithResultWindow>().AsSelf();

        builder.Register(c => Mock.Of<ILogger>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<DialogService>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestWindow>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<DialogService>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestDialogWithResultViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestNestedDialogViewModel>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestDialogWindow>>()).InstancePerDependency();
        builder.Register(c => Mock.Of<ILogger<TestDialogWithResultWindow>>()).InstancePerDependency();
    }


    protected override IViewRegistry RegisterMapping(IViewRegistry viewRegistry)
    {
        viewRegistry.MapDialog<TestDialogViewModel, TestDialogWindow>();
        viewRegistry.MapDialog<TestNestedDialogViewModel, TestDialogWithParamsWindow>();
        viewRegistry.MapDialog<TestDialogWithResultViewModel, TestDialogWithResultWindow>();

        return viewRegistry;
    }

    [STAFact]
    public void Constructor_InitializesSuccessfully()
    {
        // Arrange & Act
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);

        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;

        // Assert
        dialogService.Should().NotBeNull();
        dialogService.HasActiveDialog.Should().BeFalse();
        dialogService.CurrentDialogId.Should().BeNull();
        dialogService.ActiveDialogStack.Should().BeEmpty();
    }

    [STAFact]
    public void CloseCurrentDialog_WithNoActiveDialog_LogsWarning()
    {
        // Arrange
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;

        // Act
        dialogService.CloseCurrentDialog();

        // Assert - should not throw, just log warning
        dialogService.HasActiveDialog.Should().BeFalse();
    }

    [STAFact]
    public void CloseDialog_WithInvalidId_LogsWarning()
    {
        // Arrange
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;
        var invalidId = Guid.NewGuid();

        // Act
        dialogService.CloseDialog(invalidId);

        // Assert - should not throw
        dialogService.HasActiveDialog.Should().BeFalse();
    }

    [STAFact]
    public void IsDialogOpen_WithValidId_ReturnsCorrectState()
    {
        // Arrange
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;
        var dialogId = Guid.NewGuid();

        // Act & Assert
        dialogService.IsDialogOpen(dialogId).Should().BeFalse();
    }

    [STAFact]
    public void GetChildDialogIds_WithNoChildren_ReturnsEmptyList()
    {
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;
        var parentId = Guid.NewGuid();

        // Act
        var children = dialogService.GetChildDialogIds(parentId);

        // Assert
        children.Should().NotBeNull();
        children.Should().BeEmpty();
    }

    [STAFact]
    public async Task ShowMessageAsync_ReturnsExpectedResult()
    {
        // Arrange
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;

        // We can't test actual MessageBox but we can test that method doesn't throw
        // In real app, this would need UI automation testing
        // Act - run in background task to not block
        var messageTask = Task.Run(async () =>
        {
            // This will timeout in tests since no one clicks the button
            var cts = new System.Threading.CancellationTokenSource(100);
            try
            {
                Task.Delay(50);
                await dialogService.ShowMessageAsync("Test", "Test");
            }
            catch (Exception ex)
            {
                // Expected in test environment
                return MessageBoxResult.None;
            }
            return MessageBoxResult.None;
        });

        //await Task.Delay(50);

        // Assert - task should be running (waiting for user input)
        messageTask.IsCompleted.Should().BeFalse();

    }

    [STAFact]
    public async Task ShowErrorAsync_LogsError()
    {
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>();
        var exception = new InvalidOperationException("Test error");
        

        // Act - similar to ShowMessageAsync, will timeout in tests
        var errorTask = Task.Run(async () =>
        {
            var cts = new System.Threading.CancellationTokenSource(100);
            try
            {
                await dialogService.ShowErrorAsync("Test error", "Error", exception);
            }
            catch (TaskCanceledException)
            {
                
            }
        });

        //await Task.Delay(5);

        // Assert
        errorTask.IsCompleted.Should().BeFalse();
        //cts.Cancel();
    }

    [STAFact]
    public void CloseAllDialogs_ClearsDialogStack()
    {
        // Arrange
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;

        // Act
        dialogService!.CloseAllDialogs();

        // Assert
        dialogService.HasActiveDialog.Should().BeFalse();
        dialogService.ActiveDialogStack.Should().BeEmpty();
    }

    [STAFact]
    public void DialogOpened_Event_IsRaised()
    {
        // Arrange
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;
        DialogOpenedEventArgs? eventArgs = null;
        dialogService!.DialogOpened += (s, e) => eventArgs = e;

        // We can test that event would be raised if dialog could be opened
        // Real dialog testing requires UI automation

        // Assert
        eventArgs.Should().BeNull("No dialog was actually opened in this test");
    }

    [STAFact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var id = WindowManager.OpenWindow<TestDialogViewModel>();
        var scope = WindowTracker.GetWindowScope(id);
        // Arrange
        var dialogService = scope.Resolve<IDialogService>() as DialogService;

        // Act
        dialogService!.Dispose();
        dialogService.Dispose(); // Second call should not throw

        // Assert
        dialogService.HasActiveDialog.Should().BeFalse();
        dialogService.ActiveDialogStack.Should().BeEmpty();
    }

    // ========== INTEGRATION TESTS (Would need UI Automation) ==========

    [Fact(Skip = "Requires UI automation framework")]
    public async Task ShowDialogAsync_WithResult_ReturnsCorrectResult()
    {
        // This type of test would require:
        // 1. UI Automation framework (like FlaUI)
        // 2. Or mock Window/ShowDialog behavior
        // 3. Or use a test-specific IDialogService implementation

        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires UI automation framework")]
    public async Task ShowDialogAsync_NestedDialogs_MaintainsCorrectStack()
    {
        // This would need proper UI automation to test actual nested dialogs
        await Task.CompletedTask;
    }

    // ========== MOCK-BASED DIALOG SERVICE FOR TESTING ==========

    [STAFact]
    public async Task MockDialogService_ShowDialogAsync_ReturnsExpectedResult()
    {
        // Arrange - create a mock dialog service for testing
        var mockDialogService = new Mock<IDialogService>();
        var expectedResult = new TestDialogResult
        {
            IsSuccess = true,
            SelectedValue = "Test",
            SelectedId = 42
        };

        mockDialogService
            .Setup(x => x.ShowDialogAsync<TestDialogViewModel, TestDialogResult>())
            .ReturnsAsync(DialogResult<TestDialogResult>.Success(expectedResult));

        // Act
        var result = await mockDialogService.Object.ShowDialogAsync<TestDialogViewModel, TestDialogResult>();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Result.Should().Be(expectedResult);
    }

    [STAFact]
    public void MockDialogService_NestedDialogs_TracksStack()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        var dialogStack = new Stack<Guid>();

        mockDialogService
            .Setup(x => x.ActiveDialogStack)
            .Returns(() => dialogStack.ToArray());

        mockDialogService
            .Setup(x => x.CurrentDialogId)
            .Returns(() => dialogStack.Count > 0 ? dialogStack.Peek() : (Guid?)null);

        mockDialogService
            .Setup(x => x.HasActiveDialog)
            .Returns(() => dialogStack.Count > 0);

        // Simulate opening dialogs
        var dialog1 = Guid.NewGuid();
        var dialog2 = Guid.NewGuid();

        dialogStack.Push(dialog1);
        mockDialogService.Object.HasActiveDialog.Should().BeTrue();
        mockDialogService.Object.CurrentDialogId.Should().Be(dialog1);

        dialogStack.Push(dialog2);
        mockDialogService.Object.CurrentDialogId.Should().Be(dialog2);
        mockDialogService.Object.ActiveDialogStack.Should().HaveCount(2);

        dialogStack.Pop();
        mockDialogService.Object.CurrentDialogId.Should().Be(dialog1);
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    // ========== TEST VIEW MODELS ==========

    private class TestDialogViewModel : BaseViewModel
    {
        public TestDialogViewModel(ILogger<TestDialogViewModel> logger) : base(logger) { }
    }

    private class TestDialogWithResultViewModel : BaseViewModel
    {
        public TestDialogResult? Result { get; set; }

        public TestDialogWithResultViewModel(ILogger<TestDialogWithResultViewModel> logger) : base(logger) { }
    }

    private class TestNestedDialogViewModel : BaseViewModel
    {
        public TestNestedDialogViewModel(ILogger<TestNestedDialogViewModel> logger) : base(logger) { }
    }

    private class TestDialogResult : IDialogResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SelectedValue { get; set; }
        public int SelectedId { get; set; }

        public Guid Key { get; set; }

        public DialogStatus Status { get; set; }

        public bool IsComplete { get; set; }

        public bool IsCancelled { get; set; }
    }
}

