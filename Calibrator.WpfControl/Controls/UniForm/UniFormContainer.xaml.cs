namespace Calibrator.WpfControl.Controls.UniForm;

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

/// <summary>
/// A dynamic form container that generates form controls based on field definitions
/// Supports grid layout with configurable columns and spacing.
/// </summary>
public partial class UniFormContainer : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UniFormContainer"/> class.
    /// </summary>
    public UniFormContainer()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the collection of field definitions for the form.
    /// </summary>
    public object Fields
    {
        get => this.GetValue(FieldsProperty);
        set => this.SetValue(FieldsProperty, value);
    }

    /// <summary>
    /// Gets or sets the data context for binding form fields.
    /// </summary>
    public new object DataContext
    {
        get => this.GetValue(DataContextProperty);
        set => this.SetValue(DataContextProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of columns in the form layout.
    /// </summary>
    public int Columns
    {
        get => (int)this.GetValue(ColumnsProperty);
        set => this.SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal spacing between form controls.
    /// </summary>
    public double HorizontalSpacing
    {
        get => (double)this.GetValue(HorizontalSpacingProperty);
        set => this.SetValue(HorizontalSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical spacing between form controls.
    /// </summary>
    public double VerticalSpacing
    {
        get => (double)this.GetValue(VerticalSpacingProperty);
        set => this.SetValue(VerticalSpacingProperty, value);
    }

    /// <summary>
    /// Identifies the Fields dependency property
    /// </summary>
    public static readonly DependencyProperty FieldsProperty =
        DependencyProperty.Register(nameof(Fields), typeof(object),
            typeof(UniFormContainer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFieldsChanged));

    /// <summary>
    /// Identifies the DataContext dependency property
    /// </summary>
    public new static readonly DependencyProperty DataContextProperty =
        DependencyProperty.Register(nameof(DataContext), typeof(object),
            typeof(UniFormContainer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDataContextChanged));

    /// <summary>
    /// Identifies the Columns dependency property
    /// </summary>
    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int),
            typeof(UniFormContainer), new PropertyMetadata(2, OnLayoutChanged));

    /// <summary>
    /// Identifies the HorizontalSpacing dependency property
    /// </summary>
    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double),
            typeof(UniFormContainer), new PropertyMetadata(16.0));

    /// <summary>
    /// Identifies the VerticalSpacing dependency property
    /// </summary>
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
        this.FormContainer.Children.Clear();
        this.FormContainer.RowDefinitions.Clear();
        this.FormContainer.ColumnDefinitions.Clear();

        // Setup Grid columns
        for (int i = 0; i < this.Columns; i++)
        {
            this.FormContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        var orderedFields = fields
            .Where(f => this.ShouldShowField(f))
            .OrderBy(f => f.Order)
            .ToList();

        int currentRow = 0;
        int currentCol = 0;

        foreach (var field in orderedFields)
        {
            if (field is not UniFormRegularField regularField)
            {
                continue;
            }

            // Add row definition when needed
            if (currentCol == 0)
            {
                this.FormContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var fieldControl = CreateFieldControl(regularField);
            if (fieldControl != null)
            {
                Grid.SetRow(fieldControl, currentRow);
                Grid.SetColumn(fieldControl, currentCol);

                // Set margins for spacing (wrapping stackpanel behavior)
                fieldControl.Margin = new Thickness(
                    currentCol > 0 ? this.HorizontalSpacing / 2 : 0,
                    currentRow > 0 ? this.VerticalSpacing : 0,
                    currentCol < this.Columns - 1 ? this.HorizontalSpacing / 2 : 0,
                    0
                );

                this.FormContainer.Children.Add(fieldControl);
            }

            // Move to next position
            currentCol++;
            if (currentCol >= this.Columns)
            {
                currentCol = 0;
                currentRow++;
            }
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
            _ => null!
        };
    }

    private ScTextBoxComponent CreateScTextBox(UniFormTextField field)
    {
        var textBox = new ScTextBoxComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : string.Empty),
            Placeholder = field.Placeholder ?? string.Empty,
            TextBoxHeight = field.IsMultiline ? 100 : 50,
            Validators = field.Validators ?? [],
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            textBox.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = this.DataContext,
            Mode = field.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
        };

        textBox.SetBinding(ScTextBoxComponent.TextBoxTextProperty, binding);

        return textBox;
    }



    private ScNumericUpDownComponent CreateScNumericUpDown(UniFormNumericField field)
    {
        var numeric = new ScNumericUpDownComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : string.Empty),
            Minimum = field.Minimum,
            Maximum = field.Maximum,
            Step = field.Step,
            ShowButtons = !field.IsReadOnly,
            Validators = field.Validators ?? [],
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            numeric.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = this.DataContext,
            Mode = field.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        };

        numeric.SetBinding(ScNumericUpDownComponent.ValueProperty, binding);

        return numeric;
    }

    private ScCheckBoxComponent CreateScCheckBox(UniFormCheckBoxField field)
    {
        var checkBox = new ScCheckBoxComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : string.Empty),
            CheckBoxContent = string.Empty, // Content je u� v labelu
            IsEnabled = !field.IsReadOnly,
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            checkBox.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = this.DataContext,
            Mode = field.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        };

        checkBox.SetBinding(ScCheckBoxComponent.IsCheckedProperty, binding);

        return checkBox;
    }

    private ScDropdownComponent CreateScDropdown(UniFormComboBoxField field)
    {
        var dropdown = new ScDropdownComponent
        {
            LabelText = field.Label + (field.IsRequired ? " *" : string.Empty),
            Items = field.ItemsSource ?? new object(),
            DisplayMemberPath = field.DisplayMemberPath ?? string.Empty,
            IsEnabled = !field.IsReadOnly,
        };

        if (!string.IsNullOrEmpty(field.ToolTip))
        {
            dropdown.ToolTip = field.ToolTip;
        }

        var binding = new Binding(field.GetPropertyName())
        {
            Source = DataContext,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        };

        dropdown.SetBinding(ScDropdownComponent.SelectedItemProperty, binding);

        return dropdown;
    }
}