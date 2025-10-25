using System;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.Services;

/// <summary>
/// Workflow session context - provides access to session and window management
/// Injected into workflow ViewModels to open windows in session
/// </summary>
public interface IWorkflowSession
{
    /// <summary>
    /// Session ID
    /// </summary>
    Guid SessionId { get; }
    
    /// <summary>
    /// Session name
    /// </summary>
    string SessionName { get; }
    
    /// <summary>
    /// Opens window in this workflow session
    /// Window will share session-scoped services
    /// </summary>
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;
    
    /// <summary>
    /// Opens window in this workflow session with options
    /// </summary>
    Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;
    
    /// <summary>
    /// Opens child window in this workflow session
    /// </summary>
    Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel;
    
    /// <summary>
    /// Opens child window in this workflow session with options
    /// </summary>
    Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;
    
    /// <summary>
    /// Closes window in this session
    /// </summary>
    void CloseWindow(Guid windowId);
    
    /// <summary>
    /// Closes entire session and all its windows
    /// </summary>
    void CloseSession();
}

