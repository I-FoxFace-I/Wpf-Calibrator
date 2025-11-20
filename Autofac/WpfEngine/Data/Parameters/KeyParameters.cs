namespace WpfEngine.Data.Parameters;

/// Parameters for entity detail/edit ViewModels
/// </summary>
public record KeyParameters<TKey> : BaseItemParameters
    where TKey : notnull, IEquatable<TKey>
{
    public KeyParameters() : base()
    {
        ItemKey = default;
        ReadOnly = true;
    }
    public KeyParameters(BaseItemParameters original) : base(original)
    {
        ItemKey = original is KeyParameters<TKey> target ? target.ItemKey : default;
    }

    public TKey? ItemKey { get; init; }
    public override object? Item => ItemKey;
}

