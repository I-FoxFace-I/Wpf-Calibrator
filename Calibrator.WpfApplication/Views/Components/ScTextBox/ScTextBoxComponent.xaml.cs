using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfApplication.Views.Components.ScTextBox;

public partial class ScTextBoxComponent
{
    public ScTextBoxComponent()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(object),
            typeof(ScTextBoxComponent), new PropertyMetadata(""));
    
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScTextBoxComponent), new PropertyMetadata(""));

    public static readonly DependencyProperty TextBoxWidthProperty =
        DependencyProperty.Register(nameof(TextBoxWidth), typeof(double),
            typeof(ScTextBoxComponent), new PropertyMetadata(120.0));

    public static readonly DependencyProperty TextBoxHeightProperty =
        DependencyProperty.Register(nameof(TextBoxHeight), typeof(double),
            typeof(ScTextBoxComponent), new PropertyMetadata(50.0));
    
    public static readonly DependencyProperty TextBoxTextProperty =
        DependencyProperty.Register(nameof(TextBoxText), typeof(object),
            typeof(ScTextBoxComponent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


    public object Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public double TextBoxWidth
    {
        get => (double)GetValue(TextBoxWidthProperty);
        set => SetValue(TextBoxWidthProperty, value);
    }

    public double TextBoxHeight
    {
        get => (double)GetValue(TextBoxHeightProperty);
        set => SetValue(TextBoxHeightProperty, value);
    }
    
    public object TextBoxText
    {
        get => GetValue(TextBoxTextProperty);
        set => SetValue(TextBoxTextProperty, value);
    }

    public bool IsLabelVisible => !string.IsNullOrEmpty(LabelText);
}


