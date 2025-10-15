using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.ScCheckBox;
using Calibrator.WpfControl.Controls.ScDivider;
using Calibrator.WpfControl.Controls.ScDropdown;
using Calibrator.WpfControl.Controls.ScNumericUpDown;
using Calibrator.WpfControl.Controls.ScTextBox;
using Calibrator.WpfControl.Controls.UniForm.Models;

namespace Calibrator.WpfControl.Controls.ScSmartContainer;

public partial class ScSmartContainerComponent : UserControl
{
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

    public object Fields
    {
        get => GetValue(FieldsProperty);
        set => SetValue(FieldsProperty, value);
    }

    public new object DataContext
    {
        get => GetValue(DataContextProperty);
        set => SetValue(DataContextProperty, value);
    }
    
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }
    
    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }
    
    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    public static readonly DependencyProperty FieldsProperty =
        DependencyProperty.Register(nameof(Fields), typeof(object),
            typeof(ScSmartContainerComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFieldsChanged));

    public new static readonly DependencyProperty DataContextProperty =
        DependencyProperty.Register(nameof(DataContext), typeof(object),
            typeof(ScSmartContainerComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDataContextChanged));
    
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int),
            typeof(ScSmartContainerComponent), new PropertyMetadata(2, OnLayoutChanged));
    
    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double),
            typeof(ScSmartContainerComponent), new PropertyMetadata(16.0));
    
    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double),
            typeof(ScSmartContainerComponent), new PropertyMetadata(16.0));

    private static void OnFieldsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScSmartContainerComponent smartContainer)
            return;

        if (e.NewValue is not IEnumerable<UniFormField> newFields)
            return;

        smartContainer.RegenerateForm(newFields);
    }

    private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScSmartContainerComponent smartContainer)
            return;

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

        if (smartContainer.Fields is IEnumerable<UniFormField> fields)
        {
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
            // Add ScDivider for category
            var divider = new ScDividerComponent
            {
                Text = categoryGroup.Key,
                Margin = new Thickness(0, isFirstCategory ? 0 : VerticalSpacing * 1.5, 0, VerticalSpacing)
            };
            FormContainer.Children.Add(divider);
            isFirstCategory = false;

            // Create UniformGrid for items in this category
            var uniformGrid = new UniformGrid
            {
                Columns = Columns,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            foreach (var field in categoryGroup)
            {
                var fieldControl = CreateFieldControl(field);
                if (fieldControl != null)
                {
                    // Set margins for spacing
                    fieldControl.Margin = new Thickness(0, 0, HorizontalSpacing, VerticalSpacing);
                    uniformGrid.Children.Add(fieldControl);
                }
            }

            FormContainer.Children.Add(uniformGrid);
        }
    }

    private bool ShouldShowField(UniFormField field)
    {
        if (field is not UniFormRegularField regularField)
            return true;

        if (regularField.VisibilityCondition == null)
            return true;

        if (DataContext == null)
            return true;

        return regularField.VisibilityCondition(DataContext);
    }

    private FrameworkElement CreateFieldControl(UniFormRegularField field)
    {
        return field switch
        {
            UniFormTextField textField => CreateScTextBox(textField),
            UniFormNumericField numericField => CreateScNumericUpDown(numericField),
            UniFormCheckBoxField checkBoxField => CreateScCheckBox(checkBoxField),
            UniFormComboBoxField comboBoxField => CreateScDropdown(comboBoxField),
            _ => null
        };
    }

    private ScTextBoxComponent CreateScTextBox(UniFormTextField field)
    {
        var textBox = new ScTextBoxComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : ""),
            Placeholder = field.Placeholder ?? string.Empty,
            TextBoxHeight = field.IsMultiline ? 100 : 50,
            Validators = field.Validators
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
            Validators = field.Validators
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
            LabelText = field.Label + (field.IsRequired ? " *" : ""),
            CheckBoxContent = string.Empty,
            IsEnabled = !field.IsReadOnly
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
            Items = field.ItemsSource,
            DisplayMemberPath = field.DisplayMemberPath ?? string.Empty,
            IsEnabled = !field.IsReadOnly
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

