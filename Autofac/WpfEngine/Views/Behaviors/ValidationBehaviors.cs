using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WpfEngine.ViewModels.Base;

namespace WpfEngine.Views.Behaviors;

/// <summary>
/// Attached behaviors for validation on focus lost
/// </summary>
public static class ValidationBehaviors
{
    // ========== ValidateOnFocusLost Attached Property ==========
    
    public static readonly DependencyProperty ValidateOnFocusLostProperty =
        DependencyProperty.RegisterAttached(
            "ValidateOnFocusLost",
            typeof(bool),
            typeof(ValidationBehaviors),
            new PropertyMetadata(false, OnValidateOnFocusLostChanged));
    
    public static bool GetValidateOnFocusLost(DependencyObject obj)
    {
        return (bool)obj.GetValue(ValidateOnFocusLostProperty);
    }
    
    public static void SetValidateOnFocusLost(DependencyObject obj, bool value)
    {
        obj.SetValue(ValidateOnFocusLostProperty, value);
    }
    
    private static void OnValidateOnFocusLostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;
        
        if ((bool)e.NewValue)
        {
            element.LostFocus += OnElementLostFocus;
            element.Unloaded += OnElementUnloaded;
        }
        else
        {
            element.LostFocus -= OnElementLostFocus;
            element.Unloaded -= OnElementUnloaded;
        }
    }
    
    private static async void OnElementLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;
        
        // Get the binding property name
        var propertyName = GetBoundPropertyName(element);
        if (string.IsNullOrEmpty(propertyName))
            return;
        
        // Get the ViewModel
        if (element.DataContext is not ValidatableViewModel viewModel)
            return;
        
        // Trigger validation
        try
        {
            await viewModel.ForceValidatePropertyAsync(propertyName);
        }
        catch (Exception ex)
        {
            // Log error if logger is available
            System.Diagnostics.Debug.WriteLine($"Validation error: {ex.Message}");
        }
    }
    
    private static void OnElementUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.LostFocus -= OnElementLostFocus;
            element.Unloaded -= OnElementUnloaded;
        }
    }
    
    private static string? GetBoundPropertyName(FrameworkElement element)
    {
        // Try to get binding from common properties
        BindingExpression? binding = null;
        
        if (element is TextBox textBox)
        {
            binding = textBox.GetBindingExpression(TextBox.TextProperty);
        }
        else if (element is ComboBox comboBox)
        {
            binding = comboBox.GetBindingExpression(System.Windows.Controls.Primitives.Selector.SelectedItemProperty) 
                   ?? comboBox.GetBindingExpression(System.Windows.Controls.Primitives.Selector.SelectedValueProperty);
        }
        else if (element is CheckBox checkBox)
        {
            binding = checkBox.GetBindingExpression(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty);
        }
        else if (element is DatePicker datePicker)
        {
            binding = datePicker.GetBindingExpression(DatePicker.SelectedDateProperty);
        }
        else if (element is Slider slider)
        {
            binding = slider.GetBindingExpression(System.Windows.Controls.Primitives.RangeBase.ValueProperty);
        }
        
        return binding?.ParentBinding?.Path?.Path;
    }
    
    // ========== ValidateOnPropertyChange Attached Property ==========
    
    public static readonly DependencyProperty ValidateOnPropertyChangeProperty =
        DependencyProperty.RegisterAttached(
            "ValidateOnPropertyChange",
            typeof(bool),
            typeof(ValidationBehaviors),
            new PropertyMetadata(false, OnValidateOnPropertyChangeChanged));
    
    public static bool GetValidateOnPropertyChange(DependencyObject obj)
    {
        return (bool)obj.GetValue(ValidateOnPropertyChangeProperty);
    }
    
    public static void SetValidateOnPropertyChange(DependencyObject obj, bool value)
    {
        obj.SetValue(ValidateOnPropertyChangeProperty, value);
    }
    
    private static void OnValidateOnPropertyChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;
        
        if ((bool)e.NewValue)
        {
            // Enable validation on each keystroke/change
            UpdateBindingValidation(element, true);
        }
        else
        {
            UpdateBindingValidation(element, false);
        }
    }
    
    private static void UpdateBindingValidation(FrameworkElement element, bool validateOnPropertyChange)
    {
        BindingExpression? binding = null;
        DependencyProperty? property = null;
        
        // Get the appropriate binding
        if (element is TextBox textBox)
        {
            property = TextBox.TextProperty;
            binding = textBox.GetBindingExpression(property);
        }
        else if (element is ComboBox comboBox)
        {
            property = System.Windows.Controls.Primitives.Selector.SelectedItemProperty;
            binding = comboBox.GetBindingExpression(property);
        }
        // Add more control types as needed
        
        if (binding != null && property != null)
        {
            var newBinding = CloneBinding(binding.ParentBinding);
            if (validateOnPropertyChange)
            {
                newBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                newBinding.ValidatesOnDataErrors = true;
                newBinding.ValidatesOnExceptions = true;
                newBinding.NotifyOnValidationError = true;
            }
            else
            {
                newBinding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;
            }
            
            element.SetBinding(property, newBinding);
        }
    }
    
    private static Binding CloneBinding(Binding original)
    {
        var binding = new Binding
        {
            Path = original.Path,
            Mode = original.Mode,
            UpdateSourceTrigger = original.UpdateSourceTrigger,
            Converter = original.Converter,
            ConverterParameter = original.ConverterParameter,
            ConverterCulture = original.ConverterCulture,
            ValidatesOnDataErrors = original.ValidatesOnDataErrors,
            ValidatesOnExceptions = original.ValidatesOnExceptions,
            NotifyOnValidationError = original.NotifyOnValidationError
        };
        
        return binding;
    }
    
    // ========== ShowValidationTooltip Attached Property ==========
    
    public static readonly DependencyProperty ShowValidationTooltipProperty =
        DependencyProperty.RegisterAttached(
            "ShowValidationTooltip",
            typeof(bool),
            typeof(ValidationBehaviors),
            new PropertyMetadata(false, OnShowValidationTooltipChanged));
    
    public static bool GetShowValidationTooltip(DependencyObject obj)
    {
        return (bool)obj.GetValue(ShowValidationTooltipProperty);
    }
    
    public static void SetShowValidationTooltip(DependencyObject obj, bool value)
    {
        obj.SetValue(ShowValidationTooltipProperty, value);
    }
    
    private static void OnShowValidationTooltipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;
        
        if ((bool)e.NewValue)
        {
            Validation.AddErrorHandler(element, OnValidationError);
        }
        else
        {
            Validation.RemoveErrorHandler(element, OnValidationError);
        }
    }
    
    private static void OnValidationError(object sender, ValidationErrorEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;
        
        if (e.Action == ValidationErrorEventAction.Added)
        {
            element.ToolTip = e.Error.ErrorContent?.ToString();
        }
        else
        {
            element.ToolTip = null;
        }
    }
}