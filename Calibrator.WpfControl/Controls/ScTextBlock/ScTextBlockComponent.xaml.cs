using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Controls.ScTextBlock;

public partial class ScTextBlockComponent : UserControl
{
    public ScTextBlockComponent()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScTextBlockComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string),
            typeof(ScTextBlockComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TextBlockWidthProperty =
        DependencyProperty.Register(nameof(TextBlockWidth), typeof(double),
            typeof(ScTextBlockComponent), new PropertyMetadata(200.0));

    public static readonly DependencyProperty TextBlockHeightProperty =
        DependencyProperty.Register(nameof(TextBlockHeight), typeof(double),
            typeof(ScTextBlockComponent), new PropertyMetadata(50.0));
    
    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping),
            typeof(ScTextBlockComponent), new PropertyMetadata(TextWrapping.NoWrap));
    
    public static readonly DependencyProperty TextTrimmingProperty =
        DependencyProperty.Register(nameof(TextTrimming), typeof(TextTrimming),
            typeof(ScTextBlockComponent), new PropertyMetadata(TextTrimming.None));

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double TextBlockWidth
    {
        get => (double)GetValue(TextBlockWidthProperty);
        set => SetValue(TextBlockWidthProperty, value);
    }

    public double TextBlockHeight
    {
        get => (double)GetValue(TextBlockHeightProperty);
        set => SetValue(TextBlockHeightProperty, value);
    }
    
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    public bool IsLabelVisible => !string.IsNullOrEmpty(LabelText);
}
