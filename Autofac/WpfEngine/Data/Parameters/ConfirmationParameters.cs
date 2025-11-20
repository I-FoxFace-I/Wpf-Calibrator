namespace WpfEngine.Data.Parameters;

public record ConfirmationParameters : BaseModelParameters
{
    public string Message { get; init; }
    public string? Title { get; init; } = null;
    public string ConfirmText { get; init; } = "OK";
    public string CancelText { get; init; } = "Cancel";

    public ConfirmationParameters(string message, string? title = null, string confirmText = "OK", string cancelText = "Cancel")
    {
        Message = message;
        Title = title;
        ConfirmText = confirmText;
        CancelText = cancelText;
    }

    public ConfirmationParameters()
    {
        Message = string.Empty;
    }
}

