namespace WpfEngine.Data.Windows;

public class WindowContextErrorInfo
{
    public DateTime Timestamp { get; init; }
    public string Operation { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public Exception? Exception { get; init; }
    public Guid? ChildWindowId { get; init; }
}
