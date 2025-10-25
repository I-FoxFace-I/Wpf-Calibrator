using System;
using System.Threading.Tasks;

namespace WpfEngine.Core.Services;

/// <summary>
/// Event args for WindowClosed event
/// </summary>
public class WindowClosedEventArgs : EventArgs
{
    public WindowClosedEventArgs(Guid windowId, Type viewModelType, object viewModel)
    {
        WindowId = windowId;
        ViewModelType = viewModelType;
        ViewModel = viewModel;
    }

    /// <summary>
    /// Unique window identifier
    /// </summary>
    public Guid WindowId { get; }

    /// <summary>
    /// Type of ViewModel
    /// </summary>
    public Type ViewModelType { get; }

    /// <summary>
    /// ViewModel instance
    /// </summary>
    public object ViewModel { get; }
}
