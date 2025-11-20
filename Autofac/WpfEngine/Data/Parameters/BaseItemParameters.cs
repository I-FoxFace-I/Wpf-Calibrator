namespace WpfEngine.Data.Parameters;

public abstract record BaseItemParameters : IEquatable<object>
{
    protected BaseItemParameters(bool readOnly=true)
    {
        ReadOnly = readOnly;
    }
    protected BaseItemParameters(BaseItemParameters original)
    {
        ReadOnly = original.ReadOnly;
    }
    public bool ReadOnly { get; init; }

    public abstract object? Item { get; }
}

