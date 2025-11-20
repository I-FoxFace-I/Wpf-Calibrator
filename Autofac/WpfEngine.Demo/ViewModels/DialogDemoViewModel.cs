//using System;
//using System.Collections.ObjectModel;
//using System.Threading.Tasks;
//using System.Windows;
//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using Microsoft.Extensions.Logging;
//using WpfEngine.Core.Data;
//using WpfEngine.Services;
//using WpfEngine.ViewModels;
//using WpfEngine.Demo.Models;
//using WpfEngine.Demo.ViewModels.Dialogs;

//namespace WpfEngine.Demo.ViewModels;

///// <summary>
///// Demo ViewModel showing various dialog scenarios
///// </summary>
//public partial class DialogDemoViewModel : BaseViewModel
//{
//    private readonly IDialogService _dialogService;
//    private readonly IWindowContext _windowContext;
//    private readonly ILogger<DialogDemoViewModel> _logger;
    
//    [ObservableProperty]
//    private string _lastDialogResult = "No dialog shown yet";
    
//    [ObservableProperty]
//    private Customer? _selectedCustomer;
    
//    [ObservableProperty]
//    private Product? _selectedProduct;
    
//    public ObservableCollection<Customer> Customers { get; }
//    public ObservableCollection<Product> Products { get; }
//    public ObservableCollection<string> DialogHistory { get; }
    
//    public DialogDemoViewModel(
//        IDialogService dialogService,
//        IWindowContext windowContext,
//        ILogger<DialogDemoViewModel> logger) : base(logger)
//    {
//        _dialogService = dialogService;
//        _windowContext = windowContext;
//        _logger = logger;
        
//        DisplayName = "Dialog Demonstrations";
        
//        Customers = new ObservableCollection<Customer>();
//        Products = new ObservableCollection<Product>();
//        DialogHistory = new ObservableCollection<string>();
        
//        LoadSampleData();
//    }
    
//    // ========== SIMPLE DIALOGS ==========
    
//    [RelayCommand]
//    private async Task ShowMessageDialogAsync()
//    {
//        _logger.LogInformation("Showing message dialog");
        
//        var result = await _dialogService.ShowMessageAsync(
//            "This is a simple message dialog.\n\n" +
//            "It can display information to the user with various button options.",
//            "Information",
//            MessageBoxButton.OK,
//            MessageBoxImage.Information);
        
//        UpdateResult($"Message dialog result: {result}");
//    }
    
//    [RelayCommand]
//    private async Task ShowConfirmationDialogAsync()
//    {
//        _logger.LogInformation("Showing confirmation dialog");
        
//        var result = await _dialogService.ShowConfirmationAsync(
//            "Are you sure you want to perform this action?\n\n" +
//            "This demonstrates a Yes/No confirmation dialog.",
//            "Confirm Action");
        
//        UpdateResult($"Confirmation result: {(result ? "Confirmed" : "Cancelled")}");
//    }
    
//    [RelayCommand]
//    private async Task ShowErrorDialogAsync()
//    {
//        _logger.LogInformation("Showing error dialog");
        
//        try
//        {
//            // Simulate an error
//            throw new InvalidOperationException("This is a simulated error for demonstration purposes.");
//        }
//        catch (Exception ex)
//        {
//            await _dialogService.ShowErrorAsync(
//                "An error occurred during the operation.",
//                "Operation Failed",
//                ex);
            
//            UpdateResult("Error dialog shown");
//        }
//    }
    
//    // ========== CUSTOM DIALOGS ==========
    
//    [RelayCommand]
//    private async Task ShowNewCustomerDialogAsync()
//    {
//        _logger.LogInformation("Showing new customer dialog");
        
//        var result = await _dialogService.ShowDialogAsync<CustomerEditDialogViewModel, CustomerEditResult>();
        
//        if (result.IsSuccess && result.Result?.Customer != null)
//        {
//            var customer = result.Result.Customer;
//            Customers.Add(customer);
//            SelectedCustomer = customer;
            
//            UpdateResult($"New customer created: {customer.Name} ({customer.Email})");
            
//            await _dialogService.ShowMessageAsync(
//                $"Customer '{customer.Name}' has been successfully created.",
//                "Customer Created",
//                MessageBoxButton.OK,
//                MessageBoxImage.Information);
//        }
//        else
//        {
//            UpdateResult("New customer dialog cancelled");
//        }
//    }
    
//    [RelayCommand]
//    private async Task EditCustomerAsync()
//    {
//        if (SelectedCustomer == null)
//        {
//            await _dialogService.ShowMessageAsync(
//                "Please select a customer to edit.",
//                "No Selection",
//                MessageBoxButton.OK,
//                MessageBoxImage.Warning);
//            return;
//        }
        
//        _logger.LogInformation("Editing customer: {CustomerName}", SelectedCustomer.Name);
        
//        // Initialize dialog with existing customer
//        var parameters = new CustomerEditParameters { Customer = SelectedCustomer };
        
//        var result = await _dialogService.ShowDialogAsync<CustomerEditDialogViewModel, CustomerEditParameters, CustomerEditResult>(parameters);
        
//        if (result.IsSuccess && result.Result?.Customer != null)
//        {
//            var updatedCustomer = result.Result.Customer;
            
//            // Update customer in list
//            var index = Customers.IndexOf(SelectedCustomer);
//            if (index >= 0)
//            {
//                Customers[index] = updatedCustomer;
//                SelectedCustomer = updatedCustomer;
//            }
            
//            UpdateResult($"Customer updated: {updatedCustomer.Name}");
//        }
//        else
//        {
//            UpdateResult("Edit customer cancelled");
//        }
//    }
    
//    [RelayCommand]
//    private async Task SelectProductAsync()
//    {
//        _logger.LogInformation("Opening product selection dialog");

