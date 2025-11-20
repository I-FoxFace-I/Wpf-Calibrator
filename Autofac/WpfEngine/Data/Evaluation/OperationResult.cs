using System;

namespace WpfEngine.Data.Evaluation;

/// <summary>
/// Non-generic result for void operations
/// </summary>
public class OperationResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    
    private OperationResult(bool isSuccess, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    public static OperationResult Success() 
        => new(true);
    
    public static OperationResult Failure(string errorMessage) 
        => new(false, errorMessage);
    
    public static OperationResult Failure(Exception exception) 
        => new(false, exception.Message, exception);
    
    public static OperationResult Failure(string errorMessage, Exception exception) 
        => new(false, errorMessage, exception);
    
    public void ThrowIfFailed()
    {
        if (!IsSuccess)
        {
            throw Exception ?? new InvalidOperationException(ErrorMessage ?? "Operation failed");
        }
    }
}