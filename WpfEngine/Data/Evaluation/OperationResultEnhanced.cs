using System;
using System.Threading.Tasks;

namespace WpfEngine.Data.Evaluation;

/// <summary>
/// Non-generic enhanced result for void operations
/// </summary>
public class OperationResultEnhanced
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    
    private OperationResultEnhanced(bool isSuccess, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    // ========== FACTORY METHODS ==========
    
    public static OperationResultEnhanced Success() 
        => new(true);
    
    public static OperationResultEnhanced Failure(string errorMessage) 
        => new(false, errorMessage);
    
    public static OperationResultEnhanced Failure(Exception exception) 
        => new(false, exception.Message, exception);
    
    public static OperationResultEnhanced Failure(string errorMessage, Exception exception) 
        => new(false, errorMessage, exception);
    
    // ========== PATTERN MATCHING ==========
    
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<string?, Exception?, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(ErrorMessage, Exception);
    }
    
    public async Task<TResult> MatchAsync<TResult>(
        Func<Task<TResult>> onSuccess,
        Func<string?, Exception?, Task<TResult>> onFailure)
    {
        return IsSuccess ? await onSuccess() : await onFailure(ErrorMessage, Exception);
    }
    
    public void Match(
        Action onSuccess,
        Action<string?, Exception?> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(ErrorMessage, Exception);
    }
    
    // ========== SIDE EFFECTS ==========
    
    public OperationResultEnhanced OnSuccess(Action action)
    {
        if (IsSuccess) action();
        return this;
    }
    
    public async Task<OperationResultEnhanced> OnSuccessAsync(Func<Task> action)
    {
        if (IsSuccess) await action();
        return this;
    }
    
    public OperationResultEnhanced OnFailure(Action<string?, Exception?> action)
    {
        if (!IsSuccess) action(ErrorMessage, Exception);
        return this;
    }
    
    public async Task<OperationResultEnhanced> OnFailureAsync(Func<string?, Exception?, Task> action)
    {
        if (!IsSuccess) await action(ErrorMessage, Exception);
        return this;
    }
    
    // ========== COMBINATION ==========
    
    public static OperationResultEnhanced Combine(params OperationResultEnhanced[] results)
    {
        if (results == null || results.Length == 0)
            return Success();
        
        foreach (var result in results)
        {
            if (!result.IsSuccess)
                return result;
        }
        
        return Success();
    }
    
    // ========== CONVERSION ==========
    
    public OperationResult ToOperationResult()
    {
        return IsSuccess 
            ? OperationResult.Success()
            : OperationResult.Failure(ErrorMessage ?? "Operation failed", Exception!);
    }
    
    public static OperationResultEnhanced FromOperationResult(OperationResult result)
    {
        return result.IsSuccess
            ? Success()
            : Failure(result.ErrorMessage ?? "Operation failed", result.Exception!);
    }
    
    // ========== TRADITIONAL METHODS ==========
    
    public void ThrowIfFailed()
    {
        if (!IsSuccess)
        {
            throw Exception ?? new InvalidOperationException(ErrorMessage ?? "Operation failed");
        }
    }
    
    public static implicit operator bool(OperationResultEnhanced result)
        => result.IsSuccess;
}


