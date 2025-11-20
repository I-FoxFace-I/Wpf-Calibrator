namespace WpfEngine.Data.Evaluation;

/// <summary>
/// Enhanced result pattern with pattern matching and LINQ-style operations
/// USAGE: Use this for new code, OperationResult stays for backward compatibility
/// </summary>
public class OperationResultEnhanced<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    
    private OperationResultEnhanced(bool isSuccess, T? value = default, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    // ========== FACTORY METHODS ==========
    
    public static OperationResultEnhanced<T> Success(T value) 
        => new(true, value);
    
    public static OperationResultEnhanced<T> Failure(string errorMessage) 
        => new(false, errorMessage: errorMessage);
    
    public static OperationResultEnhanced<T> Failure(Exception exception) 
        => new(false, errorMessage: exception.Message, exception: exception);
    
    public static OperationResultEnhanced<T> Failure(string errorMessage, Exception exception) 
        => new(false, errorMessage: errorMessage, exception: exception);
    
    // ========== PATTERN MATCHING ==========
    
    /// <summary>
    /// Pattern matching - execute different logic based on success/failure
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string?, Exception?, TResult> onFailure)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
        
        return IsSuccess 
            ? onSuccess(Value!) 
            : onFailure(ErrorMessage, Exception);
    }
    
    /// <summary>
    /// Async pattern matching
    /// </summary>
    public async Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<string?, Exception?, Task<TResult>> onFailure)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
        
        return IsSuccess 
            ? await onSuccess(Value!) 
            : await onFailure(ErrorMessage, Exception);
    }
    
    /// <summary>
    /// Execute action without returning value
    /// </summary>
    public void Match(
        Action<T> onSuccess,
        Action<string?, Exception?> onFailure)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));
        
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(ErrorMessage, Exception);
    }
    
    // ========== LINQ-STYLE OPERATIONS ==========
    
    /// <summary>
    /// Transform success value (functor map)
    /// </summary>
    public OperationResultEnhanced<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        if (mapper == null) throw new ArgumentNullException(nameof(mapper));
        
        return IsSuccess 
            ? OperationResultEnhanced<TOut>.Success(mapper(Value!))
            : OperationResultEnhanced<TOut>.Failure(ErrorMessage ?? "Operation failed", Exception!);
    }
    
    /// <summary>
    /// Async transform
    /// </summary>
    public async Task<OperationResultEnhanced<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> mapper)
    {
        if (mapper == null) throw new ArgumentNullException(nameof(mapper));
        
        return IsSuccess 
            ? OperationResultEnhanced<TOut>.Success(await mapper(Value!))
            : OperationResultEnhanced<TOut>.Failure(ErrorMessage ?? "Operation failed", Exception!);
    }
    
    /// <summary>
    /// Chain operations (monad bind/flatMap)
    /// </summary>
    public OperationResultEnhanced<TOut> Bind<TOut>(Func<T, OperationResultEnhanced<TOut>> binder)
    {
        if (binder == null) throw new ArgumentNullException(nameof(binder));
        
        return IsSuccess 
            ? binder(Value!)
            : OperationResultEnhanced<TOut>.Failure(ErrorMessage ?? "Operation failed", Exception!);
    }
    
    /// <summary>
    /// Async chain operations
    /// </summary>
    public async Task<OperationResultEnhanced<TOut>> BindAsync<TOut>(Func<T, Task<OperationResultEnhanced<TOut>>> binder)
    {
        if (binder == null) throw new ArgumentNullException(nameof(binder));
        
        return IsSuccess 
            ? await binder(Value!)
            : OperationResultEnhanced<TOut>.Failure(ErrorMessage ?? "Operation failed", Exception!);
    }
    
    // ========== SIDE EFFECTS ==========
    
    /// <summary>
    /// Execute action on success, returns same result
    /// </summary>
    public OperationResultEnhanced<T> OnSuccess(Action<T> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        if (IsSuccess)
            action(Value!);
        
        return this;
    }
    
    /// <summary>
    /// Async execute action on success
    /// </summary>
    public async Task<OperationResultEnhanced<T>> OnSuccessAsync(Func<T, Task> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        if (IsSuccess)
            await action(Value!);
        
        return this;
    }
    
    /// <summary>
    /// Execute action on failure, returns same result
    /// </summary>
    public OperationResultEnhanced<T> OnFailure(Action<string?, Exception?> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        if (!IsSuccess)
            action(ErrorMessage, Exception);
        
        return this;
    }
    
    /// <summary>
    /// Async execute action on failure
    /// </summary>
    public async Task<OperationResultEnhanced<T>> OnFailureAsync(Func<string?, Exception?, Task> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        
        if (!IsSuccess)
            await action(ErrorMessage, Exception);
        
        return this;
    }
    
    // ========== COMBINATION ==========
    
    /// <summary>
    /// Combine multiple results - all must succeed
    /// </summary>
    public static OperationResultEnhanced<T[]> Combine(params OperationResultEnhanced<T>[] results)
    {
        if (results == null || results.Length == 0)
            return OperationResultEnhanced<T[]>.Success(Array.Empty<T>());
        
        var values = new T[results.Length];
        
        for (int i = 0; i < results.Length; i++)
        {
            if (!results[i].IsSuccess)
            {
                return OperationResultEnhanced<T[]>.Failure(
                    $"Operation {i + 1} failed: {results[i].ErrorMessage}",
                    results[i].Exception!);
            }
            values[i] = results[i].Value!;
        }
        
        return OperationResultEnhanced<T[]>.Success(values);
    }
    
    /// <summary>
    /// Combine multiple results - returns first success or last failure
    /// </summary>
    public static OperationResultEnhanced<T> FirstSuccess(params OperationResultEnhanced<T>[] results)
    {
        if (results == null || results.Length == 0)
            return OperationResultEnhanced<T>.Failure("No results to combine");
        
        foreach (var result in results)
        {
            if (result.IsSuccess)
                return result;
        }
        
        // Return last failure
        return results[^1];
    }
    
    // ========== CONVERSION ==========
    
    /// <summary>
    /// Convert to original OperationResult (for backward compatibility)
    /// </summary>
    public OperationResult<T> ToOperationResult()
    {
        return IsSuccess 
            ? OperationResult<T>.Success(Value!)
            : OperationResult<T>.Failure(ErrorMessage ?? "Operation failed", Exception!);
    }
    
    /// <summary>
    /// Create from original OperationResult
    /// </summary>
    public static OperationResultEnhanced<T> FromOperationResult(OperationResult<T> result)
    {
        return result.IsSuccess
            ? Success(result.Value!)
            : Failure(result.ErrorMessage ?? "Operation failed", result.Exception!);
    }
    
    // ========== IMPLICIT CONVERSIONS ==========
    
    public static implicit operator OperationResultEnhanced<T>(T value) 
        => Success(value);
    
    public static implicit operator bool(OperationResultEnhanced<T> result)
        => result.IsSuccess;
    
    // ========== TRADITIONAL METHODS ==========
    
    public T GetValueOrThrow()
    {
        if (!IsSuccess)
        {
            throw Exception ?? new InvalidOperationException(ErrorMessage ?? "Operation failed");
        }
        return Value!;
    }
    
    public bool TryGetValue(out T? value)
    {
        value = Value;
        return IsSuccess;
    }
    
    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? Value! : defaultValue;
    }
}


