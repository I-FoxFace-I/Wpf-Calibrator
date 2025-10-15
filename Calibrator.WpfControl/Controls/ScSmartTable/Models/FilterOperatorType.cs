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
    // Text operators (simplified to 3 most common)
    Contains,           // Universal text search
    IsEqualTo,          // Exact match
    IsEmpty,            // Null/empty check
    IsNotEmpty,         // Not null/empty check

    // Numeric/Date operators (simplified to cover all cases with 3 operators)
    Equals,             // Exact value match (=)
    GreaterOrEqual,     // Greater than or equal (≥) 
    LessOrEqual,        // Less than or equal (≤)

    // Boolean operators (minimal set)
    IsTrue,
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
    public static List<FilterOperatorType> GetDefaultForType(Type propertyType)
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
            // Examples: x=5, x≥5, x≤5 can express: equals, greater, less, between (≥5 AND ≤10)
            return new List<FilterOperatorType>
            {
                FilterOperatorType.Equals,          // Exact match (x = 5)
                FilterOperatorType.GreaterOrEqual,  // Greater or equal (x ≥ 5)
                FilterOperatorType.LessOrEqual      // Less or equal (x ≤ 5)
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
    /// Convert simplified FilterOperatorType to Telerik FilterOperator
    /// </summary>
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
    /// Get display name for simplified filter operators
    /// </summary>
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
            FilterOperatorType.GreaterOrEqual => "≥ (Greater or Equal)",
            FilterOperatorType.LessOrEqual => "≤ (Less or Equal)",
            
            // Boolean operators
            FilterOperatorType.IsTrue => "Is True",
            FilterOperatorType.IsFalse => "Is False",
            
            _ => operatorType.ToString()
        };
    }
}