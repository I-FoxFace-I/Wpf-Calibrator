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
    public InverseBoolToVisibilityConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Collapsed;
        }
        
        return true;
    }
}
