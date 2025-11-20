namespace WpfEngine.Data.Content;

/// <summary>
/// Event args for close request (legacy support)
/// </summary>
public class NavigatorCloseRequestedEventArgs : EventArgs
{
    public bool ShowConfirmation { get; init; }
    public string? ConfirmationMessage { get; init; }
    
    public NavigatorCloseRequestedEventArgs(bool showConfirmation, string? confirmationMessage)
    {
        ShowConfirmation = showConfirmation;
        ConfirmationMessage = confirmationMessage;
    }
}
