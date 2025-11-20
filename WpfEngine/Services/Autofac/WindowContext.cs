using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Evaluation;
using WpfEngine.Data.Parameters;
using WpfEngine.Data.Windows;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Enums;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Enhanced Window Context with ViewModel-aware child tracking
/// When current ViewModel changes, all its child windows are closed
/// </summary>
public class WindowContext : IWindowContext, IDisposable
{
    private readonly IWindowIdentity _windowIdentity;
    private readonly IWindowTracker _windowTracker;
    private readonly IScopedWindowManager _windowManager;
    private readonly ILogger<WindowContext> _logger;

    private WindowContextErrorInfo? _lastError;
    private bool _disposed;
    private readonly object _lock = new();

    public Guid WindowId => _windowIdentity.WindowId;

    public WindowContext(
        IWindowIdentity windowIdentity,
        IWindowTracker windowTracker,
        IScopedWindowManager windowManager,
        ILogger<WindowContext> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
        _windowTracker = windowTracker ?? throw new ArgumentNullException(nameof(windowTracker));
        _windowIdentity = windowIdentity ?? throw new ArgumentNullException(nameof(windowIdentity));
        
        _windowManager.WindowClosed += OnGlobalWindowClosed;
        _windowManager.WindowError += OnGlobalWindowError;
    }

    // ========== EVENTS ==========

    public event EventHandler<ChildWindowClosedEventArgs>? ChildClosed;
    public event EventHandler<WindowContextErrorEventArgs>? OperationError;


    // ========== CHILD WINDOW OPERATIONS ==========

    public OperationResult<Guid> TryOpenWindow<TViewModel>() where TViewModel : IViewModel
     => TryOpenChildInternal<TViewModel>();

    public OperationResult<Guid> TryOpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    => TryOpenChildInternal<TViewModel, TParameters>(parameters);

    public Guid OpenWindow<TViewModel>() where TViewModel : IViewModel
        => TryOpenChildInternal<TViewModel>().GetValueOrThrow();

    public Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
        => TryOpenChildInternal<TViewModel, TParameters>(parameters).GetValueOrThrow();

    // ========== SHOW DIALOG METHODS ==========
    public async Task<DialogResult> ShowDialogAsync<TViewModel>()
        where TViewModel : class, IViewModel, IDialogViewModel
    {
        return await _windowManager.ShowDialogAsync<TViewModel>(WindowId, DialogModality.WindowModal);
    }

    public async Task<DialogResult> ShowDialogAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : class, IViewModel, IDialogViewModel<TParameters>
        where TParameters : IViewModelParameters
    {
        return await _windowManager.ShowDialogAsync<TViewModel, TParameters>(WindowId, parameters, DialogModality.WindowModal);
    }

    public async Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : class, IViewModel, IDialogViewModel, IResultDialogViewModel<TResult>
        where TResult : class
    {
        return await _windowManager.ShowDialogAsync<TViewModel, TResult>(WindowId, DialogModality.WindowModal);
    }

    public async Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TParameters, TResult>(TParameters parameters)
        where TViewModel : class, IViewModel, IDialogViewModel<TParameters>, IResultDialogViewModel<TResult>
        where TParameters : IViewModelParameters
        where TResult : class

    {
        return await _windowManager.ShowDialogAsync<TViewModel, TParameters, TResult>(WindowId, parameters, DialogModality.WindowModal);
    }

    // ========== WINDOW MANAGEMENT ==========

    public OperationResult TryCloseWindow(bool showConfirmation = false)
    {
        try
        {
            EnsureActiveWindow();

            if (showConfirmation)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to close this window?",
                    "Close Window",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return OperationResult.Failure("Close cancelled by user");
                }
            }

            // Close all children first
            TryCloseAllChildWindows();

