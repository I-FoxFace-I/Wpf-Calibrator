namespace WpfEngine.Data.Dialogs.Events;

/// <summary>
/// Event args for dialog closed events with result
/// </summary>
public class DialogClosedEventArgs<TResult> : DialogClosedEventArgs<DialogResult<TResult>, TResult>
    where TResult : class
{
    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, DialogResult<TResult>? result = null)
        : base(dialogId, viewModelType, result)
    {

    }

    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, string errorMessage = "")
        : base(dialogId, viewModelType, errorMessage)
    {

    }

    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, TResult? result = null)
    {
        DialogId = dialogId;
        ViewModelType = viewModelType;
        Result = result is null ? DialogResult<TResult>.Success(result) : DialogResult<TResult>.Cancel();
    }
}