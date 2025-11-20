using System;
using System.CodeDom;
using System.Configuration;
using WpfEngine.ViewModels;
using WpfEngine.Data.Abstract;
using WpfEngine.Enums;

namespace WpfEngine.Data.Dialogs;

/// <summary>
/// Simple dialog result without data
/// </summary>
public record DialogResult : BaseResult, IDialogResult<DialogResult>
{
    public bool Result { get; private set; }


    private DialogResult(DialogStatus dialogStatus, bool? result = null, string? errorMessage = null)
    {
        Status = dialogStatus;
        Result = result ?? false;
        ErrorMessage = errorMessage;
    }

    public static DialogResult Success(bool data) => new(DialogStatus.Success, data);
    public static DialogResult Success() => new(DialogStatus.Success, true);
    public static DialogResult Cancel() => new(DialogStatus.Cancel, false);
    public static DialogResult Yes() => new(DialogStatus.Success, true);
    public static DialogResult No() => new(DialogStatus.Success, false);
    public static DialogResult Error(string? message) => new(DialogStatus.Error, errorMessage: message);
}
