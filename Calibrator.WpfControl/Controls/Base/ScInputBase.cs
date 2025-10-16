using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Calibrator.WpfControl.Validation;

namespace Calibrator.WpfControl.Controls.Base;

/// <summary>
/// Base class for all input components with validation support
/// </summary>
public abstract class ScInputBase : UserControl, INotifyPropertyChanged
{
    /// <summary>
    /// Identifies the Validators dependency property
    /// </summary>
    public static readonly DependencyProperty ValidatorsProperty =
        DependencyProperty.Register(nameof(Validators), typeof(ICollection<IValidator<object>>),
            typeof(ScInputBase), new PropertyMetadata(null));

    /// <summary>
    /// Identifies the ValidationError dependency property
    /// </summary>
    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(nameof(ValidationError), typeof(string),
            typeof(ScInputBase), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the IsValid dependency property
    /// </summary>
    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(ScInputBase), new PropertyMetadata(true));

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
    /// Validates the provided value against all validators
    /// </summary>
    /// <param name="value">Value to validate</param>
    protected void ValidateValue(object value)
    {
        if (Validators == null || Validators.Count == 0)
        {
            this.SetValidationState(true, string.Empty);
            return;
        }

        foreach (var validator in Validators)
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
    /// <param name="isValid">Whether the value is valid</param>
    /// <param name="errorMessage">The error message if validation failed</param>
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
    /// Raises the PropertyChanged event
    /// </summary>
    /// <param name="propertyName">Name of the property that changed</param>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}