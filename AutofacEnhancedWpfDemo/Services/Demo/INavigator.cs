using System.ComponentModel;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.Services.Demo;

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
}
