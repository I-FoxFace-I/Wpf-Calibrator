namespace WpfEngine.Data.Windows.Events;

/// <summary>
/// Event args for child window closed
/// </summary>
public class ChildWindowClosedEventArgs : EventArgs
{
    public Guid ChildWindowId { get; }
    public Type? ViewModelType { get; }

    public ChildWindowClosedEventArgs(Guid childWindowId, Type? viewModelType = null)
    {
        ChildWindowId = childWindowId;
        ViewModelType = viewModelType;
    }
}
