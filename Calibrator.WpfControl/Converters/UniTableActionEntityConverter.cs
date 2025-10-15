using System;
using System.Globalization;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.UniTable;

namespace Calibrator.WpfControl.Converters;

/// <summary>
/// Allows to retrieve both params of button click as tuple for <see cref="UniTableComponent.OnExecuteAction"/>
/// </summary>
public class UniTableActionEntityConverter: IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is UniTableBaseAction action && values[1] != null)
        {
            return Tuple.Create(action, values[1]);
        }

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

