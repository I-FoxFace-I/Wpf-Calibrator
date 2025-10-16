using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Calibrator.WpfControl.Controls.ScButton;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfControl.Controls.ScButton;

/// <summary>
/// A customizable button component with icon support and various styling options
/// </summary>
public partial class ScButtonComponent : UserControl
{
    /// <summary>
    /// Initializes a new instance of the ScButtonComponent class
    /// </summary>
    public ScButtonComponent()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Identifies the Content dependency property
    /// </summary>
    public new static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object),
            typeof(ScButtonComponent), new PropertyMetadata("NoText"));

    /// <summary>
    /// Identifies the Command dependency property
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand),
            typeof(ScButtonComponent), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the IconKind dependency property
    /// </summary>
    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind),
            typeof(ScButtonComponent), new PropertyMetadata(default(PackIconMaterialKind)));

    /// <summary>
    /// Identifies the UppercaseText dependency property
    /// </summary>
    public static readonly DependencyProperty UppercaseTextProperty =
        DependencyProperty.Register(nameof(UppercaseText), typeof(bool),
            typeof(ScButtonComponent), new PropertyMetadata(true));

    /// <summary>
    /// Identifies the ButtonWidth dependency property
    /// </summary>
    public static readonly DependencyProperty ButtonWidthProperty =
        DependencyProperty.Register(nameof(ButtonWidth), typeof(double),
            typeof(ScButtonComponent), new PropertyMetadata(135.0));

    /// <summary>
    /// Identifies the ButtonHeight dependency property
    /// </summary>
    public static readonly DependencyProperty ButtonHeightProperty =
        DependencyProperty.Register(nameof(ButtonHeight), typeof(double),
            typeof(ScButtonComponent), new PropertyMetadata(40.0));

    /// <summary>
    /// Identifies the ButtonType dependency property
    /// </summary>
    public static readonly DependencyProperty ButtonTypeProperty =
        DependencyProperty.Register(nameof(ButtonType), typeof(ScButtonType),
            typeof(ScButtonComponent), new PropertyMetadata(ScButtonType.Regular));


    /// <summary>
    /// Gets or sets the content to display on the button
    /// </summary>
    public new object Content
    {
        get => GetValue(ContentProperty);
        set => this.SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the button is clicked
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);
        set => this.SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the Material Design icon to display on the button
    /// </summary>
    public PackIconMaterialKind IconKind
    {
        get => (PackIconMaterialKind)this.GetValue(IconKindProperty);
        set => this.SetValue(IconKindProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the button text should be displayed in uppercase
    /// </summary>
    public bool UppercaseText
    {
        get => (bool)this.GetValue(UppercaseTextProperty);
        set => this.SetValue(UppercaseTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the button
    /// </summary>
    public double ButtonWidth
    {
        get => (double)this.GetValue(ButtonWidthProperty);
        set => this.SetValue(ButtonWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the button
    /// </summary>
    public double ButtonHeight
    {
        get => (double)this.GetValue(ButtonHeightProperty);
        set => this.SetValue(ButtonHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the visual style type of the button
    /// </summary>
    public ScButtonType ButtonType
    {
        get => (ScButtonType)this.GetValue(ButtonTypeProperty);
        set => this.SetValue(ButtonTypeProperty, value);
    }
}