using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Controls.ScCheckBox;

public partial class ScCheckBoxComponent : UserControl
{
    public ScCheckBoxComponent()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScCheckBoxComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CheckBoxContentProperty =
        DependencyProperty.Register(nameof(CheckBoxContent), typeof(string),
            typeof(ScCheckBoxComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool?),
            typeof(ScCheckBoxComponent),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool),
            typeof(ScCheckBoxComponent), new PropertyMetadata(true));

    /// <summary>
    /// Label displayed above the checkbox (optional)
    /// </summary>
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// Content displayed next to the checkbox
    /// </summary>
    public string CheckBoxContent
    {
        get => (string)GetValue(CheckBoxContentProperty);
        set => SetValue(CheckBoxContentProperty, value);
    }

    /// <summary>
    /// Checked state of the checkbox
    /// </summary>
    public bool? IsChecked
    {
        get => (bool?)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// Whether the checkbox is enabled
    /// </summary>
    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public bool IsLabelVisible => !string.IsNullOrEmpty(LabelText);
}