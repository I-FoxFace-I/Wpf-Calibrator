using Calibrator.WpfControl.Controls.Base;
using Calibrator.WpfControl.Validation;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calibrator.WpfControl.Controls.ScNumericUpDown;

public partial class ScNumericUpDownComponent : UserControl
{
    private static readonly Regex NumericRegex = new Regex(@"^-?\d*\.?\d*$", RegexOptions.Compiled);
    
    public ScNumericUpDownComponent()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double?),
            typeof(ScNumericUpDownComponent), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double?),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double?),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(1.0));

    public static readonly DependencyProperty NumericWidthProperty =
        DependencyProperty.Register(nameof(NumericWidth), typeof(double),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(200.0));

    public static readonly DependencyProperty NumericHeightProperty =
        DependencyProperty.Register(nameof(NumericHeight), typeof(double),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(50.0));
    
    public static readonly DependencyProperty ShowButtonsProperty =
        DependencyProperty.Register(nameof(ShowButtons), typeof(bool),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(true));

    public static readonly DependencyProperty ValidatorsProperty =
        DependencyProperty.Register(nameof(Validators), typeof(List<IValidator<object>>),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(nameof(ValidationError), typeof(string),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(true));

    /// <summary>
    /// Collection of validators to apply
    /// </summary>
    public List<IValidator<object>> Validators
    {
        get => (List<IValidator<object>>)GetValue(ValidatorsProperty);
        set => SetValue(ValidatorsProperty, value);
    }

    /// <summary>
    /// Current validation error message
    /// </summary>
    public string ValidationError
    {
        get => (string)GetValue(ValidationErrorProperty);
        protected set => SetValue(ValidationErrorProperty, value);
    }

    /// <summary>
    /// Indicates whether the current value is valid
    /// </summary>
    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        protected set => SetValue(IsValidProperty, value);
    }

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double? Minimum
    {
        get => (double?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double? Maximum
    {
        get => (double?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public double NumericWidth
    {
        get => (double)GetValue(NumericWidthProperty);
        set => SetValue(NumericWidthProperty, value);
    }

    public double NumericHeight
    {
        get => (double)GetValue(NumericHeightProperty);
        set => SetValue(NumericHeightProperty, value);
    }
    
    public bool ShowButtons
    {
        get => (bool)GetValue(ShowButtonsProperty);
        set => SetValue(ShowButtonsProperty, value);
    }

    public bool IsLabelVisible => !string.IsNullOrEmpty(LabelText);

    /// <summary>
    /// Validates the provided value against all validators
    /// </summary>
    /// <param name="value">Value to validate</param>
    protected void ValidateValue(object value)
    {
        if (Validators == null || !Validators.Any())
        {
            SetValidationState(true, string.Empty);
            return;
        }

        foreach (var validator in Validators)
        {
            var result = validator.Validate(value);
            if (!result.IsValid)
            {
                SetValidationState(false, result.ErrorMessage);
                return;
            }
        }

        SetValidationState(true, string.Empty);
    }

    /// <summary>
    /// Sets the validation state
    /// </summary>
    private void SetValidationState(bool isValid, string errorMessage)
    {
        IsValid = isValid;
        ValidationError = errorMessage;
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(ValidationError));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScNumericUpDownComponent control)
        {
            control.CoerceValue();
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        ValidateAndUpdateValue();
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as System.Windows.Controls.TextBox;
        var newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
        e.Handled = !NumericRegex.IsMatch(newText);
    }

    private void OnIncrementClick(object sender, RoutedEventArgs e)
    {
        if (Value.HasValue)
            Value = Value.Value + Step;
        else
            Value = Minimum ?? 0;
        
        CoerceValue();
        ValidateValue(Value);
    }

    private void OnDecrementClick(object sender, RoutedEventArgs e)
    {
        if (Value.HasValue)
            Value = Value.Value - Step;
        else
            Value = Maximum ?? 0;
        
        CoerceValue();
        ValidateValue(Value);
    }

    private void ValidateAndUpdateValue()
    {
        if (double.TryParse(NumericTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            Value = result;
            CoerceValue();
        }
        else
        {
            NumericTextBox.Text = Value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }
        
        ValidateValue(Value);
    }

    private void CoerceValue()
    {
        if (!Value.HasValue)
            return;

        if (Minimum.HasValue && Value < Minimum)
            Value = Minimum;
        
        if (Maximum.HasValue && Value > Maximum)
            Value = Maximum;
    }
}
