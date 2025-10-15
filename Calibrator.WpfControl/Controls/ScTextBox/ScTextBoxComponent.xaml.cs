using Calibrator.WpfControl.Controls.Base;
using Calibrator.WpfControl.Validation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Calibrator.WpfControl.Controls.ScTextBox;

public partial class ScTextBoxComponent : UserControl
{
    public ScTextBoxComponent()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(object),
            typeof(ScTextBoxComponent), new PropertyMetadata(string.Empty));
    
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScTextBoxComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TextBoxWidthProperty =
        DependencyProperty.Register(nameof(TextBoxWidth), typeof(double),
            typeof(ScTextBoxComponent), new PropertyMetadata(200.0));

    public static readonly DependencyProperty TextBoxHeightProperty =
        DependencyProperty.Register(nameof(TextBoxHeight), typeof(double),
            typeof(ScTextBoxComponent), new PropertyMetadata(50.0));

    //public static readonly DependencyProperty TextBoxTextProperty =
    //    DependencyProperty.Register(nameof(TextBoxText), typeof(object),
    //        typeof(ScTextBoxComponent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty TextBoxTextProperty =
        DependencyProperty.Register(nameof(TextBoxText), typeof(object),
            typeof(ScTextBoxComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty ValidatorsProperty =
        DependencyProperty.Register(nameof(Validators), typeof(List<IValidator<object>>),
            typeof(ScTextBoxComponent), new PropertyMetadata(null));

    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(nameof(ValidationError), typeof(string),
            typeof(ScTextBoxComponent), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(ScTextBoxComponent), new PropertyMetadata(true));

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

    public object Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public double TextBoxWidth
    {
        get => (double)GetValue(TextBoxWidthProperty);
        set => SetValue(TextBoxWidthProperty, value);
    }

    public double TextBoxHeight
    {
        get => (double)GetValue(TextBoxHeightProperty);
        set => SetValue(TextBoxHeightProperty, value);
    }
    
    public object TextBoxText
    {
        get => GetValue(TextBoxTextProperty);
        set => SetValue(TextBoxTextProperty, value);
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


    //private void OnLostFocus(object sender, RoutedEventArgs e)
    //{
    //    ValidateValue(TextBoxText);
    //}


    // NEW: Enable/disable real-time validation
    public static readonly DependencyProperty EnableRealtimeValidationProperty =
        DependencyProperty.Register(nameof(EnableRealtimeValidation), typeof(bool),
            typeof(ScTextBoxComponent), new PropertyMetadata(true));


    /// <summary>
    /// Enable real-time validation while typing (default: true)
    /// </summary>
    public bool EnableRealtimeValidation
    {
        get => (bool)GetValue(EnableRealtimeValidationProperty);
        set => SetValue(EnableRealtimeValidationProperty, value);
    }


    // Real-time validation on text change
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScTextBoxComponent textBox && textBox.EnableRealtimeValidation)
        {
            textBox.ValidateValue(e.NewValue);
        }
    }

    // Also validate on lost focus (for when real-time is disabled)
    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (!EnableRealtimeValidation)
        {
            ValidateValue(TextBoxText);
        }
    }

}
