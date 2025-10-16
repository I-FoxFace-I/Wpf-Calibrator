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
    /// Gets whether filtering is enabled for this column
    /// </summary>
    public bool IsFilterable { get; init; } = true;

    /// <summary>
    /// Gets whether sorting is enabled for this column
    /// </summary>
    public bool IsSortable { get; init; } = true;

    /// <summary>
    /// Gets the allowed filter operators for this column
    /// If null, defaults will be determined by column data type
    /// </summary>
    public ICollection<FilterOperatorType>? AllowedFilterOperators { get; init; }

    /// <summary>
    /// Gets the default filter operator (first in AllowedFilterOperators if not set)
    /// </summary>
    public FilterOperatorType? DefaultFilterOperator { get; init; }

    /// <summary>
    /// Gets whether grouping is enabled for this column
    /// </summary>
    public bool IsGroupable { get; init; }

    /// <summary>
    /// Gets whether the column can be reordered by user
    /// </summary>
    public bool CanReorder { get; init; } = true;

    /// <summary>
    /// Gets whether the column can be frozen by user
    /// </summary>
    public bool CanFreeze { get; init; }

    /// <summary>
    /// Gets the data type of the column (used for automatic filter operator selection)
    /// </summary>
    public Type? DataType { get; init; }
}

/// <summary>
/// Typed SmartTable column with property selector
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public class SmartTableRegularColumn<T> : SmartTableRegularColumn
{
    /// <summary>
    /// Gets the expression that selects the property to bind to
    /// </summary>
    public required Expression<Func<T, object>> PropertySelector { get; init; }

    /// <summary>
    /// Gets the property name from the property selector expression
    /// </summary>
    /// <returns>The name of the property</returns>
    public override string GetPropertyName()
    {
        ArgumentNullException.ThrowIfNull(this.PropertySelector);

        return this.PropertySelector.Body switch
        {
            UnaryExpression { Operand: MemberExpression memberExpression }
                => memberExpression.Member.Name,
            MemberExpression memberExpression =>
                memberExpression.Member.Name,
            _ =>
                throw new InvalidOperationException("Invalid expression - property selector must reference a property"),
        };
    }

    /// <summary>
    /// Gets the data type from the property selector if not explicitly set
    /// </summary>
    public Type ColumnDataType
    {
        get
        {
            if (this.DataType != null)
            {
                return this.DataType;
            }

            return this.PropertySelector.Body switch
            {
                UnaryExpression { Operand: MemberExpression memberExpression }
                    => memberExpression.Type,
                MemberExpression memberExpression =>
                    memberExpression.Type,
                _ => typeof(object),
            };
        }
    }
}