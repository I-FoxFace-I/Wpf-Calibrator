using Autofac;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfEngine.Abstract;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Dialogs.Events;
using WpfEngine.Services;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Enhanced dialog service with proper window ownership and scope management
/// </summary>
public class DialogService : IDialogService, IDisposable
{
    private readonly IWindowTracker _tracker;
    private readonly ILifetimeScope _scope;
    private readonly IViewRegistry _registry;
    private readonly IWindowIdentity _ownerIdentity;
    private readonly ILogger<DialogService> _logger;

    // Owner window tracking
    private readonly Guid _ownerWindowId;

    // Dialog tracking
    private readonly Stack<DialogInfo> _dialogStack = new();
    private readonly Dictionary<Guid, DialogInfo> _activeDialogs = new();
    private readonly Dictionary<Guid, List<Guid>> _childDialogs = new();
    private bool _disposed;

    public DialogService(
        ILifetimeScope ownerScope,                       // window scope where this service lives
        IViewRegistry registry,
        IWindowTracker tracker,
        IWindowIdentity windowIdentity,                  // injected from window scope (no UI in DI)
        ILogger<DialogService> logger)
    {
        _scope = ownerScope ?? throw new ArgumentNullException(nameof(ownerScope));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        _ownerIdentity = windowIdentity ?? throw new ArgumentNullException(nameof(windowIdentity));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ownerWindowId = _ownerIdentity.WindowId;
        _logger.LogInformation("[DIALOG_SERVICE] Created for window {WindowId}", _ownerIdentity.WindowId);
    }

    // ========== EVENTS ==========

    public event EventHandler<DialogOpenedEventArgs>? DialogOpened;
    public event EventHandler<DialogClosedEventArgs>? DialogClosed;
    public event EventHandler<ChildDialogClosedEventArgs>? ChildDialogClosed;

    // ========== STATE ==========

    public IReadOnlyList<Guid> ActiveDialogStack => _dialogStack.Select(d => d.DialogId).ToList();
    public Guid? CurrentDialogId => _dialogStack.Count > 0 ? _dialogStack.Peek().DialogId : null;
    public bool HasActiveDialog => _dialogStack.Count > 0;

    // ========== SHOW DIALOG METHODS ==========

    public async Task<DialogResult> ShowDialogAsync<TViewModel>()
        where TViewModel : IViewModel
    {
        return await ShowDialogInternalAsync<TViewModel, DialogResult>(null);
    }

    public async Task<DialogResult> ShowDialogAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {

        return await ShowDialogInternalAsync<TViewModel, DialogResult>(parameters);
    }

    public async Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : IViewModel
        where TResult : class, IDialogResult
    {
        return await ShowDialogInternalAsync<TViewModel, DialogResult<TResult>>(null);
    }

    public async Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TParameters, TResult>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
        where TResult : class, IDialogResult
    {
        return await ShowDialogInternalAsync<TViewModel, DialogResult<TResult>>(parameters);
    }

    // ========== MESSAGE BOX DIALOGS ==========

    public async Task<MessageBoxResult> ShowMessageAsync(
        string message,
        string title = "Message",
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None)
    {
        try
        {
            var owner = GetOwnerWindow();
            var wpfButtons = ConvertToWpfButton(buttons);
            var wpfIcon = ConvertToWpfImage(icon);

            if (Application.Current?.Dispatcher is Dispatcher disp)
            {
                var result = await disp.InvokeAsync(() =>
                {
                    if (owner != null)
                    {
                        return MessageBox.Show(owner, message, title, wpfButtons, wpfIcon);
                    }
                    return MessageBox.Show(message, title, wpfButtons, wpfIcon);
                });

                return ConvertFromWpfResult(result);
            }
            else
            {
                if (owner != null)
                {
                    return MessageBox.Show(owner, message, title, wpfButtons, wpfIcon);
                }
                return MessageBox.Show(message, title, wpfButtons, wpfIcon);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DIALOG_SERVICE] Failed to show message box");
            return MessageBoxResult.None;
        }
    }

