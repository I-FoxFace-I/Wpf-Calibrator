using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Controls.ScDivider;

/// <summary>
/// A divider component that displays a text header with a styled background
/// </summary>
public partial class ScDividerComponent : UserControl
{
    /// <summary>
    /// Initializes a new instance of the ScDividerComponent class
    /// </summary>
    public ScDividerComponent()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Identifies the Text dependency property
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(object),
            typeof(ScDividerComponent), new PropertyMetadata(""));

    /// <summary>
    /// Gets or sets the text to display in the divider
    /// </summary>
    public object Text
    {
        get => GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }
}