            // Close this window
            return _windowManager.TryCloseWindow(WindowId);
        }
        catch (Exception ex)
        {
            return HandleCloseError("CloseWindow", ex);
        }
    }
    public OperationResult TryCloseAllChildWindows() => _windowManager.TryCloseAllChildWindows(WindowId);
    public OperationResult TryCloseChild(Guid childId) => _windowManager.TryCloseWindow(childId);


    public void CloseWindow() => TryCloseWindow(false).ThrowIfFailed();
    public void CloseAllChildWindows() => TryCloseAllChildWindows().ThrowIfFailed();
    public void CloseChildWindow(Guid childId) => TryCloseChild(childId).ThrowIfFailed();

    // ========== QUERIES ==========

    public IReadOnlyList<Guid> GetChildIds() => _windowTracker.GetChildWindows(WindowId).ToList();
    
    public bool IsWindowOpen(Guid childId) => _windowTracker.IsWindowOpen(childId);

    public int ChildWindowsCount => _windowTracker.GetChildWindows(WindowId).Count;


    // ========== ERROR RECOVERY ==========

    public OperationResult TryRecover()
    {
        try
        {
            _logger.LogInformation("[WINDOW_CONTEXT] Attempting recovery for window {WindowId}", WindowId);

            // Clear error state
            _lastError = null;

            _logger.LogInformation("[WINDOW_CONTEXT] Recovery successful");

            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Recovery failed: {ex.Message}", ex);
        }
    }

    private void OnGlobalWindowClosed(object? sender, WindowClosedEventArgs e)
    {
        lock (_lock)
        {
            if (e.ParentWindowId.HasValue && e.ParentWindowId == WindowId)
            {
                _logger.LogDebug("[WINDOW_CONTEXT] Child window {ChildId} closed", e.WindowId);

                ChildClosed?.Invoke(this, new ChildWindowClosedEventArgs(e.WindowId, e.ViewModelType));
            }
        }
    }

    private void OnGlobalWindowError(object? sender, WindowErrorEventArgs e)
    {
        if (e.WindowId.HasValue)
        {
            HandleError(e.Operation, e.ErrorMessage, e.Exception, e.WindowId);
        }
    }

    private void EnsureActiveWindow()
    {
        if (WindowId == Guid.Empty)
        {
            throw new InvalidOperationException("WindowId not set");
        }
        else if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WindowContext));
        }
    }

    private OperationResult<Guid> TryOpenChildInternal<TViewModel>() where TViewModel : IViewModel
    {
        return TryOpenChildInternal<TViewModel, BaseModelParameters>(null);
    }

    private OperationResult<Guid> TryOpenChildInternal<TViewModel, TParameters>(TParameters? parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
    {

        try
        {
            EnsureActiveWindow();

            OperationResult<Guid> result;

            if (parameters is null)
            {
                result = _windowManager.TryOpenChildWindow<TViewModel>(WindowId);
            }
            else
            {
                result = _windowManager.TryOpenChildWindow<TViewModel, TParameters>(WindowId, parameters);
            }

            if (result.IsSuccess)
            {
                _logger.LogInformation("[WINDOW_CONTEXT] Opened child window {ChildId} with parameters", result.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            return HandleError<TViewModel>("OpenChild", ex);
        }
    }

    private void HandleError(string operation, string errorMessage, Exception? exception, Guid? childWindowId = null)
    {
        _lastError = new WindowContextErrorInfo
        {
            Timestamp = DateTime.Now,
            Operation = operation,
            ErrorMessage = errorMessage,
            Exception = exception,
            ChildWindowId = childWindowId
        };

        OperationError?.Invoke(this, new WindowContextErrorEventArgs(operation, errorMessage, exception, childWindowId));

        _logger.LogError(exception, "[WINDOW_CONTEXT] {Operation} error: {ErrorMessage}", operation, errorMessage);
    }

    private OperationResult<Guid> HandleError<TViewModel>(string operation, Exception ex) where TViewModel : IViewModel
    {
        var errorMessage = $"Failed to {operation} for {typeof(TViewModel).Name}: {ex.Message}";

        HandleError(operation, errorMessage, ex);

        return OperationResult<Guid>.Failure(errorMessage, ex);
    }

    private OperationResult HandleCloseError(string operation, Exception ex)
    {
        var errorMessage = $"Failed to {operation}: {ex.Message}";
        HandleError(operation, errorMessage, ex);
        return OperationResult.Failure(errorMessage, ex);
    }

    // ========== DISPOSAL ==========

    private void Dispose(bool disposiong)
    {
        if (_disposed) return;
        if (disposiong)
        {
            _logger.LogDebug("[WINDOW_CONTEXT] Disposing (WindowId: {WindowId})", WindowId);

            try
            {
                TryCloseAllChildWindows();
                TryCloseWindow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WINDOW_CONTEXT] Error closing children during disposal");
            }

            _windowManager.WindowClosed -= OnGlobalWindowClosed;
            _windowManager.WindowError -= OnGlobalWindowError;

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}