    public async Task ShowErrorAsync(string message, string title = "Error", Exception? exception = null)
    {
        var fullMessage = exception != null
            ? $"{message}\n\nDetails:\n{exception.Message}"
            : message;

        if (exception != null)
        {
            _logger.LogError(exception, "[DIALOG_SERVICE] Error dialog: {Message}", message);
        }

        await ShowMessageAsync(fullMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public async Task<bool> ShowConfirmationAsync(string message, string title = "Confirm")
    {
        var result = await ShowMessageAsync(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    // ========== DIALOG MANAGEMENT ==========

    public void CloseDialog(Guid dialogId, IDialogResult? result = null)
    {
        if (!_activeDialogs.TryGetValue(dialogId, out var dialogInfo))
        {
            _logger.LogWarning("[DIALOG_SERVICE] Attempted to close unknown dialog {DialogId}", dialogId);
            return;
        }

        CloseDialogInternal(dialogInfo, result);
    }

    public void CloseCurrentDialog(IDialogResult? result = null)
    {
        if (_dialogStack.Count == 0)
        {
            _logger.LogWarning("[DIALOG_SERVICE] No active dialog to close");
            return;
        }

        var currentDialog = _dialogStack.Peek();
        CloseDialogInternal(currentDialog, result);
    }

    public void CloseAllDialogs()
    {
        _logger.LogInformation("[DIALOG_SERVICE] Closing all {Count} active dialogs", _dialogStack.Count);

        while (_dialogStack.Count > 0)
        {
            var dialog = _dialogStack.Peek();
            CloseDialogInternal(dialog, DialogResult<object>.Cancel());
        }
    }

    public IReadOnlyList<Guid> GetChildDialogIds(Guid parentDialogId)
    {
        return _childDialogs.TryGetValue(parentDialogId, out var children)
            ? children.ToList()
            : new List<Guid>();
    }

    public bool IsDialogOpen(Guid dialogId)
    {
        return _activeDialogs.ContainsKey(dialogId);
    }

    // ========== PRIVATE METHODS ==========
    private async Task<TResult> ShowDialogInternalAsync<TViewModel, TResult>(IViewModelParameters? parameters)
        where TViewModel : IViewModel
        where TResult : IDialogResult
    {
        var dialogId = Guid.NewGuid();
        var parentId = CurrentDialogId;

        try
        {
            _logger.LogInformation("[DIALOG_SERVICE] Opening dialog {DialogId} of type {ViewModelType}",
                dialogId, typeof(TViewModel).Name);

            // Resolve ViewModel from current scope (not creating new scope)
            IViewModel viewModel;
            if (parameters != null)
            {
                viewModel = _scope.Resolve<TViewModel>(new TypedParameter(parameters.GetType(), parameters));
            }
            else
            {
                viewModel = _scope.Resolve<TViewModel>();
            }

            // Initialize ViewModel
            if (viewModel is IInitializable initializable)
            {
                await initializable.InitializeAsync();
            }

            // Get view type
            if (!_registry.TryGetViewType(typeof(TViewModel), out var viewType))
            {
                throw new InvalidOperationException($"No view registered for {typeof(TViewModel).Name}");
            }

            // Resolve view
            var view = _scope.Resolve(viewType) as Window;
            if (view == null)
            {
                throw new InvalidOperationException($"View type {viewType.Name} is not a Window");
            }

            // Set DataContext
            view.DataContext = viewModel;

            // Set owner
            view.Owner = GetOwnerWindow();
            view.WindowStartupLocation = view.Owner != null
                ? WindowStartupLocation.CenterOwner
                : WindowStartupLocation.CenterScreen;

            // Create dialog info
            var dialogInfo = new DialogInfo
            {
                DialogId = dialogId,
                ParentDialogId = parentId,
                Window = view,
                ViewModel = viewModel,
                ViewModelType = typeof(TViewModel),
                CompletionSource = new TaskCompletionSource<IDialogResult>()
            };

            // Track dialog
            _dialogStack.Push(dialogInfo);
            _activeDialogs[dialogId] = dialogInfo;

            if (parentId.HasValue)
            {
                if (!_childDialogs.ContainsKey(parentId.Value))
                    _childDialogs[parentId.Value] = new List<Guid>();
                _childDialogs[parentId.Value].Add(dialogId);
            }

            // Handle window closed
            view.Closed += (s, e) =>
            {
                if (_activeDialogs.ContainsKey(dialogId))
                {
                    // Dialog was closed by user (X button)
                    CloseDialogInternal(dialogInfo, DialogResult<object>.Cancel());
                }
            };

            // Show dialog
            _logger.LogDebug("[DIALOG_SERVICE] Showing modal dialog {DialogId}", dialogId);

            bool? dialogResult;

            void OnLoadedRaiseEvent(object s, RoutedEventArgs e)
            {
                view.Loaded -= OnLoadedRaiseEvent;
                // Raise event
                DialogOpened?.Invoke(this, new DialogOpenedEventArgs(dialogId, typeof(TViewModel), parentId));
            }

            view.Loaded += OnLoadedRaiseEvent;

            if (view.Dispatcher is Dispatcher dispatcher && !dispatcher.CheckAccess())
            {
                dialogResult = dispatcher.Invoke(() => view.ShowDialog());
            }
            else
            {
                dialogResult = view.ShowDialog();
            }

            // Wait for result
            var result = await dialogInfo.CompletionSource.Task;

            if (result is TResult typedResult)
            {
                return typedResult;
            }
            else if (typeof(TResult) == typeof(DialogResult))
            {
                // Convert bool? to SimpleDialogResult
                var flag = dialogResult ?? false;
                return (TResult)(IDialogResult)(flag
                    ? DialogResult.Success()
                    : DialogResult.Cancel());
            }
            else
            {
                throw new InvalidCastException($"Result type mismatch. Expected {typeof(TResult).Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DIALOG_SERVICE] Failed to show dialog {DialogId}", dialogId);

            // Cleanup on error
            if (_activeDialogs.ContainsKey(dialogId))
            {
                _activeDialogs.Remove(dialogId);
                _dialogStack.TryPop(out _);
            }

            // Show error dialog
            await ShowErrorAsync($"Failed to open dialog: {ex.Message}", "Dialog Error", ex);

            // Return failure result
            if (typeof(TResult) == typeof(DialogResult))
                return (TResult)(IDialogResult)DialogResult.Error(ex.Message);

            throw;
        }
    }

    private void CloseDialogInternal(DialogInfo dialogInfo, IDialogResult? result)
    {
        _logger.LogInformation("[DIALOG_SERVICE] Closing dialog {DialogId}", dialogInfo.DialogId);

        // Close all child dialogs first
        if (_childDialogs.TryGetValue(dialogInfo.DialogId, out var children))
        {
            foreach (var childId in children.ToList())
            {
                if (_activeDialogs.TryGetValue(childId, out var childInfo))
                {
                    CloseDialogInternal(childInfo, DialogResult<object>.Cancel());
                }
            }
            _childDialogs.Remove(dialogInfo.DialogId);
        }

        // Set result
        dialogInfo.CompletionSource.TrySetResult(result ?? DialogResult<object>.Cancel());

        // Close window
        void CloseWindow()
        {
            if (dialogInfo.Window.IsVisible)
            {
                dialogInfo.Window.DialogResult = result?.IsSuccess ?? false;
                dialogInfo.Window.Close();
            }
        }

        if (dialogInfo.Window.Dispatcher is Dispatcher dispatcher && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(CloseWindow);
        }
        else
        {
            CloseWindow();
        }

        // Remove from tracking
        _activeDialogs.Remove(dialogInfo.DialogId);

        // Remove from stack
        if (_dialogStack.Count > 0 && _dialogStack.Peek().DialogId == dialogInfo.DialogId)
        {
            _dialogStack.Pop();
        }
        else
        {
            // Remove from middle of stack
            var tempStack = new Stack<DialogInfo>();
            while (_dialogStack.Count > 0)
            {
                var item = _dialogStack.Pop();
                if (item.DialogId != dialogInfo.DialogId)
                    tempStack.Push(item);
            }
            while (tempStack.Count > 0)
                _dialogStack.Push(tempStack.Pop());
        }

        // Raise events
        DialogClosed?.Invoke(this, new DialogClosedEventArgs(dialogInfo.DialogId, dialogInfo.ViewModelType, result));

        if (dialogInfo.ParentDialogId.HasValue)
        {
            ChildDialogClosed?.Invoke(this, new ChildDialogClosedEventArgs(
                dialogInfo.ParentDialogId.Value,
                dialogInfo.DialogId,
                dialogInfo.ViewModelType,
                result));
        }

        // Dispose ViewModel if disposable
        if (dialogInfo.ViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private Window? GetOwnerWindow()
    {
        // Try to get current active dialog
        if (_dialogStack.Count > 0)
        {
            var topDialog = _dialogStack.Peek();
            if (topDialog.Window?.IsVisible == true)
                return topDialog.Window;
        }

        // Try to get owner window from WindowManager using WithWindow pattern
        if (_ownerWindowId != Guid.Empty)
        {
            var metadata = _tracker.GetMetadata(_ownerWindowId);
            if (metadata != null)
            {
                // Use WithWindow to safely get the window reference
                return metadata.WithWindow(
                    window => window,
                    defaultValue: null);
            }
        }

        // Fallback to main window
        return Application.Current?.MainWindow;
    }

    // ========== CONVERSION HELPERS ==========

    private MessageBoxButton ConvertToWpfButton(MessageBoxButton button)
    {
        return button switch
        {
            MessageBoxButton.OK => MessageBoxButton.OK,
            MessageBoxButton.OKCancel => MessageBoxButton.OKCancel,
            MessageBoxButton.YesNo => MessageBoxButton.YesNo,
            MessageBoxButton.YesNoCancel => MessageBoxButton.YesNoCancel,
            _ => MessageBoxButton.OK
        };
    }

    private MessageBoxImage ConvertToWpfImage(MessageBoxImage image)
    {
        return image switch
        {
            MessageBoxImage.Error => MessageBoxImage.Error,
            MessageBoxImage.Warning => MessageBoxImage.Warning,
            MessageBoxImage.Information => MessageBoxImage.Information,
            MessageBoxImage.Question => MessageBoxImage.Question,
            _ => MessageBoxImage.None
        };
    }

    private MessageBoxResult ConvertFromWpfResult(MessageBoxResult result)
    {
        return result switch
        {
            MessageBoxResult.OK => MessageBoxResult.OK,
            MessageBoxResult.Cancel => MessageBoxResult.Cancel,
            MessageBoxResult.Yes => MessageBoxResult.Yes,
            MessageBoxResult.No => MessageBoxResult.No,
            _ => MessageBoxResult.None
        };
    }

    // ========== DISPOSAL ==========

    public void Dispose()
    {
        if (_disposed) return;

        _logger.LogDebug("[DIALOG_SERVICE] Disposing with {Count} active dialogs", _dialogStack.Count);

        CloseAllDialogs();

        _dialogStack.Clear();
        _activeDialogs.Clear();
        _childDialogs.Clear();

        _disposed = true;
    }

    // ========== INNER CLASSES ==========

    private class DialogInfo
    {
        public Guid DialogId { get; init; }
        public Guid? ParentDialogId { get; init; }
        public Window Window { get; init; } = null!;
        public IViewModel ViewModel { get; init; } = null!;
        public Type ViewModelType { get; init; } = null!;
        public TaskCompletionSource<IDialogResult> CompletionSource { get; init; } = null!;
    }
}
