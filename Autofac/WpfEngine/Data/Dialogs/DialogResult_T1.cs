using WpfEngine.Data.Abstract;
using WpfEngine.Enums;

namespace WpfEngine.Data.Dialogs;

/// <summary>
/// Generic result container for dialogs
/// </summary>
public record DialogResult<TResult> : BaseResult, IDialogResult<DialogResult<TResult>, TResult>
    where TResult : class
{
    public TResult? Result { get; init; }

    public DialogResult()
    {
        Result = null;
        ErrorMessage = string.Empty;
    }

    private DialogResult(DialogStatus dialogStatus, TResult? data = null, string? errorMessage = null)
    {
        Result = data;
        Status = dialogStatus;
        ErrorMessage = errorMessage;
    }

    // Factory methods for creating results
    public static DialogResult<TResult> Success(TResult? data) => new(DialogStatus.Success, data);
    public static DialogResult<TResult> Success() => new(DialogStatus.Success);

    public static DialogResult<TResult> Cancel() => new(DialogStatus.Cancel);

    public static DialogResult<TResult> Error(string? errorMessage) => new(DialogStatus.Error, errorMessage: errorMessage);
}
