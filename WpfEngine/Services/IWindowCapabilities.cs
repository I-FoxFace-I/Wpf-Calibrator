using System.Windows;

namespace WpfEngine.Services;

public interface IWindowCapabilities
{
    Task ActivateAsync(CancellationToken ct = default);
    Task<nint?> GetHandleAsync(CancellationToken ct = default);
    Task<bool> WithWindowAsync(Func<Window, Task> action, CancellationToken ct = default);
}
