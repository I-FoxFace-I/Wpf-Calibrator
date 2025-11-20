using Autofac;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Sessions;
using WpfEngine.Services.Sessions;
using WpfEngine.ViewModels;

namespace WpfEngine.Services;

/// <summary>
/// Represents an active scope session with its own DI lifetime scope
/// </summary>
public interface IScopeSession : IDisposable, IAsyncDisposable
{
    // ========== SESSION INFO ==========
    
    /// <summary>
    /// Unique session identifier
    /// </summary>
    Guid SessionId { get; }

    /// <summary>
    /// Unique identifier of parent session if exists.
    /// </summary>
    Guid? ParentId { get; }

    /// <summary>
    /// Session scope tag
    /// </summary>
    ScopeTag Tag { get; }
    
    /// <summary>
    /// The DI lifetime scope for this session
    /// </summary>
    ILifetimeScope Scope { get; }
    
    /// <summary>
    /// Parent session (if this is a child session)
    /// </summary>
    IScopeSession? Parent { get; }
    
    /// <summary>
    /// Whether this session is still active
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Number of windows opened in this session
    /// </summary>
    int WindowCount { get; }
    
    // ========== CHILD SESSION CREATION ==========
    
    /// <summary>
    /// Create a child session builder for fluent configuration
    /// </summary>
    /// <param name="tag">Scope tag for the child session</param>
    /// <returns>Session builder for fluent configuration</returns>
    ISessionBuilder CreateChild(ScopeTag tag);
    
    // ========== SERVICE RESOLUTION ==========
    
    /// <summary>
    /// Resolve service from session scope
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <returns>Resolved service instance</returns>
    TService Resolve<TService>() where TService : notnull;
    
    /// <summary>
    /// Try to resolve service from session scope
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <param name="service">Resolved service if successful</param>
    /// <returns>True if service was resolved successfully</returns>
    bool TryResolve<TService>(out TService? service) where TService : class;
    
    // ========== DATABASE OPERATIONS (if DB session) ==========
    
    /// <summary>
    /// Save all pending changes (for database sessions)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    Task SaveChangesAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Rollback changes (for database sessions)
    /// </summary>
    void Rollback();
    
    // ========== WINDOW MANAGEMENT ==========
    
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
    
    // ========== LIFECYCLE ==========
    
    /// <summary>
    /// Close this session and all child sessions
    /// </summary>
    void Close();
    
    /// <summary>
    /// Close this session asynchronously
    /// </summary>
    Task CloseAsync();
    
    /// <summary>
    /// Raised when the session is closed
    /// </summary>
    event EventHandler? Closed;
}

