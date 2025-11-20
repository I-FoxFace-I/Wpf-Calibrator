using Autofac;
using Microsoft.Extensions.Logging;
using System.CodeDom;
using System.Windows;
using System.Windows.Threading;
using WpfEngine.Abstract;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Evaluation;
using WpfEngine.Data.Parameters;
using WpfEngine.Data.Windows;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Enums;
using WpfEngine.Extensions;
using WpfEngine.Helpers;
using WpfEngine.Services;
using WpfEngine.Services.Metadata;
using WpfEngine.ViewModels;
using WpfEngine.Views;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Abstract base class for window management with support for windows and dialogs
/// Provides core window lifecycle management and error handling
/// </summary>
public abstract class WindowManager : IWindowManager
{
    protected readonly ILifetimeScope _rootScope;
    protected readonly IViewRegistry _registry;
    protected readonly IWindowTracker _windowTracker;
    protected readonly ILogger _logger;

    // Error tracking
    private WindowErrorInfo? _lastError;
    protected readonly object _lock = new();

    protected WindowManager(
        ILifetimeScope rootScope,
        IViewRegistry registry,
        IWindowTracker windowTracker,
        ILogger logger)
    {
        _rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _windowTracker = windowTracker ?? throw new ArgumentNullException(nameof(windowTracker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========== EVENTS ==========

    public event EventHandler<WindowOpenedEventArgs>? WindowOpened;
    public event EventHandler<WindowClosedEventArgs>? WindowClosed;
    public event EventHandler<WindowErrorEventArgs>? WindowError;

    // ========== ABSTRACT METHODS (session/scope handling) ==========

    /// <summary>
    /// Get the scope for a specific session
    /// </summary>
    protected abstract ILifetimeScope? GetSessionScope(Guid sessionId);

    /// <summary>
    /// Get the session ID from window metadata
    /// </summary>
    protected abstract Guid? GetWindowSessionId(Guid windowId);

    // ========== CORE WINDOW OPERATIONS (protected) ==========

    /// <summary>
    /// Create a window-specific lifetime scope with WindowIdentity
    /// </summary>
    protected static ILifetimeScope CreateWindowScope(
        ILifetimeScope parentScope,
        Guid windowId,
        Guid? parentId,
        Guid? sessionId,
        bool isDialog)
    {
        return parentScope.BeginLifetimeScope($"Window:{windowId}", b =>
        {
            b.RegisterInstance(new WindowIdentity(windowId, parentId, sessionId, isDialog))
             .As<IWindowIdentity>()
             .SingleInstance();

            b.RegisterType<WindowCapabilities>()
             .As<IWindowCapabilities>()
             .InstancePerLifetimeScope();
        });
    }

    /// <summary>
    /// Core window opening logic - creates window, view model, and tracks lifecycle
    /// </summary>
    protected Guid OpenWindowCore<TViewModel>(
        ILifetimeScope parentScope,
        Guid? parentWindowId,
        Guid? sessionId,
        bool isDialog = false)
        where TViewModel : IViewModel
    {
        return OpenWindowCore<TViewModel, BaseModelParameters>(
            parentScope, null, parentWindowId, sessionId, isDialog);
    }



    /// <summary>
    /// Core window opening logic with parameters
    /// </summary>
    protected Guid OpenWindowCore<TViewModel, TParameters>(
        ILifetimeScope parentScope,
        TParameters? parameters,
        Guid? parentWindowId,
        Guid? sessionId,
        bool isDialog = false)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters?
    {
        Guid windowId = Guid.NewGuid();
        ILifetimeScope? windowScope = null;
        Window? window = null;
        IViewModel? viewModel = null;
        List<Action> errorCallbacks = new();

        lock (_lock)
        {
            try
            {
                Type viewModelType = typeof(TViewModel);

                _logger.LogInformation("[WINDOW_MANAGER] Opening {Type} for {ViewModelType}",
                    isDialog ? "dialog" : "window", viewModelType.Name);

                // 1) Get view type
                if (!_registry.TryGetViewType(viewModelType, out var viewType))
                {
                    throw new InvalidOperationException(
                        $"No view registered for {viewModelType.Name}");
                }

                // 2) Create window scope
                windowScope = CreateWindowScope(parentScope, windowId, parentWindowId, sessionId, isDialog);

                // 3) Resolve window
                if (windowScope.TryResolve(viewType, out var instance) && instance is Window)
                {
                    window = (Window)instance;
                }

                if (window == null)
                {
                    throw new InvalidOperationException(
                        "Window must implement IWindowView or IScopedView");
                }

                // 4) Assign window ID to scoped view
                if (window is IScopedView scopedView)
                {
                    scopedView.AssignedWindowId = windowId;
                }

                // 5) Resolve ViewModel
                if (parameters != null)
                {
                    viewModel = windowScope.Resolve<TViewModel>(
                        new TypedParameter(parameters.GetType(), parameters));
                }
                else
                {
                    viewModel = windowScope.Resolve<TViewModel>();
                }

                // 6) Data binding
                window.DataContext = viewModel;

                // 7) Initialize ViewModel when loaded
                async void LoadedHandler(object sender, RoutedEventArgs e)
                {
                    if (sender is Window w)
                    {
                        w.Loaded -= LoadedHandler;
                        if (w.DataContext is IInitializable initializable)
                        {
                            await initializable.InitializeAsync();
                        }
                    }
                }
                window.Loaded += LoadedHandler;

                // 8) Create WindowHandle - owns scope and prevents memory leaks
                var handle = new WindowHandle(windowId, windowScope, window, viewModel, _logger);

                // 9) Create metadata and track
                var metadata = new WindowMetadata
                {
                    WindowId = windowId,
                    ParentId = parentWindowId,
                    SessionId = sessionId,
                    WindowRef = new WeakReference<Window>(window),
                    ViewModelRef = new WeakReference<IViewModel>(viewModel),
                    ViewModelType = viewModelType,
                    Handle = handle,
                    Lifecycle = WindowLifecycleState.Creating
                };

                _windowTracker.Track(windowId, metadata);

                // 10) Handle window closed
                void ClosedHandler(object? s, EventArgs e)
                {
                    _windowTracker.WithMetadata(windowId, meta =>
                    {
                        if (meta?.WindowRef?.TryGetTarget(out var w) ?? false)
                        {
                            w.Closed -= ClosedHandler;
                        }
                    });
                    CleanupWindow(windowId);
                }
                window.Closed += ClosedHandler;

                // 11) Show window (on UI thread if needed)
                if (isDialog && parentWindowId.HasValue)
                {
                    // For dialogs, set owner and show modal
                    if (_windowTracker.TryGetMetadata(parentWindowId.Value, out var parentMetadata))
                    {
                        parentMetadata.WithWindow(parentWindow =>
                        {
                            window.Owner = parentWindow;
                        });
                    }

                    // ShowDialog will be called by caller for modal behavior
                    // For now just prepare the window
                }
                else
                {
                    // Regular window - show non-modal
                    DispatcherTools.ShowWindow(window);
                }

                // 12) Update lifecycle status
                _windowTracker.WithMetadata(windowId, m => m.Lifecycle = WindowLifecycleState.Open);

                // 13) Raise event (WindowClosed event will be handled by SessionWindowContext)
                WindowOpened?.Invoke(this, new WindowOpenedEventArgs(windowId, viewModelType, parentWindowId, sessionId));

                _logger.LogInformation("[WINDOW_MANAGER] {Type} {WindowId} opened successfully",
                    isDialog ? "Dialog" : "Window", windowId);

                return windowId;
            }
            catch (Exception)
            {
                // Execute error callbacks
                errorCallbacks.ForEach(callback =>
                {
                    try { callback?.Invoke(); }
                    catch { /* ignore cleanup errors */ }
                });

                // Update metadata to faulted
                try
                {
                    _windowTracker.WithMetadata(windowId, m => m.Lifecycle = WindowLifecycleState.Faulted);
                    _windowTracker.Untrack(windowId);
                }
                catch { /* ignore tracking errors */ }

                // Dispose scope
                try { windowScope?.Dispose(); }
                catch { /* ignore disposal errors */ }

                throw;
            }
            finally
            {
                window = null;
                errorCallbacks.Clear();
            }
        }
    }


    /// <summary>
    /// Core dialog opening logic - creates dialog window with modal behavior
    /// </summary>
    protected Task<TResult> OpenDialogCore<TViewModel, TResult, TData>(
        Guid? ownerWindowId,
        ILifetimeScope sessionScope,
        IViewModelParameters? parameters = null,
        DialogModality modality = DialogModality.WindowModal)
        where TViewModel : class, IViewModel, IDialogViewModel
        where TResult : IDialogResult<TResult, TData>
        where TData : notnull
    {
        var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        Guid? sessionId = null;
        TViewModel? viewModel = null;
        ILifetimeScope? parentScope = null;
        Guid dialogId = Guid.NewGuid();

        // 1) Determine parent scope + owner window
        if (ownerWindowId == null)
        {
            parentScope = sessionScope;
        }
        else if (_windowTracker.TryGetMetadata(ownerWindowId.Value, out var ownerMetadata))
        {
            sessionId = ownerMetadata.SessionId;
            parentScope = ownerMetadata.WindowScope;
        }

        if (parentScope == null)
        {
            throw new ArgumentNullException(nameof(sessionScope));
        }

        // 2) Create dialog scope
        var dialogScope = CreateDialogScope(parentScope, dialogId, ownerWindowId, sessionId);

        // 3) Resolve view type
        if (!_registry.TryGetViewType(typeof(TViewModel), out var viewType))
        {
            throw new InvalidOperationException($"No view registered for {typeof(TViewModel).Name}");
        }

        // 4) Resolve dialog window
        var dialogWindow = dialogScope.Resolve(viewType) as Window;
        if (dialogWindow == null)
        {
            throw new InvalidOperationException($"Resolved view {viewType.Name} must be a Window");
        }

        // 5) Assign window ID to scoped view
        if (dialogWindow is IScopedView scopedView)
        {
            scopedView.AssignedWindowId = dialogId;
        }

        // 6) Resolve ViewModel
        if (parameters != null)
        {
            viewModel = dialogScope.Resolve<TViewModel>(
                new TypedParameter(parameters.GetType(), parameters));
        }
        else
        {
            viewModel = dialogScope.Resolve<TViewModel>();
        }

        // 7) DataContext + owner setup
        dialogWindow.DataContext = viewModel;

        if (ownerWindowId.HasValue)
        {
            _windowTracker.WithMetadata(ownerWindowId.Value, metadata =>
            {
                metadata.WithWindow(ownerWindow =>
                {
                    dialogWindow.Owner = ownerWindow;
                    dialogWindow.ShowInTaskbar = false;
                    dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                });
            });
        }

        // 8) Initialize ViewModel when loaded
        async void LoadedHandler(object sender, RoutedEventArgs e)
        {
            if (sender is Window w)
            {
                w.Loaded -= LoadedHandler;
                if (w.DataContext is IInitializable initializable)
                {
                    await initializable.InitializeAsync();
                }
            }
        }
        dialogWindow.Loaded += LoadedHandler;

        // 9) Create WindowHandle
        var handle = new WindowHandle(dialogId, dialogScope, dialogWindow, viewModel, _logger);

        // 10) Create metadata and track
        var metadata = new WindowMetadata
        {
            WindowId = dialogId,
            ParentId = ownerWindowId,
            SessionId = sessionId,
            WindowRef = new WeakReference<Window>(dialogWindow),
            ViewModelRef = new WeakReference<IViewModel>(viewModel),
            ViewModelType = typeof(TViewModel),
            Handle = handle,
            Lifecycle = WindowLifecycleState.Creating
        };

        _windowTracker.Track(dialogId, metadata);

        if (ownerWindowId.HasValue)
        {
            _windowTracker.SetParent(dialogId, ownerWindowId.Value);
        }

        // 11) Setup dialog result handling
        var windowIdSet = new HashSet<Guid> { dialogId };

        void DialogClosed(object? s, EventArgs e)
        {
            TResult result = TResult.Cancel();

            _windowTracker.WithMetadata(dialogId, meta =>
            {
                if (meta?.WindowRef?.TryGetTarget(out var w) ?? false)
                {
                    w.Closed -= DialogClosed;
                }
            });

            _windowTracker.WithMetadata(dialogId, meta =>
            {
                if (meta?.ViewModelRef?.TryGetTarget(out var vm) ?? false)
                {
                    result = ((TViewModel)vm).CreateDialogResult<TViewModel, TResult, TData>();
                }
                else
                {
                    result = TResult.Error("View model is not IDialogViewModel");
                }
            });

            //TResult result = viewModel.CreateDialogResult<TViewModel, TResult, TData>();

            if (ownerWindowId.HasValue)
            {
                try { EnableOwnerTree(ownerWindowId.Value, windowIdSet); } catch { /* ignore */ }
            }
            try { _windowTracker.WithMetadata(dialogId, m => m.Lifecycle = WindowLifecycleState.Closing); } catch { /* ignore */ }
            try { _windowTracker.WithMetadata(dialogId, m => m.DisposeScopeSafely()); } catch { /* ignore */ }
            try { _windowTracker.WithMetadata(dialogId, m => m.SetClosed()); } catch { /* ignore */ }
            try { _windowTracker.Untrack(dialogId); } catch { /* ignore */ }

            taskCompletionSource.TrySetResult(result);

            //CleanupWindow(dialogId);
        }

        dialogWindow.Closed += DialogClosed;


        // 12) Show dialog
        void ShowDialog()
        {
            try
            {
                if (modality == DialogModality.WindowModal && ownerWindowId.HasValue)
                {
                    DisableOwnerTree(ownerWindowId.Value, windowIdSet);
                    metadata.Lifecycle = WindowLifecycleState.Open;
                    dialogWindow.IsEnabled = true;
                    dialogWindow.Show();
                }
                else
                {
                    metadata.Lifecycle = WindowLifecycleState.Open;
                    dialogWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                if (ownerWindowId.HasValue)
                {
                    try { EnableOwnerTree(ownerWindowId.Value, windowIdSet); } catch { /* ignore */ }
                }
                try { _windowTracker.WithMetadata(dialogId, m => m.Lifecycle = WindowLifecycleState.Faulted); } catch { /* ignore */ }
                try { _windowTracker.WithMetadata(dialogId, m => m.DisposeScopeSafely()); } catch { /* ignore */ }
                try { _windowTracker.Untrack(dialogId); } catch { /* ignore */ }

                taskCompletionSource.TrySetResult(TResult.Error(ex.Message));
            }
        }

        if (dialogWindow.Dispatcher != null && !dialogWindow.Dispatcher.CheckAccess())
        {
            dialogWindow.Dispatcher.Invoke(ShowDialog, DispatcherPriority.Input);
        }
        else
        {
            ShowDialog();
        }

        return taskCompletionSource.Task;

    }

    /// <summary>
    /// Core window closing logic
    /// </summary>
    protected void CloseWindowCore(Guid windowId)
    {
        lock (_lock)
        {
            if (!_windowTracker.TryGetMetadata(windowId, out var metadata))
            {
                _logger.LogError("[WINDOW_MANAGER] Cannot close window {WindowId} - not found", windowId);
                throw new InvalidOperationException($"Window {windowId} not found");
            }

            _windowTracker.WithMetadata(windowId, m => m.Lifecycle = WindowLifecycleState.Closing);

            bool windowClosed = metadata.WithWindow(window =>
            {
                try
                {
                    DispatcherTools.CloseWindow(window);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WINDOW_MANAGER] Error closing window {WindowId}", windowId);
                    throw new InvalidOperationException("Window closing failed", ex);
                }
            });

            if (!windowClosed)
            {
                _logger.LogWarning("[WINDOW_MANAGER] Window {WindowId} was already garbage collected",
                    windowId);
            }
        }

    }

    /// <summary>
    /// Wait for all child windows to close
    /// </summary>
    protected void WaitForChildrenClosed(Guid windowId)
    {
        var childWindowIds = _windowTracker.GetChildWindows(windowId);

        if (childWindowIds.Count > 0)
        {
            var waitingTasks = new List<Task>();
            using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(100));

            foreach (var child in childWindowIds)
            {
                if (_windowTracker.GetMetadata(child)?.ClosedTask is Task task)
                {
                    waitingTasks.Add(task);
                }
                CloseWindowCore(child);
            }

            try
            {
                Task.WaitAll(waitingTasks.ToArray(), cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[WINDOW_MANAGER] Timeout waiting for children to close");
            }
        }
    }

    /// <summary>
    /// Cleanup window resources and raise events
    /// </summary>
    protected void CleanupWindow(Guid windowId)
    {
        WaitForChildrenClosed(windowId);

        if (_windowTracker.TryGetMetadata(windowId, out var metadata))
        {
            try
            {
                var parentId = metadata.ParentId;
                var viewModelType = metadata.ViewModelType;
                var sessionId = metadata.SessionId;

                _logger.LogInformation("[WINDOW_MANAGER] Cleaning up window {WindowId}", windowId);

                // Update state
                _windowTracker.WithMetadata(windowId, m => m.Lifecycle = WindowLifecycleState.Closing);
                _windowTracker.WithMetadata(windowId, m => m.SetClosed());

                // Untrack
                _windowTracker.Untrack(windowId);

                // Dispose scope
                metadata.DisposeScopeSafely();
                metadata.Dispose();

                // Raise event
                WindowClosed?.Invoke(this, new WindowClosedEventArgs(windowId, viewModelType, parentId, sessionId));

                _logger.LogInformation("[WINDOW_MANAGER] Window {WindowId} cleaned up", windowId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WINDOW_MANAGER] Error during cleanup for {WindowId}", windowId);
            }
        }
    }

    // ========== ERROR HANDLING (protected) ==========

    protected OperationResult<Guid> HandleOpenError<TViewModel>(Exception ex, Guid? parentWindowId)
        where TViewModel : IViewModel
    {
        var errorMessage = $"Failed to open window for {typeof(TViewModel).Name}: {ex.Message}";

        _lastError = new WindowErrorInfo
        {
            Timestamp = DateTime.Now,
            Operation = "OpenWindow",
            ErrorMessage = errorMessage,
            Exception = ex,
            WindowId = null
        };

        WindowError?.Invoke(this, new WindowErrorEventArgs("OpenWindow", errorMessage, ex, null));
        _logger.LogError(ex, "[WINDOW_MANAGER] {ErrorMessage}", errorMessage);

        return OperationResult<Guid>.Failure(errorMessage, ex);
    }

    protected OperationResult HandleCloseError(Guid windowId, Exception ex)
    {
        var errorMessage = $"Failed to close window {windowId}: {ex.Message}";

        _lastError = new WindowErrorInfo
        {
            Timestamp = DateTime.Now,
            Operation = "CloseWindow",
            ErrorMessage = errorMessage,
            Exception = ex,
            WindowId = windowId
        };

        WindowError?.Invoke(this, new WindowErrorEventArgs("CloseWindow", errorMessage, ex, windowId));
        _logger.LogError(ex, "[WINDOW_MANAGER] {ErrorMessage}", errorMessage);

        return OperationResult.Failure(errorMessage, ex);
    }

    // ========== PUBLIC INTERFACE (IWindowManager) ==========

    // Throwing methods - primary implementation
    public abstract Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;
    public abstract Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;
    public abstract Guid OpenChildWindow<TViewModel>(Guid parentWindowId) where TViewModel : IViewModel;
    public abstract Guid OpenChildWindow<TViewModel, TParameters>(Guid parentWindowId, TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;
    public abstract Guid OpenWindowInSession<TViewModel>(Guid sessionId) where TViewModel : IViewModel;
    public abstract Guid OpenWindowInSession<TViewModel, TParameters>(Guid sessionId, TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    public abstract void CloseWindow(Guid windowId);
    public abstract void CloseAllChildWindows(Guid parentId);
    public abstract void CloseAllSessionWindows(Guid sessionId);

    // Try methods - error handling wrappers
    public virtual OperationResult<Guid> TryOpenWindow<TViewModel>()
        where TViewModel : IViewModel
    {
        try
        {
            return OperationResult<Guid>.Success(OpenWindow<TViewModel>());
        }
        catch (Exception ex)
        {
            return HandleOpenError<TViewModel>(ex, null);
        }
    }

    public virtual OperationResult<Guid> TryOpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {
        try
        {
            return OperationResult<Guid>.Success(OpenWindow<TViewModel, TParameters>(parameters));
        }
        catch (Exception ex)
        {
            return HandleOpenError<TViewModel>(ex, null);
        }
    }

    public virtual OperationResult<Guid> TryOpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel
    {
        try
        {
            return OperationResult<Guid>.Success(OpenChildWindow<TViewModel>(parentWindowId));
        }
        catch (Exception ex)
        {
            return HandleOpenError<TViewModel>(ex, parentWindowId);
        }
    }

    public virtual OperationResult<Guid> TryOpenChildWindow<TViewModel, TParameters>(
        Guid parentWindowId,
        TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {
        try
        {
            return OperationResult<Guid>.Success(
                OpenChildWindow<TViewModel, TParameters>(parentWindowId, parameters));
        }
        catch (Exception ex)
        {
            return HandleOpenError<TViewModel>(ex, parentWindowId);
        }
    }

    public virtual OperationResult TryCloseWindow(Guid windowId)
    {
        try
        {
            CloseWindow(windowId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return HandleCloseError(windowId, ex);
        }
    }

    public virtual OperationResult TryCloseAllChildWindows(Guid parentId)
    {
        try
        {
            CloseAllChildWindows(parentId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return HandleCloseError(parentId, ex);
        }
    }

    public virtual OperationResult TryCloseAllSessionWindows(Guid sessionId)
    {
        try
        {
            CloseAllSessionWindows(sessionId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return HandleCloseError(sessionId, ex);
        }
    }

    // ========== QUERIES ==========

    public virtual bool IsWindowOpen(Guid windowId) => _windowTracker.WithMetadata(windowId, metadata => metadata.IsOpened(), false);

    public virtual IReadOnlyList<Guid> GetOpenWindowIds() => _windowTracker.OpenWindows;

    public virtual Guid? GetParentWindowId(Guid windowId) => _windowTracker.WithMetadata(windowId, metadata => metadata.ParentId);

    public virtual IReadOnlyList<Guid> GetChildWindowIds(Guid parentWindowId) => _windowTracker.GetChildWindows(parentWindowId);

    public virtual Type? GetViewModelType(Guid windowId) => _windowTracker.WithMetadata(windowId, metadata => metadata.ViewModelType);

    public virtual IReadOnlyList<Guid> GetSessionWindows(Guid sessionId) => _windowTracker.GetSessionWindows(sessionId);


    public virtual bool Activate(Guid windowId)
    {
        return _windowTracker.WithMetadata(windowId, metadata =>
        {
            return metadata.WithWindow(window =>
            {
                try
                {
                    if (Application.Current?.Dispatcher != null)
                    {
                        return Application.Current.Dispatcher.Invoke(() =>
                        {
                            window.Activate();
                            window.Focus();
                            return true;
                        });
                    }

                    window.Activate();
                    window.Focus();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[WINDOW_MANAGER] Error activating window {WindowId}", windowId);
                    return false;
                }
            }, defaultValue: false);
        }, defaultValue: false);
    }

    // ========== ERROR HANDLING ==========

    public virtual void ShowWindowError(string message, Exception? exception = null)
    {
        _logger.LogError(exception, "[WINDOW_MANAGER] Error: {Message}", message);

        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            MessageBox.Show(
                exception != null ? $"{message}\n\nDetails: {exception.Message}" : message,
                "Window Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        });
    }

    public virtual WindowErrorInfo? GetLastError() => _lastError;

    // ========== DIALOG SUPPORT ==========

    /// <summary>
    /// Create a dialog-specific lifetime scope with WindowIdentity
    /// </summary>
    protected static ILifetimeScope CreateDialogScope(
        ILifetimeScope parentScope,
        Guid dialogId,
        Guid? ownerWindowId,
        Guid? sessionId)
    {
        return parentScope.BeginLifetimeScope($"Dialog:{dialogId}", b =>
        {
            b.RegisterInstance(new WindowIdentity(dialogId, ownerWindowId, sessionId, isDialog: true))
             .As<IWindowIdentity>()
             .SingleInstance();

            b.RegisterType<WindowCapabilities>()
             .As<IWindowCapabilities>()
             .InstancePerLifetimeScope();
        });
    }

    /// <summary>
    /// Disable owner window and its entire tree recursively
    /// </summary>
    protected void DisableOwnerTree(Guid ownerId, ISet<Guid> windowIdSet)
    {
        void DisableRecursive(Guid id)
        {
            if (windowIdSet.Contains(id))
                return;

            _windowTracker.WithMetadata(id, metadata =>
            {
                if (metadata.IsOpened())
                {
                    if (metadata.WindowRef?.TryGetTarget(out var window) ?? false)
                    {
                        if (window.IsEnabled)
                        {
                            // Execute on UI thread of that window

                            DispatcherTools.DisableWindow(window);

                            windowIdSet.Add(metadata.WindowId);
                        }
                    }
                }
            });

            // Recursively disable children
            foreach (var childId in _windowTracker.GetChildWindows(id))
            {
                DisableRecursive(childId);
            }
        }

        DisableRecursive(ownerId);
    }

    /// <summary>
    /// Enable owner window tree that was previously disabled
    /// </summary>
    protected void EnableOwnerTree(Guid ownerId, ISet<Guid> windowIdSet)
    {
        if (windowIdSet.Count == 0)
            return;

        foreach (var id in windowIdSet)
        {
            _windowTracker.WithMetadata(id, metadata =>
            {
                if (metadata.Lifecycle == WindowLifecycleState.Open)
                {
                    metadata.WithWindow(window =>
                    {
                        // Execute on UI thread of that window
                        if (window.Dispatcher is Dispatcher dispatcher && !dispatcher.CheckAccess())
                        {
                            window.Dispatcher.Invoke(() =>
                            {
                                if (_windowTracker.TryGetMetadata(id, out var m) &&
                                    m.WindowRef?.TryGetTarget(out var w) == true)
                                {
                                    w.IsEnabled = true;
                                }
                            }, DispatcherPriority.ContextIdle);
                        }
                        else
                        {
                            window.IsEnabled = true;
                        }
                    });
                }
            });
        }

        windowIdSet.Clear();
    }
}

