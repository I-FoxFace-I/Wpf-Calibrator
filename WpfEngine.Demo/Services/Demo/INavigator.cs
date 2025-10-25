using System.ComponentModel;
using System.Threading.Tasks;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Handles ViewModel navigation within a window
/// Used for workflows and multi-view windows
/// </summary>
public interface INavigator : INotifyPropertyChanged
{
    /// <summary>
    /// Navigates to a new ViewModel within the current window
    /// </summary>
    Task NavigateToAsync<TViewModel>(object? parameters = null) where TViewModel : class;

    /// <summary>
    /// Goes back to previous ViewModel
    /// </summary>
    Task NavigateBackAsync();

    /// <summary>
    /// Gets the current ViewModel (notifies when changed)
    /// </summary>
    object? CurrentViewModel { get; }

    /// <summary>
    /// Checks if can navigate back
    /// </summary>
    bool CanNavigateBack { get; }

    /// <summary>
    /// Clears navigation history
    /// </summary>
    void ClearHistory();

    /// <summary>
    /// Event raised when Navigator requests window closure
    /// Host ViewModel should handle this and close the window
    /// </summary>
    event EventHandler<WindowCloseRequestedEventArgs>? WindowCloseRequested;

    /// <summary>
    /// Requests the host window to close
    /// </summary>
    void RequestWindowClose(bool showConfirmation = false, string? confirmationMessage = null);
}

/// <summary>
/// Event args for window close request
/// </summary>
public class WindowCloseRequestedEventArgs : EventArgs
{
    public bool ShowConfirmation { get; init; }
    public string? ConfirmationMessage { get; init; }
}