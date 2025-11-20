using System.Windows;
using System.Windows.Interop;
using WpfEngine.Services;

namespace WpfEngine.Services.Autofac;

public sealed class WindowCapabilities : IWindowCapabilities
{
    private readonly IWindowTracker _tracker;
    private readonly IWindowIdentity _id;

    public WindowCapabilities(IWindowTracker tracker, IWindowIdentity id) { _tracker = tracker; _id = id; }

    public async Task ActivateAsync(CancellationToken ct = default)
        => await WithWindowAsync(w => { w.Activate(); w.Focus(); return Task.CompletedTask; }, ct);

    public async Task<nint?> GetHandleAsync(CancellationToken ct = default)
    {
        nint handle = 0;
        await WithWindowAsync(w => { handle = new WindowInteropHelper(w).Handle; return Task.CompletedTask; }, ct);
        return handle == nint.Zero ? null : handle;
    }

    public async Task<bool> WithWindowAsync(Func<Window, Task> action, CancellationToken ct = default)
    {
        var meta = _tracker.GetMetadata(_id.WindowId);
        if (meta == null) return false;

        // Use WithWindow to safely get window reference and hold it during action
        return await Task.Run(async () =>
        {
            Window? window = null;
            bool hasWindow = meta.WithWindow(w =>
            {
                window = w; // Capture window reference
            });

            if (!hasWindow || window == null) return false;

            var d = window.Dispatcher;
            if (d.CheckAccess())
            {
                await action(window);
                return true;
            }

            await d.InvokeAsync(async () => await action(window), System.Windows.Threading.DispatcherPriority.Normal, ct);
            return true;
        });
    }
}
