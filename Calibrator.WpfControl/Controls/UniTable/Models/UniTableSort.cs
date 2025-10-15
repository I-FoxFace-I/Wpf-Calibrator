namespace Calibrator.WpfControl.Controls.UniTable.Models;

/// <summary>
/// Represents a sort configuration for UniTable columns
/// </summary>
public class UniTableSort
{
    /// <summary>
    /// Name of the column to sort
    /// </summary>
    public string ColumnName { get; set; }
    
    /// <summary>
    /// Sort direction
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}

/// <summary>
/// Sort direction
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}
