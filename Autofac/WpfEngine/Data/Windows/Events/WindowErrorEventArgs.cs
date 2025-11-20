namespace WpfEngine.Data.Windows.Events;

public class WindowErrorEventArgs : EventArgs
{
    public string Operation { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    public Guid? WindowId { get; }

    public WindowErrorEventArgs(string operation, string errorMessage, Exception? exception, Guid? windowId)
    {
        Operation = operation;
        ErrorMessage = errorMessage;
        Exception = exception;
        WindowId = windowId;
    }
}
