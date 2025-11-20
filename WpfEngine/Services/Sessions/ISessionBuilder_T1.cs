namespace WpfEngine.Services.Sessions;

/// <summary>
/// Session builder with one declared service
/// </summary>
/// <typeparam name="T1">First service type</typeparam>
public interface ISessionBuilder<T1> : ISessionBuilder
    where T1 : notnull
{
    // ========== SERVICE DECLARATION ==========
    
    /// <summary>
    /// Declare additional service
    /// </summary>
    /// <typeparam name="T2">Second service type</typeparam>
    new ISessionBuilder<T1, T2> WithService<T2>() where T2 : notnull;
    
    // ========== EXECUTE ==========
    
    /// <summary>
    /// Execute synchronous action with resolved service, then dispose
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    void Execute(Action<T1> action, Action<Exception>? onError = null);
    
    /// <summary>
    /// Execute asynchronous action with resolved service, then dispose
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    Task ExecuteAsync(Func<T1, Task> action, Action<Exception>? onError = null);
    
    // ========== EXECUTE WITH RESULT ==========
    
    /// <summary>
    /// Execute function with resolved service and return result, then dispose
    /// </summary>
    /// <param name="func">Function to execute</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    TResult ExecuteWithResult<TResult>(Func<T1, TResult> func, TResult defaultValue = default!, Action<Exception>? onError = null);
    
    /// <summary>
    /// Execute async function with resolved service and return result, then dispose
    /// </summary>
    /// <param name="func">Function to execute</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    Task<TResult> ExecuteWithResultAsync<TResult>(Func<T1, Task<TResult>> func, TResult defaultValue = default!, Action<Exception>? onError = null);
}

