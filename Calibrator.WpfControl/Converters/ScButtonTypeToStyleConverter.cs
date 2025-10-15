using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.ScButton;

namespace Calibrator.WpfControl.Converters;

public class ScButtonTypeToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ScButtonType buttonType) 
            return GetStyle("ScRegularButton");
        
        return buttonType switch
        {
            ScButtonType.Accent => GetStyle("AccentButton"),
            ScButtonType.Transparent => GetStyle("TransparentButton"),
            ScButtonType.Regular => GetStyle("ScRegularButton"),
            _ => GetStyle("ScRegularButton")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static Style? GetStyle(string key)
    {
        try
        {
            return Application.Current?.FindResource(key) as Style;
        }
        catch (ResourceReferenceKeyNotFoundException)
        {
            // Log warning if needed
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}


