namespace Calibrator.WpfControl.Controls.ScTextBox;

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Calibrator.WpfControl.Controls.Base;
using Calibrator.WpfControl.Validation;

/// <summary>
/// A text input component with label and validation support
/// </summary>
public partial class ScTextBoxComponent : UserControl
{
    /// <summary>
    /// Initializes a new instance of the ScTextBoxComponent class
    /// </summary>
    public ScTextBoxComponent()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Identifies the Placeholder dependency property
    /// </summary>
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(object),
            typeof(ScTextBoxComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the LabelText dependency property
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(nameof(LabelText), typeof(string),
            typeof(ScTextBoxComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the TextBoxWidth dependency property
    /// </summary>
    public static readonly DependencyProperty TextBoxWidthProperty =
        DependencyProperty.Register(nameof(TextBoxWidth), typeof(double),
            typeof(ScTextBoxComponent), new PropertyMetadata(200.0));

    /// <summary>
    /// Identifies the TextBoxHeight dependency property
    /// </summary>
    public static readonly DependencyProperty TextBoxHeightProperty =
        DependencyProperty.Register(nameof(TextBoxHeight), typeof(double),
            typeof(ScTextBoxComponent), new PropertyMetadata(50.0));

    //public static readonly DependencyProperty TextBoxTextProperty =
    //    DependencyProperty.Register(nameof(TextBoxText), typeof(object),
    //        typeof(ScTextBoxComponent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Identifies the TextBoxText dependency property
    /// </summary>
    public static readonly DependencyProperty TextBoxTextProperty =
        DependencyProperty.Register(nameof(TextBoxText), typeof(object),
            typeof(ScTextBoxComponent),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    /// <summary>
    /// Identifies the Validators dependency property
    /// </summary>
    public static readonly DependencyProperty ValidatorsProperty =
        DependencyProperty.Register(nameof(Validators), typeof(ICollection<IValidator<object>>),
            typeof(ScTextBoxComponent), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the ValidationError dependency property
    /// </summary>
    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(nameof(ValidationError), typeof(string),
            typeof(ScTextBoxComponent), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the IsValid dependency property
    /// </summary>
    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(ScTextBoxComponent), new PropertyMetadata(true));

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
    /// Gets or sets the placeholder text displayed when the textbox is empty
    /// </summary>
    public object Placeholder
    {
        get => this.GetValue(PlaceholderProperty);
        set => this.SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets the label text displayed above the textbox
    /// </summary>
    public string LabelText
    {
        get => (string)this.GetValue(LabelTextProperty);
        set => this.SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the textbox control
    /// </summary>
    public double TextBoxWidth
    {
        get => (double)this.GetValue(TextBoxWidthProperty);
        set => this.SetValue(TextBoxWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the textbox control
    /// </summary>
    public double TextBoxHeight
    {
        get => (double)this.GetValue(TextBoxHeightProperty);
        set => this.SetValue(TextBoxHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the text content of the textbox
    /// </summary>
    public object TextBoxText
    {
        get => this.GetValue(TextBoxTextProperty);
        set => this.SetValue(TextBoxTextProperty, value);
    }

    /// <summary>
    /// Gets whether the label should be visible based on whether LabelText has content
    /// </summary>
    public bool IsLabelVisible => !string.IsNullOrEmpty(this.LabelText);

    /// <summary>
    /// Validates the provided value against all validators.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    protected void ValidateValue(object value)
    {
        if (Validators == null || Validators.Count == 0)
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
        IsValid = isValid;
        ValidationError = errorMessage;
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(ValidationError));
    }

    /// <summary>
    /// Occurs when a property value changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    //private void OnLostFocus(object sender, RoutedEventArgs e)
    //{
    //    ValidateValue(TextBoxText);
    //}


    /// <summary>
    /// Identifies the EnableRealtimeValidation dependency property
    /// </summary>
    public static readonly DependencyProperty EnableRealtimeValidationProperty =
        DependencyProperty.Register(nameof(EnableRealtimeValidation), typeof(bool),
            typeof(ScTextBoxComponent), new PropertyMetadata(true));


    /// <summary>
    /// Enable real-time validation while typing (default: true)
    /// </summary>
    public bool EnableRealtimeValidation
    {
        get => (bool)this.GetValue(EnableRealtimeValidationProperty);
        set => this.SetValue(EnableRealtimeValidationProperty, value);
    }


    /// <summary>
    /// Handles text changes for real-time validation.
    /// </summary>
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScTextBoxComponent textBox && textBox.EnableRealtimeValidation)
        {
            textBox.ValidateValue(e.NewValue);
        }
    }

    /// <summary>
    /// Handles lost focus event to validate when real-time validation is disabled.
    /// </summary>
    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (!this.EnableRealtimeValidation)
        {
            this.ValidateValue(this.TextBoxText);
        }
    }

}