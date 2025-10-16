using System;
using System.Globalization;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.UniTable;

namespace Calibrator.WpfControl.Converters;

/// <summary>
/// Allows to retrieve both params of button click as tuple for <see cref="UniTableComponent.OnExecuteAction"/>
/// </summary>
public class UniTableActionEntityConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts multiple values into a tuple containing the action and entity
    /// </summary>
    /// <param name="values">Array of values where [0] is UniTableBaseAction and [1] is the entity</param>
    /// <param name="targetType">The target type (not used)</param>
    /// <param name="parameter">The converter parameter (not used)</param>
    /// <param name="culture">The culture to use for conversion (not used)</param>
    /// <returns>A tuple containing the action and entity, or null if conversion fails</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 2 && values[0] is UniTableBaseAction action && values[1] != null)
        {
            return Tuple.Create(action, values[1]);
        }

        return null!;
    }

    /// <summary>
    /// Converts back from tuple to multiple values (not implemented)
    /// </summary>
    /// <param name="value">The value to convert back</param>
    /// <param name="targetTypes">The target types</param>
    /// <param name="parameter">The converter parameter</param>
    /// <param name="culture">The culture to use for conversion</param>
    /// <returns>Not implemented</returns>
    /// <exception cref="NotImplementedException">Always thrown as this conversion is not supported</exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}