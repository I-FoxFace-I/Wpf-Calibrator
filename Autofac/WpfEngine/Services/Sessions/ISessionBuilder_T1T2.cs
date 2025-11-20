namespace WpfEngine.Services.Sessions;

/// <summary>
/// Session builder with two declared services
/// </summary>
/// <typeparam name="T1">First service type</typeparam>
/// <typeparam name="T2">Second service type</typeparam>
public interface ISessionBuilder<T1, T2> : ISessionBuilder
    where T1 : notnull
    where T2 : notnull
{
    // ========== SERVICE DECLARATION ==========
    
    /// <summary>
    /// Declare additional service
    /// </summary>
    /// <typeparam name="T3">Third service type</typeparam>
    new ISessionBuilder<T1, T2, T3> WithService<T3>() where T3 : notnull;
    
    // ========== EXECUTE ==========
    
    /// <summary>
    /// Execute synchronous action with resolved services, then dispose
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    void Execute(Action<T1, T2> action, Action<Exception>? onError = null);
    
    /// <summary>
    /// Execute asynchronous action with resolved services, then dispose
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    Task ExecuteAsync(Func<T1, T2, Task> action, Action<Exception>? onError = null);
    
    // ========== EXECUTE WITH RESULT ==========
    
    /// <summary>
    /// Execute function with resolved services and return result, then dispose
    /// </summary>
    /// <param name="func">Function to execute</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    TResult ExecuteWithResult<TResult>(Func<T1, T2, TResult> func, TResult defaultValue = default!, Action<Exception>? onError = null);
    
    /// <summary>
    /// Execute async function with resolved services and return result, then dispose
    /// </summary>
    /// <param name="func">Function to execute</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    Task<TResult> ExecuteWithResultAsync<TResult>(Func<T1, T2, Task<TResult>> func, TResult defaultValue = default!, Action<Exception>? onError = null);
}

