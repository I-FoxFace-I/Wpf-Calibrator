using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Dialogs.Events;
using WpfEngine.ViewModels;

namespace WpfEngine.Services;

/// <summary>
/// Dialog service for managing modal dialogs with support for nested dialogs and complex results
/// Similar to IWindowContext but for modal dialogs
/// </summary>
public interface IDialogService
{
    // ========== EVENTS ==========

    /// <summary>
    /// Raised when any dialog is opened
    /// </summary>
    event EventHandler<DialogOpenedEventArgs>? DialogOpened;

    /// <summary>
    /// Raised when any dialog is closed
    /// </summary>
    event EventHandler<DialogClosedEventArgs>? DialogClosed;

    /// <summary>
    /// Raised when a child dialog is closed
    /// </summary>
    event EventHandler<ChildDialogClosedEventArgs>? ChildDialogClosed;

    // ========== STATE ==========

    /// <summary>
    /// Currently active dialog stack (for nested dialogs)
    /// </summary>
    IReadOnlyList<Guid> ActiveDialogStack { get; }

    /// <summary>
    /// Current top-most dialog ID
    /// </summary>
    Guid? CurrentDialogId { get; }

    /// <summary>
    /// Check if any dialog is currently open
    /// </summary>
    bool HasActiveDialog { get; }

    // ========== SHOW DIALOG METHODS ==========

    /// <summary>
    /// Shows modal dialog and returns simple result (OK/Cancel)
    /// </summary>
    Task<DialogResult> ShowDialogAsync<TViewModel>()
        where TViewModel : IViewModel;

    /// <summary>
    /// Shows modal dialog with parameters and returns simple result
    /// </summary>
    Task<DialogResult> ShowDialogAsync<TViewModel, TParameters>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters;

    /// <summary>
    /// Shows modal dialog and returns strongly-typed result
    /// </summary>
    Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TResult>()
        where TViewModel : IViewModel
        where TResult : class, IDialogResult;

    /// <summary>
    /// Shows modal dialog with parameters and returns strongly-typed result
    /// </summary>
    Task<DialogResult<TResult>> ShowDialogAsync<TViewModel, TParameters, TResult>(TParameters parameters)
        where TViewModel : IViewModel
        where TParameters : IViewModelParameters
        where TResult : class, IDialogResult;

    // ========== MESSAGE BOX DIALOGS ==========

    /// <summary>
    /// Shows simple message box dialog
    /// </summary>
    Task<MessageBoxResult> ShowMessageAsync(
        string message,
        string title = "Message",
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None);

    /// <summary>
    /// Shows error dialog
    /// </summary>
    Task ShowErrorAsync(string message, string title = "Error", Exception? exception = null);

    /// <summary>
    /// Shows confirmation dialog
    /// </summary>
    Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");

    // ========== DIALOG MANAGEMENT ==========

    /// <summary>
    /// Closes specific dialog with result
    /// </summary>
    void CloseDialog(Guid dialogId, IDialogResult? result = null);

    /// <summary>
    /// Closes current top-most dialog
    /// </summary>
    void CloseCurrentDialog(IDialogResult? result = null);

    /// <summary>
    /// Closes all open dialogs (cleanup)
    /// </summary>
    void CloseAllDialogs();

    /// <summary>
    /// Gets child dialog IDs for specific parent dialog
    /// </summary>
    IReadOnlyList<Guid> GetChildDialogIds(Guid parentDialogId);

    /// <summary>
    /// Check if specific dialog is open
    /// </summary>
    bool IsDialogOpen(Guid dialogId);
}
