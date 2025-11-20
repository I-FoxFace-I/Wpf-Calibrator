using System;
using System.Windows;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Autofac;

/// <summary>
/// Self-disposing handle that owns window scope
/// Prevents memory leaks by ensuring scope disposal
/// 
/// Key responsibilities:
/// - Owns the ILifetimeScope for the window
/// - Auto-disposes when window closes
/// - Uses weak references to prevent memory leaks
/// - Finalizer detects if Dispose() was not called (memory leak indicator)
/// </summary>
public sealed class WindowHandle : IDisposable
{
    private readonly Guid _windowId;
    private readonly ILifetimeScope _scope;
    private readonly ILogger? _logger;
    private bool _disposed;
    
    // Weak references - no memory leaks!
    private WeakReference<Window>? _windowRef;
    private WeakReference<IViewModel>? _viewModelRef;
    
    public Guid WindowId => _windowId;
    public ILifetimeScope Scope => _scope;
    
    public WindowHandle(
        Guid windowId, 
        ILifetimeScope scope,
        Window window,
        IViewModel viewModel,
        ILogger? logger = null)
    {
        _windowId = windowId;
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _windowRef = new WeakReference<Window>(window);
        _viewModelRef = new WeakReference<IViewModel>(viewModel);
        _logger = logger;
        
        // Critical: Auto-dispose when window closed
        window.Closed += OnWindowClosed;
        
        _logger?.LogDebug("[WINDOW_HANDLE] Created for window {WindowId}", windowId);
    }
    
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        // Unsubscribe immediately to prevent leak
        if (sender is Window w)
            w.Closed -= OnWindowClosed;
        
        _logger?.LogDebug("[WINDOW_HANDLE] Window {WindowId} closed, auto-disposing", _windowId);
        
        // Self-dispose
        Dispose();
    }

    /// <summary>
    /// Finalizer - detects memory leaks!
    /// If this is called, it means Dispose() was never called = MEMORY LEAK
    /// </summary>
    ~WindowHandle()
    {
        if (!_disposed || (_windowRef?.TryGetTarget(out _) ?? false))
        {
            _logger?.LogError(
                "[MEMORY LEAK] WindowHandle {WindowId} was finalized without disposal! " +
                "This indicates a memory leak - Dispose() was never called.", 
                _windowId);


            #if DEBUG
            System.Diagnostics.Debug.WriteLine(
                $"[MEMORY LEAK] WindowHandle {_windowId} was not disposed!");
            #endif
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _logger?.LogDebug("[WINDOW_HANDLE] Disposing window {WindowId}", _windowId);
        
        _disposed = true;

        // Suppress finalizer - no leak!
        GC.SuppressFinalize(this);

        // 1) Unsubscribe from window events (defensive)
        if (_windowRef?.TryGetTarget(out var window) ?? false)
        {
            try { window.Closed -= OnWindowClosed; }
            catch { }
        }
        
        // 2) Dispose ViewModel first
        if (_viewModelRef?.TryGetTarget(out var viewModel) ?? false)
        {
            if (viewModel is IDisposable disposable)
            {
                try 
                { 
                    disposable.Dispose(); 
                }
                catch (Exception ex) 
                { 
                    _logger?.LogError(ex, "[WINDOW_HANDLE] Error disposing ViewModel for window {WindowId}", _windowId); 
                }
            }
        }
        
        // 3) Dispose Scope (this disposes everything in it, including Window)
        try 
        { 
            _scope.Dispose(); 
        }
        catch (Exception ex) 
        { 
            _logger?.LogError(ex, "[WINDOW_HANDLE] Error disposing scope for window {WindowId}", _windowId); 
        }

        //// 4) Clear weak references
        _windowRef = null;
        _viewModelRef = null;

        _logger?.LogInformation("[WINDOW_HANDLE] Disposed window {WindowId}", _windowId);
    }
}

