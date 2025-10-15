using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Calibrator.WpfControl.Controls.UniTable;

namespace Calibrator.WpfControl.Controls.ScSmartTable.Models;

/// <summary>
/// Enhanced column for SmartTable with advanced filtering, sorting and grouping capabilities
/// Extends UniTableRegularColumn with additional properties
/// </summary>
public abstract class SmartTableRegularColumn : UniTableRegularColumn
{
    /// <summary>
    /// Enable filtering for this column
    /// </summary>
    public bool IsFilterable { get; init; } = true;

    /// <summary>
    /// Enable sorting for this column
    /// </summary>
    public bool IsSortable { get; init; } = true;

    /// <summary>
    /// Allowed filter operators for this column
    /// If null, defaults will be determined by column data type
    /// </summary>
    public List<FilterOperatorType>? AllowedFilterOperators { get; init; }

    /// <summary>
    /// Default filter operator (first in AllowedFilterOperators if not set)
    /// </summary>
    public FilterOperatorType? DefaultFilterOperator { get; init; }

    /// <summary>
    /// Enable grouping for this column
    /// </summary>
    public bool IsGroupable { get; init; } = false;

    /// <summary>
    /// Column can be reordered by user
    /// </summary>
    public bool CanReorder { get; init; } = true;

    /// <summary>
    /// Column can be frozen by user
    /// </summary>
    public bool CanFreeze { get; init; } = false;

    /// <summary>
    /// Data type of the column (used for automatic filter operator selection)
    /// </summary>
    public Type? DataType { get; init; }
}

/// <summary>
/// Typed SmartTable column with property selector
/// </summary>
public class SmartTableRegularColumn<T> : SmartTableRegularColumn
{
    public required Expression<Func<T, object>> PropertySelector { get; init; }

    public override string GetPropertyName()
    {
        ArgumentNullException.ThrowIfNull(PropertySelector);

        return PropertySelector.Body switch
        {
            UnaryExpression { Operand: MemberExpression memberExpression }
                => memberExpression.Member.Name,
            MemberExpression memberExpression =>
                memberExpression.Member.Name,
            _ =>
                throw new InvalidOperationException("Invalid expression - property selector must reference a property")
        };
    }

    /// <summary>
    /// Get the data type from the property selector if not explicitly set
    /// </summary>
    public Type GetDataType()
    {
        if (DataType != null)
            return DataType;

        return PropertySelector.Body switch
        {
            UnaryExpression { Operand: MemberExpression memberExpression }
                => memberExpression.Type,
            MemberExpression memberExpression =>
                memberExpression.Type,
            _ => typeof(object)
        };
    }
}