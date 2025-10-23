//using System;
//using System.Globalization;
//using System.Windows;
//using System.Windows.Data;
//using System.Windows.Media;

//namespace AutofacEnhancedWpfDemo.Converters;

///// <summary>
///// Converts bool to Visibility (true = Visible, false = Collapsed)
///// </summary>
//public class BoolToVisibilityConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is bool boolValue)
//        {
//            return boolValue ? Visibility.Visible : Visibility.Collapsed;
//        }
//        return Visibility.Collapsed;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is Visibility visibility)
//        {
//            return visibility == Visibility.Visible;
//        }
//        return false;
//    }
//}

///// <summary>
///// Converts null to Visibility (null = Collapsed, not null = Visible)
///// </summary>
//public class NullToVisibilityConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        return value != null ? Visibility.Visible : Visibility.Collapsed;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

///// <summary>
///// Inverts bool value
///// </summary>
//public class InverseBoolConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is bool boolValue)
//        {
//            return !boolValue;
//        }
//        return true;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is bool boolValue)
//        {
//            return !boolValue;
//        }
//        return false;
//    }
//}


///// <summary>
///// Converter to show/hide content based on workflow step
///// </summary>
//public class StepVisibilityConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
//        {
//            return currentStep == step ? Visibility.Visible : Visibility.Collapsed;
//        }
//        return Visibility.Collapsed;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

///// <summary>
///// Converts hex color string to Color
///// </summary>
//public class StringToColorConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
//        {
//            try
//            {
//                return (Color)ColorConverter.ConvertFromString(hexColor);
//            }
//            catch
//            {
//                return Colors.Blue;
//            }
//        }
//        return Colors.Blue;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}


/////// <summary>
/////// Converter to show/hide content based on workflow step
/////// </summary>
////public class StepVisibilityConverter : IValueConverter
////{
////    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
////    {
////        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
////        {
////            return currentStep == step ? Visibility.Visible : Visibility.Collapsed;
////        }
////        return Visibility.Collapsed;
////    }

////    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
////    {
////        throw new NotImplementedException();
////    }
////}