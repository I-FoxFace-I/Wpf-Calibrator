using System;
using System.Threading.Tasks;

namespace WpfEngine.Data.Windows;

public class WindowErrorInfo
{
    public DateTime Timestamp { get; init; }
    public string Operation { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public Exception? Exception { get; init; }
    public Guid? WindowId { get; init; }
}