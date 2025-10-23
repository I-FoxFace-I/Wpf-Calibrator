using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Calibrator.WpfApplication.Views.Components.ScButton;

namespace Calibrator.WpfApplication.Converters;

public class ScButtonTypeToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ScButtonType buttonType) 
            return Application.Current.FindResource("ScRegularButton") as Style;
        
        return buttonType switch
        {
            ScButtonType.Accent => Application.Current.FindResource("AccentButton") as Style,
            ScButtonType.Transparent => Application.Current.FindResource("TransparentButton") as Style,
            ScButtonType.Regular => Application.Current.FindResource("ScRegularButton") as Style,
            _ => Application.Current.FindResource("ScRegularButton") as Style
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


