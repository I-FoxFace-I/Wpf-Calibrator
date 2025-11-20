using Autofac;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using WpfEngine.Enums;
using WpfEngine.Services.Autofac;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Metadata;

/// <summary>
/// Window metadata stored in tracker
/// </summary>
public class WindowMetadata : IDisposable
{
    private bool _closed = false;
    private bool _disposed = false;
    private object _lock = new object();
    private Lazy<TaskCompletionSource<bool>>? _closedTcs = new(() => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));


    /// <summary>
    /// Window ID
    /// </summary>
    public Guid WindowId { get; set; }

    /// <summary>
    /// Parent window ID (if child)
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Session ID (if in session)
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Weak reference to window instance
    /// </summary>
    public WeakReference<Window>? WindowRef { get; set; }

    /// <summary>
    /// Weak reference to ViewModel
    /// </summary>
    public WeakReference<IViewModel>? ViewModelRef { get; set; }

    /// <summary>
    /// ViewModel type
    /// </summary>
    public Type? ViewModelType { get; set; }

    /// <summary>
    /// Window handle that owns the scope (prevents memory leaks)
    /// </summary>
    public WindowHandle? Handle { get; set; }

    /// <summary>
    /// Window's own scope (if ScopedWindow)
    /// NOTE: Deprecated - use Handle.Scope instead. This property is kept for backward compatibility.
    /// </summary>
    public ILifetimeScope? WindowScope 
    {
        get => Handle?.Scope;
        set { } // Deprecated - set Handle instead
    }

    /// <summary>
    /// Lifecycle state of window (Creating/Open/Closing/Closed/Faulted)
    /// </summary>
    public WindowLifecycleState Lifecycle { get; set; } = WindowLifecycleState.Creating;


    public int CreatedThreadId { get; set; } = Thread.CurrentThread.ManagedThreadId;

    public Task ClosedTask
    {
        get
        {
            lock (_lock)
            {
                if(!_closed)
                {
                    return _closedTcs?.Value.Task ?? Task.CompletedTask;
                }
                if (_closedTcs?.IsValueCreated ?? false)
                {
                    return _closedTcs?.Value.Task ?? Task.CompletedTask;
                }

                return Task.CompletedTask;
            }
        }
    }
    
    public void SetClosed()
    {
        lock (_lock)
        {
            _closed = true;

            if (_closedTcs?.Value != null)
            {
                _closedTcs.Value.TrySetResult(_closed);
            }

            WindowRef = null;
            ViewModelRef = null;
            Lifecycle = WindowLifecycleState.Closed;
        }
    }

    public bool IsOpened()
    {
        lock (_lock)
        {
            if (_closed)
                return false;

            return Lifecycle == WindowLifecycleState.Open;
        }
    }


    public void DisposeScopeSafely()
    {
        // Dispose handle - it will dispose the scope properly
        if (Handle != null)
        {
            try { Handle.Dispose(); }
            catch (ObjectDisposedException) { /* already disposed */ }
            finally
            {
                Handle = null;
                Lifecycle = WindowLifecycleState.Closed;
            }
        }
    }

    /// <summary>
    /// Safely executes an action with the window if it's still alive.
    /// Prevents GC race conditions by holding a strong reference during the action.
    /// </summary>
    /// <param name="action">Action to execute with the window</param>
    /// <returns>True if window was alive and action was executed; false otherwise</returns>
    public bool WithWindow(Action<Window> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        if (WindowRef?.TryGetTarget(out var window) ?? false)
        {
            // Hold strong reference during action to prevent GC race condition
            try
            {
                action(window);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Safely executes a function with the window if it's still alive and returns a result.
    /// Prevents GC race conditions by holding a strong reference during the function execution.
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <param name="func">Function to execute with the window</param>
    /// <param name="defaultValue">Default value to return if window is not alive</param>
    /// <returns>Function result if window was alive; defaultValue otherwise</returns>
    public TResult? WithWindow<TResult>(Func<Window, TResult> func, TResult? defaultValue = default)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));

        if (WindowRef?.TryGetTarget(out var window) ?? false)
        {
            // Hold strong reference during function to prevent GC race condition
            try
            {
                return func(window);
            }
            catch
            {
                return default;
            }
            finally
            {
                window = null;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Safely executes an action with the ViewModel if it's still alive.
    /// Prevents GC race conditions by holding a strong reference during the action.
    /// </summary>
    /// <param name="action">Action to execute with the ViewModel</param>
    /// <returns>True if ViewModel was alive and action was executed; false otherwise</returns>
    public bool WithViewModel(Action<IViewModel> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        if (ViewModelRef?.TryGetTarget(out var viewModel) ?? false)
        {
            // Hold strong reference during action to prevent GC race condition
            try
            {
                action(viewModel);
                return true;
            }
            catch
            {
                return false;
            }
            finally { viewModel = null; }
        }

        return false;
    }

    /// <summary>
    /// Safely executes a function with the ViewModel if it's still alive and returns a result.
    /// Prevents GC race conditions by holding a strong reference during the function execution.
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <param name="func">Function to execute with the ViewModel</param>
    /// <param name="defaultValue">Default value to return if ViewModel is not alive</param>
    /// <returns>Function result if ViewModel was alive; defaultValue otherwise</returns>
    public TResult? WithViewModel<TResult>(Func<IViewModel, TResult> func, TResult? defaultValue = default)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));

        if (ViewModelRef?.TryGetTarget(out var viewModel) ?? false)
        {
            // Hold strong reference during function to prevent GC race condition
            try
            {
                return func(viewModel);
            }
            catch
            {
                return default;
            }
            finally
            {
                viewModel = null;
            }
        }

        return defaultValue;
    }

    public void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_disposed)
            {
                WindowRef = null;
                WindowScope = null;
                ViewModelRef = null;
                ViewModelType = null;
                _disposed = true;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}