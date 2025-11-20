namespace WpfEngine.Data.Parameters;

/// <summary>
/// Parameters for list ViewModels with optional filter
/// </summary>
public record CollectionParameters: BaseModelParameters
{
    public int? PageSize { get; init; }
    public string? FilterText { get; init; }

    public CollectionParameters(string? filterText = null, int? pageSize = null)
    {
        PageSize = pageSize;
        FilterText = filterText;
    }

    public CollectionParameters()
    {
        
    }
}

