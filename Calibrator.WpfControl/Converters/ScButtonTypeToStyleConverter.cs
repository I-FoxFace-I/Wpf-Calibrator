using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.ScButton;

namespace Calibrator.WpfControl.Converters;

/// <summary>
/// Converts ScButtonType enum values to corresponding WPF Style resources
/// </summary>
public class ScButtonTypeToStyleConverter : IValueConverter
{
    /// <summary>
    /// Converts a ScButtonType value to the appropriate Style resource
    /// </summary>
    /// <param name="value">The ScButtonType value to convert</param>
    /// <param name="targetType">The target type (not used)</param>
    /// <param name="parameter">The converter parameter (not used)</param>
    /// <param name="culture">The culture to use for conversion (not used)</param>
    /// <returns>The Style resource corresponding to the button type</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ScButtonType buttonType)
            return GetStyle("ScRegularButton")!;

        return buttonType switch
        {
            ScButtonType.Accent => GetStyle("AccentButton")!,
            ScButtonType.Transparent => GetStyle("TransparentButton")!,
            ScButtonType.Regular => GetStyle("ScRegularButton")!,
            _ => GetStyle("ScRegularButton")!
        };
    }

    /// <summary>
    /// Converts back from Style to ScButtonType (not implemented).
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture to use for conversion.</param>
    /// <returns>Not implemented.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this conversion is not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves a Style resource by key from the application resources
    /// </summary>
    /// <param name="key">The resource key to look up</param>
    /// <returns>The Style resource if found, otherwise null</returns>
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
        catch (InvalidOperationException)
        {
            // Handle cases where resource system is not available
            return null;
        }
        catch (System.Windows.Markup.XamlParseException)
        {
            // Handle malformed XAML resources
            return null;
        }
    }
}