using WpfEngine.Data.Abstract;

namespace WpfEngine.Data.Dialogs.Events;

public class DialogClosedEventArgs : EventArgs
{
    public Guid DialogId { get; protected set; }
    public IDialogResult Result { get; protected set; }
    public Type ViewModelType { get; protected set; }

    public DialogClosedEventArgs()
    {
        DialogId = Guid.Empty;
        ViewModelType = typeof(void);
        Result = DialogResult.Cancel();
    }
    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, IDialogResult? result = null)
    {
        DialogId = dialogId;
        ViewModelType = viewModelType;
        Result = result ?? DialogResult.Cancel();
    }

    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, bool success)
    {
        DialogId = dialogId;
        ViewModelType = viewModelType;
        Result = success ? DialogResult.Success() : DialogResult.Cancel();
    }

    public DialogClosedEventArgs(Guid dialogId, Type viewModelType, string errorMessage = "")
    {
        DialogId = dialogId;
        ViewModelType = viewModelType;
        Result = DialogResult.Error(errorMessage);
    }
}
