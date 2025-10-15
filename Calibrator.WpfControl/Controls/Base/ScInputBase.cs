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
    public static readonly DependencyProperty ValidatorsProperty =
        DependencyProperty.Register(nameof(Validators), typeof(List<IValidator<object>>),
            typeof(ScInputBase), new PropertyMetadata(null));
    
    public static readonly DependencyProperty ValidationErrorProperty =
        DependencyProperty.Register(nameof(ValidationError), typeof(string),
            typeof(ScInputBase), new PropertyMetadata(string.Empty));
    
    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool),
            typeof(ScInputBase), new PropertyMetadata(true));
    
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
}
