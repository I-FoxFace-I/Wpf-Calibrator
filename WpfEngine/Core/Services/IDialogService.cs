using System.Threading.Tasks;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Services;

/// <summary>
/// Service for managing modal dialogs
/// </summary>
public interface IDialogService
{
    // ========== SHOW DIALOG ==========

    /// <summary>
    /// Shows modal dialog and waits for result
    /// </summary>
    Task<TResult?> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : IDialogViewModel
        where TResult : IVmResult;

    /// <summary>
    /// Shows modal dialog with options and waits for result
    /// </summary>
    Task<TResult?> ShowDialogAsync<TViewModel, TOptions, TResult>(TOptions options)
        where TViewModel : IViewModel, IDialogViewModel<TOptions, TResult>
        where TOptions : IVmParameters
        where TResult : IVmResult;

    // ========== COMMON DIALOGS ==========

    /// <summary>
    /// Shows message box
    /// </summary>
    Task<MessageBoxResult> ShowMessageBoxAsync(
        string message,
        string? title = null,
        MessageBoxType type = MessageBoxType.Information);

    /// <summary>
    /// Shows confirmation dialog
    /// </summary>
    Task<bool> ShowConfirmationAsync(
        string message,
        string? title = null,
        string confirmText = "OK",
        string cancelText = "Cancel");

    /// <summary>
    /// Shows error dialog
    /// </summary>
    Task ShowErrorAsync(string errorMessage, string? title = null);

    /// <summary>
    /// Shows input dialog for text entry
    /// </summary>
    Task<string?> ShowInputAsync(
        string prompt,
        string? title = null,
        string? defaultValue = null);
}

// ========== ENUMS ==========

/// <summary>
/// Message box type
/// </summary>
public enum MessageBoxType
{
    Information,
    Warning,
    Error,
    Question
}

/// <summary>
/// Message box result
/// </summary>
public enum MessageBoxResult
{
    OK,
    Cancel,
    Yes,
    No
}
