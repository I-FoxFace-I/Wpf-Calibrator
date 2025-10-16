using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Controls.ScTextBlock;

/// <summary>
/// A read-only text display component with label support
/// </summary>
public partial class ScTextBlockComponent : UserControl
{
    /// <summary>
    /// Initializes a new instance of the ScTextBlockComponent class
    /// </summary>
    public ScTextBlockComponent()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Identifies the LabelText dependency property
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScTextBlockComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the Text dependency property
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(ScTextBlockComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the TextBlockWidth dependency property
    /// </summary>
    public static readonly DependencyProperty TextBlockWidthProperty =
        DependencyProperty.Register(nameof(TextBlockWidth), typeof(double),
            typeof(ScTextBlockComponent), new PropertyMetadata(200.0));

    /// <summary>
    /// Identifies the TextBlockHeight dependency property
    /// </summary>
    public static readonly DependencyProperty TextBlockHeightProperty =
        DependencyProperty.Register(nameof(TextBlockHeight), typeof(double),
            typeof(ScTextBlockComponent), new PropertyMetadata(50.0));

    /// <summary>
    /// Identifies the TextWrapping dependency property
    /// </summary>
    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping),
            typeof(ScTextBlockComponent), new PropertyMetadata(TextWrapping.NoWrap));

    /// <summary>
    /// Identifies the TextTrimming dependency property
    /// </summary>
    public static readonly DependencyProperty TextTrimmingProperty =
        DependencyProperty.Register(nameof(TextTrimming), typeof(TextTrimming),
            typeof(ScTextBlockComponent), new PropertyMetadata(TextTrimming.None));

    /// <summary>
    /// Gets or sets the label text displayed above the text block
    /// </summary>
    public string LabelText
    {
        get => (string)this.GetValue(LabelTextProperty);
        set => this.SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text content to display
    /// </summary>
    public string Text
    {
        get => (string)this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the text block control
    /// </summary>
    public double TextBlockWidth
    {
        get => (double)this.GetValue(TextBlockWidthProperty);
        set => this.SetValue(TextBlockWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the text block control
    /// </summary>
    public double TextBlockHeight
    {
        get => (double)this.GetValue(TextBlockHeightProperty);
        set => this.SetValue(TextBlockHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets how text should wrap within the control
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)this.GetValue(TextWrappingProperty);
        set => this.SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// Gets or sets how text should be trimmed when it exceeds the available space
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)this.GetValue(TextTrimmingProperty);
        set => this.SetValue(TextTrimmingProperty, value);
    }

    /// <summary>
    /// Gets whether the label should be visible based on whether LabelText has content
    /// </summary>
    public bool IsLabelVisible => !string.IsNullOrEmpty(this.LabelText);
}