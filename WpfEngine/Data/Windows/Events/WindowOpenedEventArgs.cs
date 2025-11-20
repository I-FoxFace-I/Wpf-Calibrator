namespace WpfEngine.Data.Windows.Events;

/// <summary>
/// Event args for window opened
/// </summary>
public class WindowOpenedEventArgs : EventArgs
{
    public Guid WindowId { get; init; }
    public Type ViewModelType { get; init; } = null!;
    public Guid? ParentWindowId { get; init; }
    public Guid? SessionId { get; init; }

    public WindowOpenedEventArgs() : base()
    {
        
    }

    public WindowOpenedEventArgs(Guid windowId, Type viewModelType, Guid? parentWindowId = null, Guid? sessionId = null) : base()
    {
        WindowId = windowId;
        ViewModelType = viewModelType;
        ParentWindowId = parentWindowId;
        SessionId = sessionId;
    }

}
