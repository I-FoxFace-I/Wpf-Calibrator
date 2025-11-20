using WpfEngine.Data.Abstract;
using WpfEngine.Data.Windows.Events;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Sessions;

/// <summary>
/// Window context scoped to a specific session - manages windows within the session
/// </summary>
public interface ISessionWindowHost : IDisposable
{
    /// <summary>
    /// Number of windows currently open in this session
    /// </summary>
    int WindowCount { get; }
    
    /// <summary>
    /// Open a window in this session
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel type</typeparam>
    /// <returns>Window ID</returns>
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;
    
    /// <summary>
    /// Open a window with parameters in this session
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel type</typeparam>
    /// <typeparam name="TParameters">Parameters type</typeparam>
    /// <param name="parameters">Parameters to pass to the ViewModel</param>
    /// <returns>Window ID</returns>
    Guid OpenWindow<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;
    
    /// <summary>
    /// Open a dialog window in this session
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel type</typeparam>
    /// <returns>Dialog result task</returns>
    Task<bool?> ShowDialogAsync<TViewModel>() where TViewModel : IViewModel;
    
    /// <summary>
    /// Open a dialog window with parameters in this session
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel type</typeparam>
    /// <typeparam name="TParameters">Parameters type</typeparam>
    /// <param name="parameters">Parameters to pass to the ViewModel</param>
    /// <returns>Dialog result task</returns>
    Task<bool?> ShowDialogAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;
    
    /// <summary>
    /// Close a specific window in this session
    /// </summary>
    /// <param name="windowId">Window ID</param>
    void CloseWindow(Guid windowId);
    
    /// <summary>
    /// Close all windows in this session
    /// </summary>
    void CloseAllWindows();
    
    /// <summary>
    /// Raised when a window is opened in this session
    /// </summary>
    event EventHandler<WindowOpenedEventArgs>? WindowOpened;
    
    /// <summary>
    /// Raised when a window is closed in this session
    /// </summary>
    event EventHandler<WindowClosedEventArgs>? WindowClosed;
}

