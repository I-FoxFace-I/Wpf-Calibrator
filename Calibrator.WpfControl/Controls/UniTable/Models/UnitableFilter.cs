namespace Calibrator.WpfControl.Controls.UniTable.Models;

/// <summary>
/// Represents a filter for UniTable columns
/// </summary>
public class UniTableFilter
{
    /// <summary>
    /// Gets or sets the name of the column to filter
    /// </summary>
    public required string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the filter text/value
    /// </summary>
    public required string FilterText { get; set; }

    /// <summary>
    /// Gets or sets the type of filter operation
    /// </summary>
    public FilterType Type { get; set; } = FilterType.Contains;
}

/// <summary>
/// Defines the types of filter operations available for table columns
/// </summary>
public enum FilterType
{
    /// <summary>
    /// Filter for values that contain the specified text
    /// </summary>
    Contains,

    /// <summary>
    /// Filter for values that exactly match the specified text
    /// </summary>
    Equals,

    /// <summary>
    /// Filter for values that do not match the specified text
    /// </summary>
    NotEquals,

    /// <summary>
    /// Filter for values that start with the specified text
    /// </summary>
    StartsWith,

    /// <summary>
    /// Filter for values that end with the specified text
    /// </summary>
    EndsWith,

    /// <summary>
    /// Filter for numeric values greater than the specified value
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Filter for numeric values less than the specified value
    /// </summary>
    LessThan
}