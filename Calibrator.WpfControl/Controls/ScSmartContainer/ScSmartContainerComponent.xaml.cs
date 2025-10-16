namespace Calibrator.WpfControl.Controls.ScSmartContainer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.ScCheckBox;
using Calibrator.WpfControl.Controls.ScDivider;
using Calibrator.WpfControl.Controls.ScDropdown;
using Calibrator.WpfControl.Controls.ScNumericUpDown;
using Calibrator.WpfControl.Controls.ScTextBlock;
using Calibrator.WpfControl.Controls.ScTextBox;
using Calibrator.WpfControl.Controls.UniForm.Models;
using Telerik.Windows.Controls;

/// <summary>
/// A smart container component that dynamically generates form controls based on field definitions
/// Supports categorization with expandable sections
/// </summary>
public partial class ScSmartContainerComponent : UserControl
{
    /// <summary>
    /// Initializes a new instance of the ScSmartContainerComponent class
    /// </summary>
    public ScSmartContainerComponent()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Regenerate form when loaded to ensure proper sizing
        if (Fields is IEnumerable<UniFormField> fields)
        {
            RegenerateForm(fields);
        }
    }

    /// <summary>
    /// Gets or sets the collection of field definitions for the form
    /// </summary>
    public object Fields
    {
        get => GetValue(FieldsProperty);
        set => this.SetValue(FieldsProperty, value);
    }

    /// <summary>
    /// Gets or sets the data context for binding form fields
    /// </summary>
    public new object DataContext
    {
        get => GetValue(DataContextProperty);
        set => this.SetValue(DataContextProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of columns in the form layout
    /// </summary>
    public int Columns
    {
        get => (int)this.GetValue(ColumnsProperty);
        set => this.SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal spacing between form controls
    /// </summary>
    public double HorizontalSpacing
    {
        get => (double)this.GetValue(HorizontalSpacingProperty);
        set => this.SetValue(HorizontalSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical spacing between form controls
    /// </summary>
    public double VerticalSpacing
    {
        get => (double)this.GetValue(VerticalSpacingProperty);
        set => this.SetValue(VerticalSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of each form control item
    /// </summary>
    public double ItemWidth
    {
        get => (double)this.GetValue(ItemWidthProperty);
        set => this.SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// Identifies the Fields dependency property
    /// </summary>
    public static readonly DependencyProperty FieldsProperty =
        DependencyProperty.Register(nameof(Fields), typeof(object),
            typeof(ScSmartContainerComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFieldsChanged));

    /// <summary>
    /// Identifies the DataContext dependency property
    /// </summary>
    public new static readonly DependencyProperty DataContextProperty =
        DependencyProperty.Register(nameof(DataContext), typeof(object),
            typeof(ScSmartContainerComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDataContextChanged));

    /// <summary>
    /// Identifies the Columns dependency property
    /// </summary>
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int),
            typeof(ScSmartContainerComponent), new PropertyMetadata(2, OnLayoutChanged));

    /// <summary>
    /// Identifies the HorizontalSpacing dependency property
    /// </summary>
    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double),
            typeof(ScSmartContainerComponent), new PropertyMetadata(16.0));

    /// <summary>
    /// Identifies the VerticalSpacing dependency property
    /// </summary>
    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double),
            typeof(ScSmartContainerComponent), new PropertyMetadata(16.0));

    /// <summary>
    /// Identifies the ItemWidth dependency property
    /// </summary>
    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double),
            typeof(ScSmartContainerComponent), new PropertyMetadata(300.0));

    private static void OnFieldsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScSmartContainerComponent smartContainer)
        {
            return;
        }

        if (e.NewValue is not IEnumerable<UniFormField> newFields)
        {
            return;
        }

        smartContainer.RegenerateForm(newFields);
    }

    private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScSmartContainerComponent smartContainer)
        {
            return;
        }

        // Regenerate form when DataContext changes (for visibility conditions)
        if (smartContainer.Fields is IEnumerable<UniFormField> fields)
        {
            smartContainer.RegenerateForm(fields);
        }
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScSmartContainerComponent smartContainer)
            return;

        // Layout properties changed - need to regenerate form with new layout settings
        if (smartContainer.Fields is IEnumerable<UniFormField> fields)
        {
            // Clear existing layout first for layout changes
            smartContainer.FormContainer.Children.Clear();
            smartContainer.RegenerateForm(fields);
        }
    }

    private void RegenerateForm(IEnumerable<UniFormField> fields)
    {
        FormContainer.Children.Clear();

        var orderedFields = fields
            .Where(f => ShouldShowField(f))
            .OrderBy(f => f.Order)
            .ToList();

        // Group by category
        var groupedByCategory = orderedFields
            .OfType<UniFormRegularField>()
            .GroupBy(f => f.Category ?? "Default")
            .OrderBy(g => g.Key);

        bool isFirstCategory = true;

        foreach (var categoryGroup in groupedByCategory)
        {
            // Create ScDivider as Header
            var divider = new ScDividerComponent
            {
                Text = categoryGroup.Key
            };

            // Create WrapPanel for items in this category
            var wrapPanel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, VerticalSpacing, 0, 0)
            };

            foreach (var field in categoryGroup)
            {
                var fieldControl = CreateFieldControl(field);
                if (fieldControl != null)
                {
                    // Set fixed width and margins for consistent layout
                    fieldControl.Width = ItemWidth;
                    fieldControl.Margin = new Thickness(0, 0, HorizontalSpacing, VerticalSpacing);
                    wrapPanel.Children.Add(fieldControl);
                }
            }

            // Create RadExpander with ScDivider as Header and WrapPanel as Content
            var expander = new RadExpander
            {
                Header = divider,
                Content = wrapPanel,
                IsExpanded = true, // Default expanded
                Margin = new Thickness(0, isFirstCategory ? 0 : VerticalSpacing * 1.5, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            FormContainer.Children.Add(expander);
            isFirstCategory = false;
        }
    }

    private bool ShouldShowField(UniFormField field)
    {
        if (field is not UniFormRegularField regularField)
        {
            return true;
        }

        if (regularField.VisibilityCondition == null)
        {
            return true;
        }

        if (this.DataContext == null)
        {
            return true;
        }

        return regularField.VisibilityCondition(this.DataContext);
    }

    private FrameworkElement CreateFieldControl(UniFormRegularField field)
    {
        return field switch
        {
            UniFormTextField textField => CreateScTextBox(textField),
            UniFormNumericField numericField => CreateScNumericUpDown(numericField),
            UniFormCheckBoxField checkBoxField => CreateScCheckBox(checkBoxField),
            UniFormComboBoxField comboBoxField => CreateScDropdown(comboBoxField),
            _ => CreateScTextBlock(field)
        };
    }

    private ScTextBlockComponent CreateScTextBlock(UniFormField field)
    {
        var textBox = new ScTextBlockComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : ""),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            textBox.ToolTip = field.ToolTip;
        }

        if (field is UniFormRegularField uniFormRegularField)
        {
            var binding = new Binding(uniFormRegularField.GetPropertyName())
            {
                Source = DataContext,
                Mode = BindingMode.OneWay,
            };

            textBox.SetBinding(ScTextBlockComponent.TextProperty, binding);
        }
        return textBox;
    }

    private ScTextBoxComponent CreateScTextBox(UniFormTextField field)
    {
        var textBox = new ScTextBoxComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : ""),
            Placeholder = field.Placeholder ?? string.Empty,
            TextBoxHeight = field.IsMultiline ? 100 : 50,
            Validators = field.Validators ?? [],
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            textBox.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = DataContext,
            Mode = field.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
        };

        textBox.SetBinding(ScTextBoxComponent.TextBoxTextProperty, binding);

        return textBox;
    }

    private ScNumericUpDownComponent CreateScNumericUpDown(UniFormNumericField field)
    {
        var numeric = new ScNumericUpDownComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : ""),
            Minimum = field.Minimum,
            Maximum = field.Maximum,
            Step = field.Step,
            ShowButtons = !field.IsReadOnly,
            Validators = field.Validators ?? [],
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            numeric.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = DataContext,
            Mode = field.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        numeric.SetBinding(ScNumericUpDownComponent.ValueProperty, binding);

        return numeric;
    }

    private ScCheckBoxComponent CreateScCheckBox(UniFormCheckBoxField field)
    {
        var checkBox = new ScCheckBoxComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : string.Empty),
            CheckBoxContent = string.Empty,
            IsEnabled = !field.IsReadOnly,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            checkBox.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = DataContext,
            Mode = field.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        checkBox.SetBinding(ScCheckBoxComponent.IsCheckedProperty, binding);

        return checkBox;
    }

    private ScDropdownComponent CreateScDropdown(UniFormComboBoxField field)
    {
        var dropdown = new ScDropdownComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : ""),
            Items = field.ItemsSource!,
            DisplayMemberPath = field.DisplayMemberPath ?? string.Empty,
            IsEnabled = !field.IsReadOnly,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            dropdown.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = DataContext,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        dropdown.SetBinding(ScDropdownComponent.SelectedItemProperty, binding);

        return dropdown;
    }
}