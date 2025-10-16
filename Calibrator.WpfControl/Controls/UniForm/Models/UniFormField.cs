using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Calibrator.WpfControl.Validation;

namespace Calibrator.WpfControl.Controls.UniForm.Models;

/// <summary>
/// Base class for all form field definitions
/// </summary>
public abstract class UniFormField
{
    /// <summary>
    /// Gets the label text for the field
    /// </summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display order of the field
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Gets the tooltip text for the field
    /// </summary>
    public string? ToolTip { get; init; }

    /// <summary>
    /// Gets whether the field is required
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets the category name for grouping fields
    /// </summary>
    public string Category { get; init; } = "Default";
}

/// <summary>
/// Base class for regular form fields that bind to properties
/// </summary>
public abstract class UniFormRegularField : UniFormField
{
    /// <summary>
    /// Gets whether the field is read-only
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Gets the name of the property this field is bound to
    /// </summary>
    /// <returns>The property name</returns>
    public abstract string GetPropertyName();

    /// <summary>
    /// Function to determine if field should be visible based on data context
    /// </summary>
    public Func<object, bool>? VisibilityCondition { get; init; }

    /// <summary>
    /// Validators for this field
    /// </summary>
    public ICollection<IValidator<object>>? Validators { get; init; }
}

/// <summary>
/// Typed implementation of a regular form field
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public class UniFormRegularField<T> : UniFormRegularField
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
/// Base class for text input fields
/// </summary>
public abstract class UniFormTextField : UniFormRegularField
{
    /// <summary>
    /// Gets the maximum length allowed for the text
    /// </summary>
    public int MaxLength { get; init; }

    /// <summary>
    /// Gets whether the field should allow multiline input
    /// </summary>
    public bool IsMultiline { get; init; }

    /// <summary>
    /// Gets the placeholder text to display when empty
    /// </summary>
    public string? Placeholder { get; init; }
}

/// <summary>
/// Typed implementation of a text input field
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public class UniFormTextField<T> : UniFormTextField
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
/// Base class for numeric input fields
/// </summary>
public abstract class UniFormNumericField : UniFormRegularField
{
    /// <summary>
    /// Gets the minimum allowed value
    /// </summary>
    public double? Minimum { get; init; }

    /// <summary>
    /// Gets the maximum allowed value
    /// </summary>
    public double? Maximum { get; init; }

    /// <summary>
    /// Gets the increment/decrement step value
    /// </summary>
    public double Step { get; init; } = 1.0;
}

/// <summary>
/// Typed implementation of a numeric input field
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public class UniFormNumericField<T> : UniFormNumericField
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
/// Base class for checkbox fields
/// </summary>
public abstract class UniFormCheckBoxField : UniFormRegularField
{
}

/// <summary>
/// Typed implementation of a checkbox field
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public class UniFormCheckBoxField<T> : UniFormCheckBoxField
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
/// Base class for dropdown/combobox fields
/// </summary>
public abstract class UniFormComboBoxField : UniFormRegularField
{
    /// <summary>
    /// Gets the collection of items to display in the dropdown
    /// </summary>
    public object? ItemsSource { get; init; }

    /// <summary>
    /// Gets the path to the property that should be displayed for each item
    /// </summary>
    public string? DisplayMemberPath { get; init; }
}

/// <summary>
/// Typed implementation of a dropdown/combobox field
/// </summary>
/// <typeparam name="T">The type of the data model</typeparam>
public class UniFormComboBoxField<T> : UniFormComboBoxField
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