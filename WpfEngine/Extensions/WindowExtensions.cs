using System.Windows;

namespace WpfEngine.Extensions;

public static class WindowExtensions
{
    public static void Enable(this Window window)
    {
        window.IsEnabled = true;
    }

    public static void Disable(this Window window)
    {
        window.IsEnabled = false;
    }
}
