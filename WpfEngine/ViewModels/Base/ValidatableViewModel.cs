using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.ViewModels.Base;

/// <summary>
/// Base ViewModel with async validation support
/// Implements INotifyDataErrorInfo for WPF binding
/// </summary>
public abstract partial class ValidatableViewModel : BaseViewModel, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();
    private readonly Dictionary<string, ValidationState> _validationStates = new();
    
    protected ValidatableViewModel(ILogger<ValidatableViewModel> logger) : base(logger)
    {
    }
    
    // ========== INotifyDataErrorInfo Implementation ==========
    
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    
    public bool HasErrors => _errors.Any();
    
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.SelectMany(e => e.Value);
        }
        
        return _errors.TryGetValue(propertyName, out var errors) 
            ? errors 
            : Enumerable.Empty<string>();
    }
    
    // ========== Validation Methods ==========
    
    /// <summary>
    /// Validates a property asynchronously
    /// Called automatically on property change if property is marked for validation
    /// </summary>
    protected virtual async Task ValidatePropertyAsync([CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName)) return;
        
        // Skip if already validating this property
        if (_validationStates.TryGetValue(propertyName, out var state) && state == ValidationState.Validating)
            return;
        
        _validationStates[propertyName] = ValidationState.Validating;
        
        try
        {
            Logger.LogDebug("[VALIDATION] Validating property {Property}", propertyName);
            
            // Clear existing errors
            ClearPropertyErrors(propertyName);
            
            // Get property value
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo == null) return;
            
            var value = propertyInfo.GetValue(this);
            
            // 1. Run Data Annotation validations
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this) { MemberName = propertyName };
            
            if (!Validator.TryValidateProperty(value, validationContext, validationResults))
            {
                foreach (var result in validationResults)
                {
                    AddPropertyError(propertyName, result.ErrorMessage ?? "Validation failed");
                }
            }
            
            // 2. Run custom async validations
            var customErrors = await ValidatePropertyCustomAsync(propertyName, value);
            foreach (var error in customErrors)
            {
                AddPropertyError(propertyName, error);
            }
            
            _validationStates[propertyName] = ValidationState.Validated;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[VALIDATION] Error validating property {Property}", propertyName);
            AddPropertyError(propertyName, $"Validation error: {ex.Message}");
            _validationStates[propertyName] = ValidationState.Error;
        }
    }
    
    /// <summary>
    /// Override to provide custom async validation logic for specific properties
    /// </summary>
    protected virtual Task<IEnumerable<string>> ValidatePropertyCustomAsync(string propertyName, object? value)
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }
    
    /// <summary>
    /// Validates entire object
    /// </summary>
    public virtual async Task<bool> ValidateAsync()
    {
        Logger.LogInformation("[VALIDATION] Validating entire object");
        
        // Clear all errors
        ClearAllErrors();
        
        // Validate all properties
        var properties = GetType().GetProperties()
            .Where(p => p.CanRead && p.GetCustomAttributes(typeof(ValidationAttribute), true).Any());
        
        foreach (var property in properties)
        {
            await ValidatePropertyAsync(property.Name);
        }
        
        // Run object-level validation
        var objectErrors = await ValidateObjectCustomAsync();
        foreach (var error in objectErrors)
        {
            AddPropertyError(string.Empty, error);
        }
        
        return !HasErrors;
    }
    
    /// <summary>
    /// Override to provide object-level validation
    /// </summary>
    protected virtual Task<IEnumerable<string>> ValidateObjectCustomAsync()
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }
    
    /// <summary>
    /// Forces validation of a specific property
    /// </summary>
    public Task ForceValidatePropertyAsync(string propertyName)
    {
        return ValidatePropertyAsync(propertyName);
    }
    
    // ========== Error Management ==========
    
    protected void AddPropertyError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
        {
            _errors[propertyName] = new List<string>();
        }
        
        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }
    
    protected void ClearPropertyErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }
    
    protected void ClearAllErrors()
    {
        var properties = _errors.Keys.ToList();
        _errors.Clear();
        
        foreach (var property in properties)
        {
            OnErrorsChanged(property);
        }
    }
    
    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
        
        // Update error message for UI
        if (HasErrors)
        {
            var allErrors = _errors.SelectMany(e => e.Value);
            SetError(string.Join(Environment.NewLine, allErrors));
        }
        else
        {
            ClearError();
        }
    }
    
    /// <summary>
    /// Gets all validation errors as a list
    /// </summary>
    public IEnumerable<string> GetValidationErrors()
    {
        return _errors.SelectMany(e => e.Value);
    }
    
    /// <summary>
    /// Gets validation errors for a specific property
    /// </summary>
    public IEnumerable<string> GetPropertyErrors(string propertyName)
    {
        return _errors.TryGetValue(propertyName, out var errors) 
            ? errors 
            : Enumerable.Empty<string>();
    }
    
    // ========== Property Change with Auto-Validation ==========
    
    /// <summary>
    /// Sets property value and triggers validation if needed
    /// </summary>
    protected bool SetPropertyWithValidation<T>(
        ref T field, 
        T value, 
        bool validateOnChange = true,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        
        field = value;
        OnPropertyChanged(propertyName);
        
        if (validateOnChange && !string.IsNullOrEmpty(propertyName))
        {
            // Fire and forget validation
            _ = ValidatePropertyAsync(propertyName);
        }
        
        return true;
    }
    
    private enum ValidationState
    {
        NotValidated,
        Validating,
        Validated,
        Error
    }
}

/// <summary>
/// Validation attributes for async validation
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AsyncValidationAttribute : Attribute
{
    public bool ValidateOnChange { get; set; } = false;
    public bool ValidateOnFocusLost { get; set; } = true;
}

/// <summary>
/// Custom validation attribute for async validation methods
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CustomValidationAttribute : ValidationAttribute
{
    public string ValidationMethodName { get; }
    
    public CustomValidationAttribute(string validationMethodName)
    {
        ValidationMethodName = validationMethodName;
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // This will be handled by ValidatePropertyCustomAsync
        return ValidationResult.Success;
    }
}