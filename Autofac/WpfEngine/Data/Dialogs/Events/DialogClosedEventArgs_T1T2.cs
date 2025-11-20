using WpfEngine.Data.Abstract;

namespace WpfEngine.Data.Dialogs.Events;

public class DialogClosedEventArgs<TResult, TData> : EventArgs
    where TResult : class, IDialogResult<TResult, TData>
    where TData : notnull
{
    public Guid DialogId { get; protected set; }
    public TResult Result { get; protected set; }
    public Type ViewModelType { get; protected set; }

    protected DialogClosedEventArgs()
    {
        DialogId = Guid.Empty;
        ViewModelType = typeof(void);
        Result = TResult.Error("Dialog was not assined!");
    }

    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, string errorMessage = "")
    {
        DialogId = dialogId;
        ViewModelType = viewModelType;
        Result = TResult.Error(errorMessage);
    }

    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, TResult? result = null)
    {
        DialogId = dialogId;
        ViewModelType = viewModelType;
        Result = result ?? TResult.Cancel();
    }
}
