namespace WpfEngine.Data.Dialogs.Events;

// ========== EVENT ARGS ==========

public class DialogOpenedEventArgs : EventArgs
{
    public Guid DialogId { get; }
    public Guid? ParentDialogId { get; }
    public Type ViewModelType { get; }
    public bool IsNested => ParentDialogId.HasValue;

    public DialogOpenedEventArgs(Guid dialogId, Type viewModelType, Guid? parentDialogId = null)
    {
        DialogId = dialogId;
        ViewModelType = viewModelType;
        ParentDialogId = parentDialogId;
    }
}
