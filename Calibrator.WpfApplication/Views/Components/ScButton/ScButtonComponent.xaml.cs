using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.IconPacks;

namespace Calibrator.WpfApplication.Views.Components.ScButton;

public partial class ScButtonComponent
{
    public ScButtonComponent()
    {
        InitializeComponent();
    }

    public new static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object),
            typeof(ScButtonComponent), new PropertyMetadata("NoText"));
    
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand),
            typeof(ScButtonComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind),
            typeof(ScButtonComponent), new PropertyMetadata(default(PackIconMaterialKind)));

    public static readonly DependencyProperty UppercaseTextProperty =
        DependencyProperty.Register(nameof(UppercaseText), typeof(bool),
            typeof(ScButtonComponent), new PropertyMetadata(true));

    public static readonly DependencyProperty ButtonWidthProperty =
        DependencyProperty.Register(nameof(ButtonWidth), typeof(double),
            typeof(ScButtonComponent), new PropertyMetadata(135.0));

    public static readonly DependencyProperty ButtonHeightProperty =
        DependencyProperty.Register(nameof(ButtonHeight), typeof(double),
            typeof(ScButtonComponent), new PropertyMetadata(40.0));

    public static readonly DependencyProperty ButtonTypeProperty =
        DependencyProperty.Register(nameof(ButtonType), typeof(ScButtonType),
            typeof(ScButtonComponent), new PropertyMetadata(ScButtonType.Regular));


    public new object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public PackIconMaterialKind IconKind
    {
        get => (PackIconMaterialKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public bool UppercaseText
    {
        get => (bool)GetValue(UppercaseTextProperty);
        set => SetValue(UppercaseTextProperty, value);
    }

    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

    public double ButtonHeight
    {
        get => (double)GetValue(ButtonHeightProperty);
        set => SetValue(ButtonHeightProperty, value);
    }

    public ScButtonType ButtonType
    {
        get => (ScButtonType)GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
}


