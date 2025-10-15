using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Calibrator.WpfControl.Controls.ScCheckBox;
using Calibrator.WpfControl.Controls.ScDropdown;
using Calibrator.WpfControl.Controls.ScNumericUpDown;
using Calibrator.WpfControl.Controls.ScTextBox;
using Calibrator.WpfControl.Controls.UniForm.Models;

namespace Calibrator.WpfControl.Controls.UniForm;

public partial class UniFormContainer : UserControl
{
    public UniFormContainer()
    {
        InitializeComponent();
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
            typeof(UniFormContainer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFieldsChanged));

    public new static readonly DependencyProperty DataContextProperty =
        DependencyProperty.Register(nameof(DataContext), typeof(object),
            typeof(UniFormContainer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDataContextChanged));
    
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int),
            typeof(UniFormContainer), new PropertyMetadata(2, OnLayoutChanged));
    
    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double),
            typeof(UniFormContainer), new PropertyMetadata(16.0));
    
    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double),
            typeof(UniFormContainer), new PropertyMetadata(16.0));

    private static void OnFieldsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UniFormContainer uniForm)
            return;

        if (e.NewValue is not IEnumerable<UniFormField> newFields)
            return;

        uniForm.RegenerateForm(newFields);
    }

    private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UniFormContainer uniForm)
            return;

        // Regenerate form when DataContext changes (for visibility conditions)
        if (uniForm.Fields is IEnumerable<UniFormField> fields)
        {
            uniForm.RegenerateForm(fields);
        }
    }
    
    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UniFormContainer uniForm)
            return;

        if (uniForm.Fields is IEnumerable<UniFormField> fields)
        {
            uniForm.RegenerateForm(fields);
        }
    }

    private void RegenerateForm(IEnumerable<UniFormField> fields)
    {
        FormContainer.Children.Clear();
        FormContainer.RowDefinitions.Clear();
        FormContainer.ColumnDefinitions.Clear();

        // Setup Grid columns
        for (int i = 0; i < Columns; i++)
        {
            FormContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        var orderedFields = fields
            .Where(f => ShouldShowField(f))
            .OrderBy(f => f.Order)
            .ToList();

        int currentRow = 0;
        int currentCol = 0;

        foreach (var field in orderedFields)
        {
            if (field is not UniFormRegularField regularField)
                continue;

            // Add row definition when needed
            if (currentCol == 0)
            {
                FormContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var fieldControl = CreateFieldControl(regularField);
            if (fieldControl != null)
            {
                Grid.SetRow(fieldControl, currentRow);
                Grid.SetColumn(fieldControl, currentCol);

                // Set margins for spacing (wrapping stackpanel behavior)
                fieldControl.Margin = new Thickness(
                    currentCol > 0 ? HorizontalSpacing / 2 : 0,
                    currentRow > 0 ? VerticalSpacing : 0,
                    currentCol < Columns - 1 ? HorizontalSpacing / 2 : 0,
                    0
                );

                FormContainer.Children.Add(fieldControl);
            }

            // Move to next position
            currentCol++;
            if (currentCol >= Columns)
            {
                currentCol = 0;
                currentRow++;
            }
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
            CheckBoxContent = string.Empty, // Content je už v labelu
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
