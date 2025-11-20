using System.Windows;
using System.Windows.Threading;
using WpfEngine.Extensions;

namespace WpfEngine.Helpers;

public static class DispatcherTools
{
    public static void CloseWindow(Window window)
    {
        if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
        {
            window.Dispatcher.Invoke(window.Close, DispatcherPriority.Input);
        }
        else
        {
            window.Close();
        }
    }

    public static void ShowWindow(Window window)
    {
        if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
        {
            window.Dispatcher.Invoke(window.Show, DispatcherPriority.Input);
        }
        else
        {
            window.Show();
        }
    }

    public static void ShowDialogWindow(Window window)
    {
        if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
        {
            window.Dispatcher.Invoke(window.ShowDialog, DispatcherPriority.Input);
        }
        else
        {
            window.ShowDialog();
        }
    }

    public static void EnableWindow(Window window)
    {
        if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
        {
            window.Dispatcher.Invoke(window.Enable, DispatcherPriority.ContextIdle);
        }
        else
        {
            window.Enable();
        }
    }

    public static void DisableWindow(Window window)
    {
        if (window.Dispatcher != null && !window.Dispatcher.CheckAccess())
        {
            window.Dispatcher.Invoke(window.Disable, DispatcherPriority.Background);
        }
        else
        {
            window.Disable();
        }
    }
}
