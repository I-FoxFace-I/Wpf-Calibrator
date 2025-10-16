using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Calibrator.WpfControl.Converters;

/// <summary>
/// Converts boolean to Visibility with inverted logic (true = Collapsed, false = Visible)
/// Used for showing validation errors
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Initializes a new instance of the InverseBoolToVisibilityConverter class
    /// </summary>
    public InverseBoolToVisibilityConverter()
    {
    }

    /// <summary>
    /// Converts a boolean value to Visibility with inverted logic
    /// </summary>
    /// <param name="value">The boolean value to convert</param>
    /// <param name="targetType">The target type (not used)</param>
    /// <param name="parameter">The converter parameter (not used)</param>
    /// <param name="culture">The culture to use for conversion (not used)</param>
    /// <returns>Visibility.Collapsed if true, Visibility.Visible if false</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts back from Visibility to boolean with inverted logic
    /// </summary>
    /// <param name="value">The Visibility value to convert back</param>
    /// <param name="targetType">The target type (not used)</param>
    /// <param name="parameter">The converter parameter (not used)</param>
    /// <param name="culture">The culture to use for conversion (not used)</param>
    /// <returns>True if Visibility.Collapsed, false otherwise</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Collapsed;
        }

        return true;
    }
}