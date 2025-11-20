namespace WpfEngine.Data.Parameters;

/// <summary>
/// Parameters for dialog ViewModels with data payload
/// </summary>
public record EntityParameters<TData> : BaseItemParameters
{
    public EntityParameters(bool readOnly = true) : base(readOnly)
    {
        
    }
    public EntityParameters(TData entity, bool readOnly = true, string? title = null) : base(readOnly)
    {
        Entity = entity;
        Title = title;
    }
    public override object? Item => Entity;

    public TData? Entity { get; init; }
    public string? Title { get; init; }
}

