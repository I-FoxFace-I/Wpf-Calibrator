using Autofac;
using WpfEngine.Data.Abstract;
using WpfEngine.ViewModels;

namespace WpfEngine.Services.Sessions;

/// <summary>
/// Base session builder interface for fluent configuration
/// </summary>
public interface ISessionBuilder : IDisposable
{
    // ========== MODULE REGISTRATION ==========
    
    /// <summary>
    /// Register a session module with custom service registrations
    /// Use when you need complex setup beyond scope tags
    /// </summary>
    /// <typeparam name="TModule">Module type</typeparam>
    ISessionBuilder WithModule<TModule>() where TModule : ISessionModule, new();
    
    /// <summary>
    /// Register services using custom configuration action
    /// </summary>
    /// <param name="configure">Configuration action</param>
    ISessionBuilder WithModule(Action<ContainerBuilder> configure);
    
    // ========== SERVICE DECLARATION ==========
    
    /// <summary>
    /// Declare a service to be resolved in Execute methods
    /// Service must be registered in central modules with matching scope tag
    /// </summary>
    /// <typeparam name="T1">Service type to declare</typeparam>
    /// <returns>Typed session builder with one declared service</returns>
    ISessionBuilder<T1> WithService<T1>() where T1 : notnull;
    
    // ========== LIFECYCLE CONFIGURATION ==========
    
    /// <summary>
    /// Auto-save database changes on dispose (for database sessions)
    /// </summary>
    /// <param name="enabled">Whether to enable auto-save</param>
    ISessionBuilder WithAutoSave(bool enabled = true);
    
    /// <summary>
    /// Execute action on dispose
    /// </summary>
    /// <param name="hook">Action to execute on dispose</param>
    ISessionBuilder OnDispose(Action? hook);
    
    /// <summary>
    /// Auto-close session when all windows are closed
    /// </summary>
    ISessionBuilder AutoCloseWhenEmpty();
    
    // ========== BUILD ==========
    
    /// <summary>
    /// Build the session and return it for manual control
    /// Use with 'using' statement for proper disposal
    /// </summary>
    IScopeSession Build();
    
    // ========== WINDOW MANAGEMENT ==========
    
    /// <summary>
    /// Build session and open window in it
    /// Returns built session for further control
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel type</typeparam>
    IScopeSession OpenWindow<TViewModel>() where TViewModel : IViewModel;
    
    /// <summary>
    /// Build session and open window with parameters
    /// Returns built session for further control
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel type</typeparam>
    /// <typeparam name="TParameters">Parameters type</typeparam>
    IScopeSession OpenWindow<TViewModel, TParameters>(TParameters parameters) 
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;
}

