namespace WpfEngine.Data.Windows.Events;

/// <summary>
/// Event args for window close request from navigation service
/// </summary>
public class WindowCloseRequestedEventArgs : EventArgs
{
    public WindowCloseRequestedEventArgs(bool showConfirmation, string? confirmationMessage)
    {
        ShowConfirmation = showConfirmation;
        ConfirmationMessage = confirmationMessage;
    }

    /// <summary>
    /// Should show confirmation dialog before closing
    /// </summary>
    public bool ShowConfirmation { get; }

    /// <summary>
    /// Optional custom confirmation message
    /// </summary>
    public string? ConfirmationMessage { get; }
}
