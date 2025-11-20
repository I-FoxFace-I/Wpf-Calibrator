namespace WpfEngine.Data.Dialogs;

public record CancelResult : BaseResult
{
    public override bool IsSuccess => false;
    public override string? ErrorMessage { get; init; } = "Canceled by User";
}
