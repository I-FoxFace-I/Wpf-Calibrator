using Microsoft.Extensions.Logging;
using WpfEngine.ViewModels;
using WpfEngine.Data.Abstract;
using WpfEngine.Services;

namespace WpfEngine.ViewModels.Dialogs;

/// <summary>
/// Base class for dialog ViewModels with parameters and result
/// Override GetDialogResult() to return custom result
/// </summary>
public abstract partial class ResultDialogViewModel<TResult> : DialogViewModel, IViewModel, IDialogViewModel, IResultDialogViewModel<TResult>
    where TResult : class
{
    protected ResultDialogViewModel(ILogger<ResultDialogViewModel<TResult>> logger,
        IDialogHost dialogHost)
        : base(logger, dialogHost)
    {

    }

    /// <summary>
    /// Dialog result - override GetDialogResult() to set this
    /// </summary>
    public TResult? ResultData { get; protected set; }


    /// <summary>
    /// Override this to create result when dialog is closed with OK
    /// </summary>
    protected abstract TResult? CreateResult();

    /// <summary>
    /// Procedure when dialog closes with the OK result
    /// </summary>
    protected override void OnCancel()
    {
        OnCancel();
        ResultData = null;
    }

    /// <summary>
    /// Procedure when dialog is closed by cancellation
    /// </summary>
    protected override void OnComplete()
    {
        OnComplete();
        ResultData = CreateResult();
    }

    protected override sealed void CloseDialogWindow(object? dialogResult)
    {
        base.CloseDialogWindow(dialogResult);
    }

    /// <summary>
    /// Closes the dialog with specified WPF DialogResult
    /// </summary>
    protected virtual void CloseDialogWindow(TResult? dialogResult)
    {
        base.CloseDialogWindow(dialogResult);
    }
}


/// <summary>
/// Base class for dialog ViewModels with parameters and result
/// Override GetDialogResult() to return custom result
/// </summary>
public abstract partial class ResultDialogViewModel<TParams, TResult> : DialogViewModel<TParams>, IViewModel, IDialogViewModel<TParams>, IResultDialogViewModel<TResult>
    where TParams : IViewModelParameters
    where TResult : class
{
    protected ResultDialogViewModel(ILogger<ResultDialogViewModel<TParams, TResult>> logger,
        IDialogHost dialogHost,
        TParams parameter)
        : base(logger, dialogHost, parameter)
    {

    }

    /// <summary>
    /// Dialog result - override GetDialogResult() to set this
    /// </summary>
    public TResult? ResultData { get; protected set; }


    /// <summary>
    /// Override this to create result when dialog is closed with OK
    /// </summary>
    protected abstract TResult? CreateResult();

    /// <summary>
    /// Procedure when dialog closes with the OK result
    /// </summary>
    protected override void OnCancel()
    {
        base.OnCancel();
        ResultData = null;
    }

    /// <summary>
    /// Procedure when dialog is closed by cancellation
    /// </summary>
    protected override void OnComplete()
    {
        base.OnComplete();
        ResultData = CreateResult();
    }

    protected override sealed void CloseDialogWindow(object? dialogResult)
    {
        base.CloseDialogWindow(dialogResult);
    }

    /// <summary>
    /// Closes the dialog with specified WPF DialogResult
    /// </summary>
    protected virtual void CloseDialogWindow(TResult? dialogResult)
    {
        base.CloseDialogWindow(dialogResult);
    }
}
