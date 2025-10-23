using System.Windows;

namespace Calibrator.WpfApplication.Views.Components.ScDropdown;

public partial class ScDropdownComponent
{
    public ScDropdownComponent()
    {
        InitializeComponent();
    }

    public new static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScDropdownComponent), new PropertyMetadata(""));

    public static readonly DependencyProperty DropdownWidthProperty =
        DependencyProperty.Register(nameof(Width), typeof(double),
            typeof(ScDropdownComponent), new PropertyMetadata(120.0));

    public static readonly DependencyProperty DropdownHeightProperty =
        DependencyProperty.Register(nameof(Height), typeof(double),
            typeof(ScDropdownComponent), new PropertyMetadata(50.0));

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(object),
            typeof(ScDropdownComponent),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object),
            typeof(ScDropdownComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string),
            typeof(ScDropdownComponent), new PropertyMetadata(""));
    
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool),
            typeof(ScDropdownComponent), new PropertyMetadata(true));

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public double DropdownWidth
    {
        get => (double)GetValue(DropdownWidthProperty);
        set => SetValue(DropdownWidthProperty, value);
    }

    public double DropdownHeight
    {
        get => (double)GetValue(DropdownHeightProperty);
        set => SetValue(DropdownHeightProperty, value);
    }

    public object Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
    
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
    
    public new string IsEnabled
    {
        get => (string)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public bool IsLabelVisible => !string.IsNullOrEmpty(LabelText);
}