//        var result = await _dialogService.ShowDialogAsync<ProductSelectionDialogViewModel, ProductSelectionResult>();
        
//        if (result.IsSuccess && result.Result?.SelectedProduct != null)
//        {
//            var product = result.Result.SelectedProduct;
//            var quantity = result.Result.Quantity;
            
//            // Add to products if not already there
//            if (!Products.Contains(product))
//            {
//                Products.Add(product);
//            }
//            SelectedProduct = product;
            
//            UpdateResult($"Product selected: {quantity}x {product.Name} @ {product.Price:C}");
            
//            // Show confirmation with nested dialog
//            var totalPrice = product.Price * quantity;
//            var confirm = await _dialogService.ShowConfirmationAsync(
//                $"Add to cart:\n" +
//                $"Product: {product.Name}\n" +
//                $"Quantity: {quantity}\n" +
//                $"Unit Price: {product.Price:C}\n" +
//                $"Total: {totalPrice:C}\n\n" +
//                $"Proceed with adding to cart?",
//                "Confirm Add to Cart");
            
//            if (confirm)
//            {
//                UpdateResult($"Added to cart: {quantity}x {product.Name} = {totalPrice:C}");
//            }
//        }
//        else
//        {
//            UpdateResult("Product selection cancelled");
//        }
//    }
    
//    // ========== NESTED DIALOGS DEMO ==========
    
//    [RelayCommand]
//    private async Task ShowNestedDialogsAsync()
//    {
//        _logger.LogInformation("Demonstrating nested dialogs");
        
//        // Level 1: Confirmation
//        var proceed = await _dialogService.ShowConfirmationAsync(
//            "This will demonstrate nested dialogs.\n" +
//            "You'll see multiple dialogs that can open on top of each other.\n\n" +
//            "Continue?",
//            "Nested Dialogs Demo");
        
//        if (!proceed)
//        {
//            UpdateResult("Nested dialogs demo cancelled");
//            return;
//        }
        
//        // Level 2: Product Selection
//        var productResult = await _dialogService.ShowDialogAsync<ProductSelectionDialogViewModel, ProductSelectionResult>();
        
//        if (!productResult.IsSuccess || productResult.Result?.SelectedProduct == null)
//        {
//            UpdateResult("Nested dialogs: Product selection cancelled");
//            return;
//        }
        
//        var product = productResult.Result.SelectedProduct;
        
//        // Level 3: Customer Selection
//        var customerResult = await _dialogService.ShowDialogAsync<CustomerEditDialogViewModel, CustomerEditResult>();
        
//        if (!customerResult.IsSuccess || customerResult.Result?.Customer == null)
//        {
//            UpdateResult("Nested dialogs: Customer selection cancelled");
//            return;
//        }
        
//        var customer = customerResult.Result.Customer;
        
//        // Level 4: Final Confirmation
//        var message = $"Order Summary:\n\n" +
//                     $"Customer: {customer.Name}\n" +
//                     $"Product: {product.Name}\n" +
//                     $"Quantity: {productResult.Result.Quantity}\n" +
//                     $"Total: {(product.Price * productResult.Result.Quantity):C}\n\n" +
//                     $"Create this order?";
        
//        var createOrder = await _dialogService.ShowConfirmationAsync(message, "Create Order");
        
//        if (createOrder)
//        {
//            UpdateResult($"Order created: {customer.Name} ordered {productResult.Result.Quantity}x {product.Name}");
            
//            // Final success message
//            await _dialogService.ShowMessageAsync(
//                "Order has been successfully created!",
//                "Success",
//                MessageBoxButton.OK,
//                MessageBoxImage.Information);
//        }
//        else
//        {
//            UpdateResult("Order creation cancelled");
//        }
//    }
    
//    // ========== CHILD WINDOWS ==========
    
//    [RelayCommand]
//    private void OpenChildWindow()
//    {
//        _logger.LogInformation("Opening child window");
        
//        var result = _windowContext.TryOpenWindow<ProductDetailSelectorViewModel>();
        
//        if (result.IsSuccess)
//        {
//            UpdateResult($"Child window opened: {result.Value}");
//        }
//        else
//        {
//            UpdateResult($"Failed to open child window: {result.ErrorMessage}");
//        }
//    }
    
//    // ========== HELPERS ==========
    
//    private void UpdateResult(string message)
//    {
//        LastDialogResult = message;
//        DialogHistory.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        
//        // Keep only last 10 items
//        while (DialogHistory.Count > 10)
//        {
//            DialogHistory.RemoveAt(DialogHistory.Count - 1);
//        }
        
//        _logger.LogDebug("Dialog result: {Message}", message);
//    }
    
//    private void LoadSampleData()
//    {
//        // Sample customers
//        Customers.Add(new Customer 
//        { 
//            Id = Guid.NewGuid(),
//            Name = "Acme Corporation",
//            Email = "contact@acme.com",
//            Company = "Acme Corp",
//            CustomerType = CustomerType.Premium
//        });
        
//        Customers.Add(new Customer 
//        { 
//            Id = Guid.NewGuid(),
//            Name = "Tech Startup Inc",
//            Email = "info@techstartup.com",
//            Company = "Tech Startup",
//            CustomerType = CustomerType.Regular
//        });
        
//        // Sample products
//        Products.Add(new Product
//        {
//            Id = Guid.NewGuid(),
//            SKU = "DEMO-001",
//            Name = "Demo Product",
//            Price = 99.99m,
//            Stock = 10,
//            Category = ProductCategory.Electronics
//        });
//    }
//}

//// ========== PARAMETERS ==========

//public class CustomerEditParameters : IViewModelParameters
//{
//    public Customer? Customer { get; init; }

//    public Guid CorrelationId => throw new NotImplementedException();
//}