using System;
using System.Windows;
using System.Windows.Threading;

namespace WpfEngine.Tests.Helpers;

/// <summary>
/// Helper utilities for WPF tests requiring dispatcher synchronization
/// </summary>
public static class WpfTestHelpers
{
    /// <summary>
    /// Waits for WPF dispatcher to process all pending operations at specified priority
    /// </summary>
    /// <param name="priority">Priority level to wait for (default: Background)</param>
    public static void WaitForDispatcher(DispatcherPriority priority = DispatcherPriority.Background)
    {
        if (Application.Current?.Dispatcher == null)
            return;

        var frame = new DispatcherFrame();
        Application.Current.Dispatcher.BeginInvoke(
            priority,
            new DispatcherOperationCallback(ExitFrame),
            frame);
        Dispatcher.PushFrame(frame);
    }

    /// <summary>
    /// Waits for window to be fully loaded and initialized
    /// </summary>
    public static void WaitForWindowLoaded()
    {
        WaitForDispatcher(DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Waits for all pending UI operations to complete
    /// </summary>
    public static void WaitForPendingOperations()
    {
        WaitForDispatcher(DispatcherPriority.Background);
    }

    /// <summary>
    /// Waits for all pending window closing operations to complete
    /// </summary>
    public static void WaitForWindowClosed()
    {
        WaitForDispatcher(DispatcherPriority.Normal);
    }

    /// <summary>
    /// Waits for rendering to complete
    /// </summary>
    public static void WaitForRender()
    {
        WaitForDispatcher(DispatcherPriority.Render);
    }

    private static object ExitFrame(object frame)
    {
        ((DispatcherFrame)frame).Continue = false;
        return null;
    }
}

/// <summary>
/// Usage in tests:
/// 
/// [STAFact]
/// public void MyTest()
/// {
///     // Open window
///     var windowId = _service.OpenWindow<MyViewModel>();
///     
///     // Wait for window to fully load
///     WpfTestHelpers.WaitForWindowLoaded();
///     
///     // Do assertions
///     Assert.True(_service.IsWindowOpen(windowId));
///     
///     // Close window
///     _service.Close(windowId);
///     
///     // Wait for close operations to complete
///     WpfTestHelpers.WaitForPendingOperations();
///     
///     // Assert window is closed
///     Assert.False(_service.IsWindowOpen(windowId));
/// }
/// </summary>