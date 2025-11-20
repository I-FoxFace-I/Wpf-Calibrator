namespace WpfEngine.Data.Windows.Events;

// ========== EVENT ARGS ==========



public class WindowContextErrorEventArgs : EventArgs
{
    public string Operation { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    public Guid? ChildWindowId { get; }

    public WindowContextErrorEventArgs(string operation, string errorMessage, Exception? exception = null, Guid? childWindowId = null)
    {
        Operation = operation;
        ErrorMessage = errorMessage;
        Exception = exception;
        ChildWindowId = childWindowId;
    }
}
