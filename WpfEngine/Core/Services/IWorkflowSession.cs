using System;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Services;

/// <summary>
/// Represents a workflow session that manages multiple windows sharing common state
/// All windows opened within a session can share InstancePerMatchingLifetimeScope services
/// Session scope is parent for all window scopes in the session
/// </summary>
public interface IWorkflowSession : IDisposable
{
    /// <summary>
    /// Unique identifier for this session
    /// </summary>
    Guid SessionId { get; }
    
    /// <summary>
    /// Session tag for debugging
    /// </summary>
    string SessionTag { get; }
    
    /// <summary>
    /// Checks if session is active
    /// </summary>
    bool IsActive { get; }
    
    // ========== OPEN WINDOW IN SESSION ==========
    
    /// <summary>
    /// Opens window within this session
    /// Window will share session-scoped services
    /// </summary>
    Guid OpenWindow<TViewModel>() where TViewModel : IViewModel;
    
    /// <summary>
    /// Opens window within this session with options
    /// </summary>
    Guid OpenWindow<TViewModel, TOptions>(TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;
    
    // ========== OPEN CHILD WINDOW IN SESSION ==========
    
    /// <summary>
    /// Opens child window within this session
    /// Child is attached to parent window
    /// </summary>
    Guid OpenChildWindow<TViewModel>(Guid parentWindowId)
        where TViewModel : IViewModel;
    
    /// <summary>
    /// Opens child window within this session with options
    /// </summary>
    Guid OpenChildWindow<TViewModel, TOptions>(Guid parentWindowId, TOptions options)
        where TViewModel : IViewModel
        where TOptions : IVmParameters;
    
    // ========== SESSION CONTROL ==========
    
    /// <summary>
    /// Closes session and all its windows
    /// </summary>
    void Close();
    
    /// <summary>
    /// Event raised when session is closed
    /// </summary>
    event EventHandler? SessionClosed;
}

/// <summary>
/// Factory for creating workflow sessions
/// </summary>
public interface IWorkflowSessionFactory
{
    /// <summary>
    /// Creates new workflow session
    /// </summary>
    /// <param name="sessionTag">Optional tag for session identification</param>
    IWorkflowSession CreateSession(string? sessionTag = null);
}

