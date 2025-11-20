using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System;
using WpfEngine.Data.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Parameters;
using WpfEngine.Demo.Models;
using WpfEngine.Enums;
using WpfEngine.Services;
using WpfEngine.ViewModels.Base;
using WpfEngine.ViewModels.Dialogs;

namespace WpfEngine.Demo.ViewModels.Customer.Repository;

/// <summary>
/// Dialog for editing customer information
/// Returns CustomerEditResult with validated customer data
/// </summary>
public partial class CustomerEditDialogViewModel : ValidatableViewModel
{
    private readonly ILogger<CustomerEditDialogViewModel> _logger;
    [ObservableProperty]
    private string _email = "";
    [ObservableProperty]
    private string? _phone = "";
    [ObservableProperty]
    private string _company = "";
    
    [ObservableProperty]
    private CustomerType _customerType = CustomerType.Regular;
    
    [ObservableProperty]
    private decimal _creditLimit = 10000;
    
    [ObservableProperty]
    private bool _isActive = true;
    
    [ObservableProperty]
    private string? _notes;
    
    // For edit mode
    private Customer? _originalCustomer;
    private bool _isEditMode;
    
    public CustomerEditDialogViewModel(ILogger<CustomerEditDialogViewModel> logger) : base(logger)
    {
        _logger = logger;
        DisplayName = "Customer Information";
    }
    
    public bool IsEditMode => _isEditMode;

    public override async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing new customer dialog");
        _isEditMode = false;
        DisplayName = "New Customer";
        
        // Set defaults
        Name = "";
        Email = "";
        Company = "";
        CustomerType = CustomerType.Regular;
        CreditLimit = 10000;
        IsActive = true;
        
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Initialize for editing existing customer
    /// </summary>
    public Task InitializeAsync(Customer customer)
    {
        _logger.LogInformation("Initializing edit customer dialog for {CustomerName}", customer.Name);
        _isEditMode = true;
        _originalCustomer = customer;
        DisplayName = $"Edit Customer - {customer.Name}";
        
        // Load customer data
        Name = customer.Name;
        Email = customer.Email;
        Phone = customer.Phone;
        Company = customer.Company;
        CustomerType = customer.CustomerType;
        CreditLimit = customer.CreditLimit;
        IsActive = customer.IsActive;
        Notes = customer.Notes;
        
        return Task.CompletedTask;
    }
    
    // Dialog result
    public CustomerEditResult? DialogResult { get; private set; }
    
    // ========== COMMANDS ==========
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        _logger.LogInformation("Attempting to save customer");
        
        // Validate
        if (!await ValidateAsync())
        {
            _logger.LogWarning("Validation failed");
            SetError("Please correct the validation errors before saving.");
            return;
        }
        
        // Check for duplicate email (async validation example)
        if (!_isEditMode || _originalCustomer?.Email != Email)
        {
            if (await CheckEmailExistsAsync(Email))
            {
                AddPropertyError(nameof(Email), "This email is already registered");
                SetError("Email already exists in the system.");
                return;
            }
        }
        
        // Create result
        var customer = new Customer
        {
            Id = _originalCustomer?.Id ?? Guid.NewGuid(),
            Name = Name,
            Email = Email,
            Phone = Phone,
            Company = Company,
            CustomerType = CustomerType,
            CreditLimit = CreditLimit,
            IsActive = IsActive,
            Notes = Notes,
            ModifiedAt = DateTime.Now
        };
        
        if (!_isEditMode)
        {
            customer.CreatedAt = DateTime.Now;
        }
        else
        {
            customer.CreatedAt = _originalCustomer!.CreatedAt;
        }
        
        DialogResult = new CustomerEditResult
        {
            IsSuccess = true,
            Customer = customer,
            IsNewCustomer = !_isEditMode
        };
        
        _logger.LogInformation("Customer saved successfully: {CustomerId}", customer.Id);
        
        // Close dialog - this would be handled by dialog service
        OnRequestClose?.Invoke(this, true);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("Customer edit cancelled");
        
        DialogResult = new CustomerEditResult
        {
            IsSuccess = false,
            ErrorMessage = "Operation cancelled by user"
        };
        
        OnRequestClose?.Invoke(this, false);
    }
    
    [RelayCommand]
    private void ResetToDefaults()
    {
        if (_isEditMode && _originalCustomer != null)
        {
            InitializeAsync(_originalCustomer);
        }
        else
        {
            InitializeAsync();
        }
    }
    
    // ========== VALIDATION ==========
    
    protected override async Task<IEnumerable<string>> ValidatePropertyCustomAsync(string propertyName, object? value)
    {
        var errors = new List<string>();
        
        switch (propertyName)
        {
            case nameof(CreditLimit):
                if (CreditLimit < 0)
                {
                    errors.Add("Credit limit cannot be negative");
                }
                if (CustomerType == CustomerType.Premium && CreditLimit < 5000)
                {
                    errors.Add("Premium customers must have at least $5,000 credit limit");
                }
                break;
                
            case nameof(Email):
                // Could check for uniqueness here
                break;
        }
        
        return errors;
    }
    
    protected override Task<IEnumerable<string>> ValidateObjectCustomAsync()
    {
        var errors = new List<string>();
        
        // Business rules
        if (!IsActive && string.IsNullOrWhiteSpace(Notes))
        {
            errors.Add("Inactive customers must have notes explaining the reason");
        }
        
        if (CustomerType == CustomerType.VIP && CreditLimit < 50000)
        {
            errors.Add("VIP customers should have at least $50,000 credit limit");
        }
        
        return Task.FromResult<IEnumerable<string>>(errors);
    }
    
    private async Task<bool> CheckEmailExistsAsync(string email)
    {
        // Simulate async database check
        await Task.Delay(100);
        
        // For demo, some emails are "taken"
        var existingEmails = new[] { "admin@company.com", "test@example.com" };
        return existingEmails.Contains(email, StringComparer.OrdinalIgnoreCase);
    }
    
    // Event for requesting dialog close
    public event EventHandler<bool>? OnRequestClose;
}

// ========== RESULT & MODELS ==========

public class CustomerEditResult : IDialogResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Customer? Customer { get; init; }
    public bool IsNewCustomer { get; init; }

    public Guid Key => throw new NotImplementedException();

    public DialogStatus Status => throw new NotImplementedException();

    public bool IsComplete => throw new NotImplementedException();

    public bool IsCancelled => throw new NotImplementedException();
}

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Company { get; set; } = "";
    public CustomerType CustomerType { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}