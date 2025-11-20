using WpfEngine.Data.Abstract;

namespace WpfEngine.Data.Dialogs.Events;

public class ChildDialogClosedEventArgs : EventArgs
{
    public Guid ParentDialogId { get; }
    public Guid ChildDialogId { get; }
    public Type ViewModelType { get; }
    public IDialogResult? Result { get; }

    public ChildDialogClosedEventArgs(Guid parentDialogId, Guid childDialogId, Type viewModelType, IDialogResult? result = null)
    {
        ParentDialogId = parentDialogId;
        ChildDialogId = childDialogId;
        ViewModelType = viewModelType;
        Result = result;
    }
}
