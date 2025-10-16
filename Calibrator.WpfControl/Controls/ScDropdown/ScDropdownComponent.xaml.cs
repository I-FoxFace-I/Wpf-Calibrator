using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Controls.ScDropdown;

/// <summary>
/// A dropdown component based on Telerik RadComboBox with label support
/// </summary>
public partial class ScDropdownComponent : UserControl
{
    /// <summary>
    /// Initializes a new instance of the ScDropdownComponent class
    /// </summary>
    public ScDropdownComponent()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Identifies the LabelText dependency property
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScDropdownComponent), new PropertyMetadata(""));

    /// <summary>
    /// Identifies the DropdownWidth dependency property
    /// </summary>
    public static readonly DependencyProperty DropdownWidthProperty =
        DependencyProperty.Register(nameof(Width), typeof(double),
            typeof(ScDropdownComponent), new PropertyMetadata(120.0));

    /// <summary>
    /// Identifies the DropdownHeight dependency property
    /// </summary>
    public static readonly DependencyProperty DropdownHeightProperty =
        DependencyProperty.Register(nameof(Height), typeof(double),
            typeof(ScDropdownComponent), new PropertyMetadata(50.0));

    /// <summary>
    /// Identifies the Items dependency property
    /// </summary>
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(object),
            typeof(ScDropdownComponent),
            new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the SelectedItem dependency property
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object),
            typeof(ScDropdownComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Identifies the DisplayMemberPath dependency property
    /// </summary>
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string),
            typeof(ScDropdownComponent), new PropertyMetadata(""));

    /// <summary>
    /// Identifies the IsEnabled dependency property
    /// </summary>
    public new static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(nameof(IsEnabled), typeof(bool),
            typeof(ScDropdownComponent), new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets the label text displayed above the dropdown
    /// </summary>
    public string LabelText
    {
        get => (string)this.GetValue(LabelTextProperty);
        set => this.SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the dropdown control
    /// </summary>
    public double DropdownWidth
    {
        get => (double)this.GetValue(DropdownWidthProperty);
        set => this.SetValue(DropdownWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the dropdown control
    /// </summary>
    public double DropdownHeight
    {
        get => (double)this.GetValue(DropdownHeightProperty);
        set => this.SetValue(DropdownHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of items to display in the dropdown
    /// </summary>
    public object Items
    {
        get => this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item in the dropdown
    /// </summary>
    public object SelectedItem
    {
        get => this.GetValue(SelectedItemProperty);
        set => this.SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the path to the property that should be displayed for each item
    /// </summary>
    public string DisplayMemberPath
    {
        get => (string)this.GetValue(DisplayMemberPathProperty);
        set => this.SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the dropdown is enabled for user interaction
    /// </summary>
    public new bool IsEnabled
    {
        get => (bool)this.GetValue(IsEnabledProperty);
        set => this.SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Gets whether the label should be visible based on whether LabelText has content
    /// </summary>
    public bool IsLabelVisible => !string.IsNullOrEmpty(this.LabelText);
}