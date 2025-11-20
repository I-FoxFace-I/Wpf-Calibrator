namespace WpfEngine.Data.Windows.Events;

public class WindowClosedEventArgs : EventArgs
{
    public Guid WindowId { get; }
    public Guid? ParentWindowId { get; }
    public Type? ViewModelType { get; }
    public Guid? SessionId { get; }

    public WindowClosedEventArgs(Guid windowId, Type? viewModelType, Guid? parentWindowId = null, Guid? sessionId = null)
    {
        WindowId = windowId;
        ViewModelType = viewModelType;
        ParentWindowId = parentWindowId;
        SessionId = sessionId;
    }
}
