using WpfEngine.Enums;

namespace WpfEngine.Data.Abstract;

public interface IDialogResult
{
    /// <summary>
    /// Key identifying the result.
    /// </summary>
    Guid Key { get; }

    /// <summary>
    /// Defines the result status of dialog opperation.
    /// </summary>
    DialogStatus Status { get; }

    /// <summary>
    /// Indicates if the operation was completed (Success or Cancel)
    /// </summary>
    bool IsComplete { get; }

    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Indicates if the operation was cancelled.
    /// </summary>
    bool IsCancelled { get; }

    /// <summary>
    /// Optional error message if operation failed.
    /// </summary>
    string? ErrorMessage { get; }
}


/// <summary>
/// Base interface for ViewModel results
/// Used when ViewModels need to return data (e.g., dialogs)
/// </summary>
public interface IDialogResult<TResult, TData> : IDialogResult
    where TResult : IDialogResult<TResult, TData>
    where TData : notnull
{
    TData? Result { get; }
    public static abstract TResult Success(TData? data);
    public static abstract TResult Success();
    public static abstract TResult Cancel();
    public static abstract TResult Error(string? errorMessage);
}

public interface IDialogResult<TResult> : IDialogResult<TResult, bool>
    where TResult : IDialogResult<TResult, bool>
{

}