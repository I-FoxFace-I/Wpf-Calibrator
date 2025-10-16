using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Controls.ScCheckBox;

/// <summary>
/// A customizable checkbox component with label support
/// </summary>
public partial class ScCheckBoxComponent : UserControl
{
    /// <summary>
    /// Initializes a new instance of the ScCheckBoxComponent class
    /// </summary>
    public ScCheckBoxComponent()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Identifies the LabelText dependency property
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScCheckBoxComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the CheckBoxContent dependency property
    /// </summary>
    public static readonly DependencyProperty CheckBoxContentProperty =
        DependencyProperty.Register(nameof(CheckBoxContent), typeof(string),
            typeof(ScCheckBoxComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the IsChecked dependency property
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool?),
            typeof(ScCheckBoxComponent),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Identifies the IsEnabled dependency property
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool),
            typeof(ScCheckBoxComponent), new PropertyMetadata(true));

    /// <summary>
    /// Label displayed above the checkbox (optional)
    /// </summary>
    public string LabelText
    {
        get => (string)this.GetValue(LabelTextProperty);
        set => this.SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// Content displayed next to the checkbox
    /// </summary>
    public string CheckBoxContent
    {
        get => (string)this.GetValue(CheckBoxContentProperty);
        set => this.SetValue(CheckBoxContentProperty, value);
    }

    /// <summary>
    /// Checked state of the checkbox
    /// </summary>
    public bool? IsChecked
    {
        get => (bool?)this.GetValue(IsCheckedProperty);
        set => this.SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// Whether the checkbox is enabled
    /// </summary>
    public new bool IsEnabled
    {
        get => (bool)this.GetValue(IsEnabledProperty);
        set => this.SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Gets whether the label should be visible based on whether LabelText has content
    /// </summary>
    public bool IsLabelVisible => !string.IsNullOrEmpty(LabelText);
}