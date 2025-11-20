namespace WpfEngine.Data.Evaluation;

/// <summary>
/// Result pattern for operations that can fail
/// </summary>
public class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    
    private OperationResult(bool isSuccess, T? value = default, string? errorMessage = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }
    
    // Factory methods
    public static OperationResult<T> Success(T value) 
        => new(true, value);
    
    public static OperationResult<T> Failure(string errorMessage) 
        => new(false, errorMessage: errorMessage);
    
    public static OperationResult<T> Failure(Exception exception) 
        => new(false, errorMessage: exception.Message, exception: exception);
    
    public static OperationResult<T> Failure(string errorMessage, Exception exception) 
        => new(false, errorMessage: errorMessage, exception: exception);
    
    // Implicit conversion from value
    public static implicit operator OperationResult<T>(T value) 
        => Success(value);
    
    // Method to throw if operation failed
    public T GetValueOrThrow()
    {
        if (!IsSuccess)
        {
            throw Exception ?? new InvalidOperationException(ErrorMessage ?? "Operation failed");
        }
        return Value!;
    }
    
    // Try get value
    public bool TryGetValue(out T? value)
    {
        value = Value;
        return IsSuccess;
    }
}
