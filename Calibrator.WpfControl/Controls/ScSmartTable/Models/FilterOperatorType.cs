using System;
using System.Collections.Generic;
using Telerik.Windows.Data;

namespace Calibrator.WpfControl.Controls.ScSmartTable.Models;

/// <summary>
/// Simplified filter operator types for SmartTable columns
/// Reduced to essential operators for better UX and maintainability
/// </summary>
public enum FilterOperatorType
{
    /// <summary>
    /// Universal text search - checks if text contains the specified value
    /// </summary>
    Contains,

    /// <summary>
    /// Exact text match
    /// </summary>
    IsEqualTo,

    /// <summary>
    /// Checks if value is null or empty
    /// </summary>
    IsEmpty,

    /// <summary>
    /// Checks if value is not null or empty
    /// </summary>
    IsNotEmpty,

    /// <summary>
    /// Exact value match for numeric and date types
    /// </summary>
    Equals,

    /// <summary>
    /// Greater than or equal comparison (?)
    /// </summary>
    GreaterOrEqual,

    /// <summary>
    /// Less than or equal comparison (?)
    /// </summary>
    LessOrEqual,

    /// <summary>
    /// Checks if boolean value is true
    /// </summary>
    IsTrue,

    /// <summary>
    /// Checks if boolean value is false
    /// </summary>
    IsFalse
}

/// <summary>
/// Helper class for filter operator conversions and defaults
/// </summary>
public static class FilterOperatorHelper
{
    /// <summary>
    /// Get default filter operators based on property type (simplified)
    /// </summary>
    /// <param name="propertyType">The type of the property to get operators for</param>
    /// <returns>A collection of appropriate filter operators for the given type</returns>
    public static ICollection<FilterOperatorType> GetDefaultForType(Type propertyType)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType == typeof(string))
        {
            // Simplified text operators: most common use cases
            return new List<FilterOperatorType>
            {
                FilterOperatorType.Contains,        // Primary text search
                FilterOperatorType.IsEqualTo,       // Exact match
                FilterOperatorType.IsEmpty,         // Null/empty check
                FilterOperatorType.IsNotEmpty       // Not null/empty check
            };
        }

        if (IsNumericType(underlyingType) || underlyingType == typeof(DateTime))
        {
            // Unified numeric and date operators: 3 operators cover all cases
            // Examples: x=5, x?5, x?5 can express: equals, greater, less, between (?5 AND ?10)
            return new List<FilterOperatorType>
            {
                FilterOperatorType.Equals,          // Exact match (x = 5)
                FilterOperatorType.GreaterOrEqual,  // Greater or equal (x ? 5)
                FilterOperatorType.LessOrEqual      // Less or equal (x ? 5)
            };
        }

        if (underlyingType == typeof(bool))
        {
            return new List<FilterOperatorType>
            {
                FilterOperatorType.IsTrue,
                FilterOperatorType.IsFalse
            };
        }

        // Default for unknown types
        return new List<FilterOperatorType>
        {
            FilterOperatorType.Equals
        };
    }

    /// <summary>
    /// Check if type is numeric
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is a numeric type, false otherwise</returns>
    private static bool IsNumericType(Type type)
    {
        return type == typeof(int)
            || type == typeof(long)
            || type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong)
            || type == typeof(ushort)
            || type == typeof(sbyte);
    }

    /// <summary>
    /// Convert simplified FilterOperatorType to Telerik FilterOperator.
    /// </summary>
    /// <param name="operatorType">The filter operator type to convert.</param>
    /// <returns>The corresponding Telerik FilterOperator.</returns>
    public static FilterOperator ToTelerikOperator(FilterOperatorType operatorType)
    {
        return operatorType switch
        {
            // Text operators
            FilterOperatorType.Contains => FilterOperator.Contains,
            FilterOperatorType.IsEqualTo => FilterOperator.IsEqualTo,
            FilterOperatorType.IsEmpty => FilterOperator.IsEmpty,
            FilterOperatorType.IsNotEmpty => FilterOperator.IsNotEmpty,

            // Numeric/Date operators (simplified)
            FilterOperatorType.Equals => FilterOperator.IsEqualTo,
            FilterOperatorType.GreaterOrEqual => FilterOperator.IsGreaterThanOrEqualTo,
            FilterOperatorType.LessOrEqual => FilterOperator.IsLessThanOrEqualTo,

            // Boolean operators
            FilterOperatorType.IsTrue => FilterOperator.IsEqualTo,
            FilterOperatorType.IsFalse => FilterOperator.IsEqualTo,

            _ => FilterOperator.IsEqualTo
        };
    }

    /// <summary>
    /// Get display name for simplified filter operators.
    /// </summary>
    /// <param name="operatorType">The filter operator type to get display name for.</param>
    /// <returns>A human-readable display name for the operator.</returns>
    public static string GetDisplayName(FilterOperatorType operatorType)
    {
        return operatorType switch
        {
            // Text operators
            FilterOperatorType.Contains => "Contains",
            FilterOperatorType.IsEqualTo => "Equals",
            FilterOperatorType.IsEmpty => "Is Empty",
            FilterOperatorType.IsNotEmpty => "Is Not Empty",

            // Numeric/Date operators (with mathematical symbols for clarity)
            FilterOperatorType.Equals => "= (Equals)",
            FilterOperatorType.GreaterOrEqual => "? (Greater or Equal)",
            FilterOperatorType.LessOrEqual => "? (Less or Equal)",

            // Boolean operators
            FilterOperatorType.IsTrue => "Is True",
            FilterOperatorType.IsFalse => "Is False",

            _ => operatorType.ToString()
        };
    }
}