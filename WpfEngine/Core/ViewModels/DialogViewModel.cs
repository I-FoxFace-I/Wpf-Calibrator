using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

/// <summary>
/// Base ViewModel for dialogs without result
/// </summary>
public abstract partial class BaseDialogViewModel : BaseViewModel, IDialogViewModel
{
    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isCancelled;

    protected BaseDialogViewModel(ILogger logger) : base(logger)
    {
    }

    /// <summary>
    /// Completes dialog successfully
    /// </summary>
    protected void CompleteDialog()
    {
        IsCompleted = true;
        IsCancelled = false;
        Logger.LogInformation("[{ViewModelType}] Dialog completed", GetType().Name);
    }

    /// <summary>
    /// Cancels dialog
    /// </summary>
    protected void CancelDialog()
    {
        IsCompleted = false;
        IsCancelled = true;
        Logger.LogInformation("[{ViewModelType}] Dialog cancelled", GetType().Name);
    }
}

/// <summary>
/// Base Dialog ViewModel with parameters (no result)
/// </summary>
public abstract partial class BaseDialogViewModel<TParameter> : BaseViewModel<TParameter>, IDialogViewModel<TParameter>
    where TParameter : IVmParameters
{
    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isCancelled;

    protected BaseDialogViewModel(ILogger logger) : base(logger)
    {
    }

    protected void CompleteDialog()
    {
        IsCompleted = true;
        IsCancelled = false;
        Logger.LogInformation("[{ViewModelType}] Dialog completed", GetType().Name);
    }

    protected void CancelDialog()
    {
        IsCompleted = false;
        IsCancelled = true;
        Logger.LogInformation("[{ViewModelType}] Dialog cancelled", GetType().Name);
    }
}

/// <summary>
/// Base Dialog ViewModel with parameters and result
/// </summary>
public abstract partial class BaseDialogViewModel<TParameter, TResult> : BaseViewModel<TParameter>, IDialogViewModel<TParameter, TResult>
    where TParameter : IVmParameters
    where TResult : IVmResult
{
    //[ObservableProperty]
    //private TResult? _dialogResult;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _isCancelled;

    protected BaseDialogViewModel(ILogger logger) : base(logger)
    {
    }

    public virtual TResult? DialogResult { get; protected set; }

    protected abstract TResult? GetDialogResult();

    /// <summary>
    /// Completes dialog with result
    /// </summary>
    protected void CompleteDialog(TResult result)
    {
        DialogResult = result;
        IsCompleted = true;
        IsCancelled = false;
        Logger.LogInformation("[{ViewModelType}] Dialog completed with result", GetType().Name);
    }

    /// <summary>
    /// Cancels dialog without result
    /// </summary>
    protected void CancelDialog()
    {
        DialogResult = default;
        IsCompleted = false;
        IsCancelled = true;
        Logger.LogInformation("[{ViewModelType}] Dialog cancelled", GetType().Name);
    }
}


