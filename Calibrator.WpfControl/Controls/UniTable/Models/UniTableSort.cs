namespace Calibrator.WpfControl.Controls.UniTable.Models;

/// <summary>
/// Represents a sort configuration for UniTable columns
/// </summary>
public class UniTableSort
{
    /// <summary>
    /// Gets or sets the name of the column to sort
    /// </summary>
    public required string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the sort direction
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

/// <summary>
/// Defines the sort direction for table columns
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Sort in ascending order (A-Z, 0-9)
    /// </summary>
    Ascending,

    /// <summary>
    /// Sort in descending order (Z-A, 9-0)
    /// </summary>
    Descending
}