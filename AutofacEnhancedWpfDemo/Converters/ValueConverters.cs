using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutofacEnhancedWpfDemo.Converters;

/// <summary>
/// Converts bool to Visibility (true = Visible, false = Collapsed)
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

/// <summary>
/// Converts null to Visibility (null = Collapsed, not null = Visible)
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts bool value
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

/// <summary>
/// Multiplies two values (for calculating order item totals)
/// </summary>
public class MultiplyConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is int quantity && values[1] is decimal unitPrice)
        {
            return (quantity * unitPrice).ToString("C2", culture);
        }
        return "$0.00";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts hex color string to Color
/// </summary>
public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(hexColor);
            }
            catch
            {
                return Colors.Blue;
            }
        }
        return Colors.Blue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


/// <summary>
/// Converter to show/hide content based on workflow step
/// </summary>
public class StepVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
        {
            return currentStep == step ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for step progress indicator colors
/// </summary>
public class StepColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
        {
            if (currentStep > step)
                return Colors.Green; // Completed
            if (currentStep == step)
                return Color.FromRgb(59, 130, 246); // Active (Blue)
            return Color.FromRgb(203, 213, 225); // Pending (Gray)
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to custom string based on parameter
/// Parameter format: "TrueText|FalseText"
/// Example: "Hide|Show" or "Yes|No"
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramStr)
            return string.Empty;

        var parts = paramStr.Split('|');
        if (parts.Length != 2)
            return string.Empty;

        return boolValue ? parts[0] : parts[1];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Checks if value is null
/// Returns true if null, false otherwise
/// Parameter: "Inverted" to invert the logic
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value == null;
        bool invert = parameter is string str && str.Equals("Inverted", StringComparison.OrdinalIgnoreCase);

        return invert ? !isNull : isNull;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

