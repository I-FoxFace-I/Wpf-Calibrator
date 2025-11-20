namespace WpfEngine.Services.Sessions;

/// <summary>
/// Session builder with four declared services (maximum)
/// </summary>
/// <typeparam name="T1">First service type</typeparam>
/// <typeparam name="T2">Second service type</typeparam>
/// <typeparam name="T3">Third service type</typeparam>
/// <typeparam name="T4">Fourth service type</typeparam>
public interface ISessionBuilder<T1, T2, T3, T4> : ISessionBuilder
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
{
    // No more WithService - 4 is the maximum for readability
    
    // ========== EXECUTE ==========
    
    /// <summary>
    /// Execute synchronous action with resolved services, then dispose
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    void Execute(Action<T1, T2, T3, T4> action, Action<Exception>? onError = null);
    
    /// <summary>
    /// Execute asynchronous action with resolved services, then dispose
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    Task ExecuteAsync(Func<T1, T2, T3, T4, Task> action, Action<Exception>? onError = null);
    
    // ========== EXECUTE WITH RESULT ==========
    
    /// <summary>
    /// Execute function with resolved services and return result, then dispose
    /// </summary>
    /// <param name="func">Function to execute</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    TResult ExecuteWithResult<TResult>(Func<T1, T2, T3, T4, TResult> func, TResult defaultValue = default!, Action<Exception>? onError = null);
    
    /// <summary>
    /// Execute async function with resolved services and return result, then dispose
    /// </summary>
    /// <param name="func">Function to execute</param>
    /// <param name="defaultValue">Default value to return if exception occurs</param>
    /// <param name="onError">Optional error handler. If not provided, defaults to Rollback() for database sessions</param>
    Task<TResult> ExecuteWithResultAsync<TResult>(Func<T1, T2, T3, T4, Task<TResult>> func, TResult defaultValue = default!, Action<Exception>? onError = null);
}

