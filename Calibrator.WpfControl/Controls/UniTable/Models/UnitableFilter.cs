namespace Calibrator.WpfControl.Controls.UniTable.Models;

/// <summary>
/// Represents a filter for UniTable columns
/// </summary>
public class UniTableFilter
{
    /// <summary>
    /// Name of the column to filter
    /// </summary>
    public string ColumnName { get; set; }
    
    /// <summary>
    /// Filter text/value
    /// </summary>
    public string FilterText { get; set; }
    
    /// <summary>
    /// Type of filter operation
    /// </summary>
    public FilterType Type { get; set; } = FilterType.Contains;
}

/// <summary>
/// Types of filter operations
/// </summary>
public enum FilterType
{
    Contains,
    Equals,
    NotEquals,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan
}
