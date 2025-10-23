using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfApplication.Views.Components.UniTable;

public abstract class UniTableColumn
{
    public string ColumnName { get; init; }
    public int Width { get; init; } = 90;
}

public abstract class UniTableRegularColumn : UniTableColumn
{
    public bool IsReadOnly { get; init; } = true;
    public bool IsCheckBox { get; init; }
    public abstract string GetPropertyName();
}

public class UniTableRegularColumn<T> : UniTableRegularColumn
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
                throw new InvalidOperationException("Invalid expression")
        };
    }
}

public abstract class UniTableBaseAction
{
    public required string ToolTip { get; set; }
    public required PackIconMaterialKind IconKind { get; set; }
}

public class UniTableAction : UniTableBaseAction
{
    public required Action<object> Command  { get; init; }
}

public class UniTableAsyncAction : UniTableBaseAction
{
    public required Func<object, Task> Command  { get; init; }
}


