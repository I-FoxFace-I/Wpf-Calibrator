using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Controls.UniTable;

/// <summary>
/// Base class for all table column definitions
/// </summary>
public abstract class UniTableColumn
{
    /// <summary>
    /// Gets the display name of the column
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// Gets the width of the column in pixels
    /// </summary>
    public int Width { get; init; } = 90;
}

/// <summary>
/// Base class for regular data-bound table columns
/// </summary>
public abstract class UniTableRegularColumn : UniTableColumn
{
    /// <summary>
    /// Gets whether the column is read-only
    /// </summary>
    public bool IsReadOnly { get; init; } = true;

    /// <summary>
    /// Gets whether the column should be rendered as a checkbox
    /// </summary>
    public bool IsCheckBox { get; init; }

    /// <summary>
    /// Gets the name of the property this column is bound to.
    /// </summary>
    /// <returns>The property name.</returns>
    public abstract string GetPropertyName();
}

/// <summary>
/// Typed implementation of a regular table column
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public class UniTableRegularColumn<T> : UniTableRegularColumn
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
        ArgumentNullException.ThrowIfNull(PropertySelector);

        return PropertySelector.Body switch
        {
            UnaryExpression { Operand: MemberExpression memberExpression }
                => memberExpression.Member.Name,
            MemberExpression memberExpression =>
                memberExpression.Member.Name,
            _ =>
                throw new InvalidOperationException("Invalid expression")
        };
    }
}

/// <summary>
/// Base class for table row actions
/// </summary>
public abstract class UniTableBaseAction
{
    /// <summary>
    /// Gets or sets the tooltip text for the action button
    /// </summary>
    public required string ToolTip { get; set; }

    /// <summary>
    /// Gets or sets the Material Design icon to display
    /// </summary>
    public required PackIconMaterialKind IconKind { get; set; }
}

/// <summary>
/// Represents a synchronous table row action
/// </summary>
public class UniTableAction : UniTableBaseAction
{
    /// <summary>
    /// Gets the command to execute when the action is triggered
    /// </summary>
    public required Action<object> Command { get; init; }
}

/// <summary>
/// Represents an asynchronous table row action
/// </summary>
public class UniTableAsyncAction : UniTableBaseAction
{
    /// <summary>
    /// Gets the asynchronous command to execute when the action is triggered
    /// </summary>
    public required Func<object, Task> Command { get; init; }
}