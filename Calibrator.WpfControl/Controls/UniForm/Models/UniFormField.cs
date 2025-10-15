using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Calibrator.WpfControl.Validation;

namespace Calibrator.WpfControl.Controls.UniForm.Models;

public abstract class UniFormField
{
    public string Label { get; init; } = string.Empty;
    public int Order { get; init; } = 0;
    public string? ToolTip { get; init; }
    public bool IsRequired { get; init; }
    public string Category { get; init; } = "Default";
}

public abstract class UniFormRegularField : UniFormField
{
    public bool IsReadOnly { get; init; }
    public abstract string GetPropertyName();
    
    /// <summary>
    /// Function to determine if field should be visible based on data context
    /// </summary>
    public Func<object, bool>? VisibilityCondition { get; init; }
    
    /// <summary>
    /// Validators for this field
    /// </summary>
    public List<IValidator<object>>? Validators { get; init; }
}

public class UniFormRegularField<T> : UniFormRegularField
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

// TextField
public abstract class UniFormTextField : UniFormRegularField
{
    public int MaxLength { get; init; } = 0;
    public bool IsMultiline { get; init; }
    public string? Placeholder { get; init; }
}

public class UniFormTextField<T> : UniFormTextField
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

// NumericField
public abstract class UniFormNumericField : UniFormRegularField
{
    public double? Minimum { get; init; }
    public double? Maximum { get; init; }
    public double Step { get; init; } = 1.0;
}

public class UniFormNumericField<T> : UniFormNumericField
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

// CheckBoxField
public abstract class UniFormCheckBoxField : UniFormRegularField
{
}

public class UniFormCheckBoxField<T> : UniFormCheckBoxField
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

// ComboBoxField
public abstract class UniFormComboBoxField : UniFormRegularField
{
    public object? ItemsSource { get; init; }
    public string? DisplayMemberPath { get; init; }
}

public class UniFormComboBoxField<T> : UniFormComboBoxField
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
