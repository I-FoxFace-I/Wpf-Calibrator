using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Data.Abstract;
using WpfEngine.Enums;
using WpfEngine.Services;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.ViewModels.Dialogs;

/// <summary>
/// Base class for dialog ViewModels without parameters or result
/// Provides CloseDialog() method for self-closing
/// </summary>
public abstract partial class DialogViewModel : BaseViewModel, IDialogViewModel
{
    public virtual DialogStatus Status { get; protected set; }
    public virtual bool IsCompleted { get; protected set; }
    public virtual bool IsCancelled { get; protected set; }

    protected readonly IDialogHost DialogHost;
    public Guid DialogId { get; private set; }

    protected DialogViewModel(ILogger<DialogViewModel> logger, IDialogHost dialogHost) : base(logger)
    {
        DialogHost = dialogHost;
        Status = DialogStatus.Pending;
        DialogId = dialogHost.DialogId;
    }

    protected virtual bool CanCompleteDialog() => true;

    /// <summary>
    /// Closes the dialog with OK result
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCompleteDialog))]
    protected abstract Task CompleteDialogAsync();


    /// <summary>
    /// Cancels the dialog (closes with false result)
    /// </summary>
    [RelayCommand]
    protected abstract Task CancelDialogAsync();


    /// <summary>
    /// Procedure when dialog closes with the OK result
    /// </summary>
    protected virtual void OnCancel()
    {
        IsCompleted = false;
        IsCancelled = true;
        Status = DialogStatus.Cancel;
    }

    /// <summary>
    /// Procedure when dialog is closed by cancellation
    /// </summary>
    protected virtual void OnComplete()
    {
        IsCompleted = true;
        IsCancelled = false;
        Status = DialogStatus.Success;
    }

    /// <summary>
    /// Closes the dialog with specified result
    /// </summary>
    protected virtual void CloseDialogWindow(object? dialogResult)
    {
        Logger.LogInformation("[DIALOG_VM] Closing dialog with status : {Status} and result : {Result}", Status, dialogResult);
        DialogHost.CloseDialog();
    }
}

/// <summary>
/// Base class for dialog ViewModels with parameters but no result
/// </summary>
public abstract partial class DialogViewModel<TParams> : DialogViewModel, IViewModel<TParams>
    where TParams : IViewModelParameters
{
    protected DialogViewModel(ILogger<DialogViewModel<TParams>> logger,
        IDialogHost dialogHost,
        TParams parameter) : base(logger, dialogHost)
    {
        _parameter = parameter;
    }

    public virtual Task InitializeAsync(TParams parameter) 
    { 
        return Task.CompletedTask;
    }

    [ObservableProperty]
    private TParams? _parameter;

    /// <summary>
    /// Initialize ViewModel with parameters
    /// Called after construction by factory
    /// </summary>
    public virtual Task InitializeAsync(TParams parameter, CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("[{ViewModelType}] Initialized with parameters (CorrelationId: {CorrelationId})", GetType().Name, parameter.CorrelationId);

        Parameter = parameter;

        return Task.CompletedTask;
    }
}
