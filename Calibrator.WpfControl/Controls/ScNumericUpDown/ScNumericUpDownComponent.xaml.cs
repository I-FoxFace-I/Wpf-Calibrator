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

/// <summary>
/// A numeric input component with up/down buttons and validation support
/// </summary>
public partial class ScNumericUpDownComponent : UserControl
{
    private static readonly Regex NumericRegex = new Regex(@"^-?\d*\.?\d*$", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the ScNumericUpDownComponent class
    /// </summary>
    public ScNumericUpDownComponent()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Identifies the LabelText dependency property
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the Value dependency property
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double?),
            typeof(ScNumericUpDownComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    /// <summary>
    /// Identifies the Minimum dependency property
    /// </summary>
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double?),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the Maximum dependency property
    /// </summary>
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double?),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the Step dependency property
    /// </summary>
    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(1.0));

    /// <summary>
    /// Identifies the NumericWidth dependency property
    /// </summary>
    public static readonly DependencyProperty NumericWidthProperty =
        DependencyProperty.Register(nameof(NumericWidth), typeof(double),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(200.0));

    /// <summary>
    /// Identifies the NumericHeight dependency property
    /// </summary>
    public static readonly DependencyProperty NumericHeightProperty =
        DependencyProperty.Register(nameof(NumericHeight), typeof(double),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(50.0));

    /// <summary>
    /// Identifies the ShowButtons dependency property
    /// </summary>
    public static readonly DependencyProperty ShowButtonsProperty =
        DependencyProperty.Register(nameof(ShowButtons), typeof(bool),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(true));

    /// <summary>
    /// Identifies the Validators dependency property
    /// </summary>
    public static readonly DependencyProperty ValidatorsProperty =
        DependencyProperty.Register(nameof(Validators), typeof(ICollection<IValidator<object>>),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the ValidationError dependency property
    /// </summary>
    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(nameof(ValidationError), typeof(string),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the IsValid dependency property
    /// </summary>
    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(ScNumericUpDownComponent), new PropertyMetadata(true));

    /// <summary>
    /// Collection of validators to apply
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public ICollection<IValidator<object>>? Validators
    {
        get => (ICollection<IValidator<object>>?)this.GetValue(ValidatorsProperty);
        set => this.SetValue(ValidatorsProperty, value);
    }
#pragma warning restore CA2227

    /// <summary>
    /// Current validation error message
    /// </summary>
    public string ValidationError
    {
        get => (string)this.GetValue(ValidationErrorProperty);
        protected set => this.SetValue(ValidationErrorProperty, value);
    }

    /// <summary>
    /// Indicates whether the current value is valid
    /// </summary>
    public bool IsValid
    {
        get => (bool)this.GetValue(IsValidProperty);
        protected set => this.SetValue(IsValidProperty, value);
    }

    /// <summary>
    /// Gets or sets the label text displayed above the numeric input
    /// </summary>
    public string LabelText
    {
        get => (string)this.GetValue(LabelTextProperty);
        set => this.SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the current numeric value
    /// </summary>
#pragma warning disable CA1721 // Property names should not match method names
    public double? Value
    {
        get => (double?)this.GetValue(ValueProperty);
        set => this.SetValue(ValueProperty, value);
    }
#pragma warning restore CA1721

    /// <summary>
    /// Gets or sets the minimum allowed value
    /// </summary>
    public double? Minimum
    {
        get => (double?)this.GetValue(MinimumProperty);
        set => this.SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum allowed value
    /// </summary>
    public double? Maximum
    {
        get => (double?)this.GetValue(MaximumProperty);
        set => this.SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Gets or sets the increment/decrement step value
    /// </summary>
    public double Step
    {
        get => (double)this.GetValue(StepProperty);
        set => this.SetValue(StepProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the numeric input control
    /// </summary>
    public double NumericWidth
    {
        get => (double)this.GetValue(NumericWidthProperty);
        set => this.SetValue(NumericWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the numeric input control
    /// </summary>
    public double NumericHeight
    {
        get => (double)this.GetValue(NumericHeightProperty);
        set => this.SetValue(NumericHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show the up/down buttons
    /// </summary>
    public bool ShowButtons
    {
        get => (bool)this.GetValue(ShowButtonsProperty);
        set => this.SetValue(ShowButtonsProperty, value);
    }

    /// <summary>
    /// Gets whether the label should be visible based on whether LabelText has content
    /// </summary>
    public bool IsLabelVisible => !string.IsNullOrEmpty(this.LabelText);

    /// <summary>
    /// Validates the provided value against all validators
    /// </summary>
    /// <param name="value">Value to validate</param>
    protected void ValidateValue(object value)
    {
        if (this.Validators == null || this.Validators.Count == 0)
        {
            this.SetValidationState(true, string.Empty);
            return;
        }

        foreach (var validator in this.Validators)
        {
            var result = validator.Validate(value);
            if (!result.IsValid)
            {
                this.SetValidationState(false, result.ErrorMessage);
                return;
            }
        }

        this.SetValidationState(true, string.Empty);
    }

    /// <summary>
    /// Sets the validation state
    /// </summary>
    private void SetValidationState(bool isValid, string errorMessage)
    {
        this.IsValid = isValid;
        this.ValidationError = errorMessage;
        this.OnPropertyChanged(nameof(IsValid));
        this.OnPropertyChanged(nameof(ValidationError));
    }

    /// <summary>
    /// Occurs when a property value changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event
    /// </summary>
    /// <param name="propertyName">Name of the property that changed</param>
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
        this.ValidateAndUpdateValue();
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = sender as System.Windows.Controls.TextBox;
        var newText = textBox?.Text.Insert(textBox.SelectionStart, e.Text) ?? string.Empty;
        e.Handled = !NumericRegex.IsMatch(newText);
    }

    private void OnIncrementClick(object sender, RoutedEventArgs e)
    {
        if (this.Value.HasValue)
        {
            this.Value = this.Value.Value + this.Step;
        }
        else
        {
            this.Value = this.Minimum ?? 0;
        }

        this.CoerceValue();
        this.ValidateValue(this.Value);
    }

    private void OnDecrementClick(object sender, RoutedEventArgs e)
    {
        if (this.Value.HasValue)
        {
            this.Value = this.Value.Value - this.Step;
        }
        else
        {
            this.Value = this.Maximum ?? 0;
        }

        this.CoerceValue();
        this.ValidateValue(this.Value);
    }

    private void ValidateAndUpdateValue()
    {
        if (double.TryParse(ValueTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            Value = result;
            CoerceValue();
        }
        else
        {
            ValueTextBox.Text = Value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }

        ValidateValue(Value ?? 0);
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