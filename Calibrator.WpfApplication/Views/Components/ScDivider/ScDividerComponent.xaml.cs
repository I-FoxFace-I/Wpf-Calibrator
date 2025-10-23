using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfApplication.Views.Components.ScDivider;

public partial class ScDividerComponent : UserControl
{
    public ScDividerComponent()
    {
        InitializeComponent();
    }
    
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(object),
            typeof(ScDividerComponent), new PropertyMetadata(""));
    
    public object Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}


