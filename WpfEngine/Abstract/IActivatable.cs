using System.Threading;
using System.Threading.Tasks;

namespace WpfEngine.Abstract;

/// <summary>
/// Interface for ViewModels that support activation/deactivation lifecycle
/// Used by IContentManager to manage content state
/// </summary>
public interface IActivatable
{
    /// <summary>
    /// Called when content becomes active (visible/focused)
    /// Use this to refresh data, start background tasks, etc.
    /// </summary>
    Task OnActivateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when content becomes inactive (hidden/unfocused)
    /// Use this to pause tasks, save state, etc.
    /// </summary>
    Task OnDeactivateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicates whether this content is currently active
    /// </summary>
    bool IsActive { get; }
}


