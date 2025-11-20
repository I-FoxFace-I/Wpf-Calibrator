using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.ViewModels;
using WpfEngine.Data.Abstract;
using WpfEngine.Enums;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels.Dialogs;

/// <summary>
/// Dialog for selecting products with filtering and search
/// Shows nested dialog capabilities
/// </summary>
public partial class ProductSelectionDialogViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<ProductSelectionDialogViewModel> _logger;
    
    [ObservableProperty]
    private string _searchText = "";
    
    [ObservableProperty]
    private ProductCategory? _selectedCategory;
    
    [ObservableProperty]
    private Product? _selectedProduct;
    
    [ObservableProperty]
    private bool _showOnlyAvailable = true;
    
    [ObservableProperty]
    private int _quantity = 1;
    
    public ObservableCollection<Product> Products { get; }
    public ICollectionView ProductsView { get; }
    
    public ObservableCollection<ProductCategory> Categories { get; }
    
    public ProductSelectionDialogViewModel(
        IDialogService dialogService,
        ILogger<ProductSelectionDialogViewModel> logger) : base(logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        
        DisplayName = "Select Product";
        
        Products = new ObservableCollection<Product>();
        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProduct;
        
        Categories = new ObservableCollection<ProductCategory>
        {
            ProductCategory.Electronics,
            ProductCategory.Clothing,
            ProductCategory.Food,
            ProductCategory.Books,
            ProductCategory.Other
        };
    }

    public override async Task InitializeAsync() => await InitializeAsync(CancellationToken.None);
    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        //await ExecuteAsync(async () =>
        //{
            _logger.LogInformation("Loading products for selection");
            
            // Simulate loading products
            await Task.Delay(500);
            
            var products = GenerateSampleProducts();
            
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            
            _logger.LogInformation("Loaded {Count} products", Products.Count);
        //});
    }
    
    // ========== FILTERING ==========
    
    partial void OnSearchTextChanged(string value)
    {
        ProductsView.Refresh();
    }
    
    partial void OnSelectedCategoryChanged(ProductCategory? value)
    {
        ProductsView.Refresh();
    }
    
    partial void OnShowOnlyAvailableChanged(bool value)
    {
        ProductsView.Refresh();
    }
    
    private bool FilterProduct(object obj)
    {
        if (obj is not Product product)
            return false;
        
        // Category filter
        if (SelectedCategory.HasValue && product.Category != SelectedCategory.Value)
            return false;
        
        // Availability filter
        if (ShowOnlyAvailable && product.Stock <= 0)
            return false;
        
        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            if (!product.Name.ToLower().Contains(search) &&
                !product.Description.ToLower().Contains(search) &&
                !product.SKU.ToLower().Contains(search))
            {
                return false;
            }
        }
        
        return true;
    }
    
    // ========== COMMANDS ==========
    
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = "";
        SelectedCategory = null;
        ShowOnlyAvailable = false;
    }
    
    [RelayCommand]
    private async Task ShowProductDetailsAsync()
    {
        if (SelectedProduct == null)
            return;
        
        _logger.LogInformation("Showing details for product {ProductName}", SelectedProduct.Name);
        
        // Show nested dialog with product details
        var message = $"Product: {SelectedProduct.Name}\n" +
                     $"SKU: {SelectedProduct.SKU}\n" +
                     $"Category: {SelectedProduct.Category}\n" +
                     $"Price: {SelectedProduct.Price:C}\n" +
                     $"Stock: {SelectedProduct.Stock} units\n\n" +
                     $"Description:\n{SelectedProduct.Description}";
        
        await _dialogService.ShowMessageAsync(
            message, 
            "Product Details", 
            MessageBoxButton.OK, 
            MessageBoxImage.Information);
    }
    
    [RelayCommand]
    private async Task AddCustomProductAsync()
    {
        _logger.LogInformation("Opening add custom product dialog");
    }
    
    [RelayCommand(CanExecute = nameof(CanSelect))]
    private void Select()
    {
        if (SelectedProduct == null)
            return;
        
        _logger.LogInformation("Product selected: {ProductName}, Quantity: {Quantity}", 
            SelectedProduct.Name, Quantity);
        
        DialogResult = new ProductSelectionResult
        {
            IsSuccess = true,
            SelectedProduct = SelectedProduct,
            Quantity = Quantity
        };
        
        OnRequestClose?.Invoke(this, true);
    }
    
    private bool CanSelect() => SelectedProduct != null && Quantity > 0;
    
    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("Product selection cancelled");
        
        DialogResult = new ProductSelectionResult
        {
            IsSuccess = false,
            ErrorMessage = "Selection cancelled"
        };
        
        OnRequestClose?.Invoke(this, false);
    }
    
    [RelayCommand]
    private void SelectProductFromList(Product product)
    {
        SelectedProduct = product;
    }
    
    [RelayCommand]
    private async Task QuickAddToOrderAsync(Product product)
    {
        if (product == null)
            return;
        
        var confirm = await _dialogService.ShowConfirmationAsync(
            $"Add 1x '{product.Name}' to order?",
            "Quick Add");
        
        if (confirm)
        {
            SelectedProduct = product;
            Quantity = 1;
            Select();
        }
    }
    
    // ========== PROPERTIES & RESULT ==========
    
    public ProductSelectionResult? DialogResult { get; private set; }
    
    public event EventHandler<bool>? OnRequestClose;
    
    // ========== SAMPLE DATA ==========
    
    private Product[] GenerateSampleProducts()
    {
        return new[]
        {
            new Product 
            { 
                Id = Guid.NewGuid(),
                SKU = "ELEC-001",
                Name = "Laptop Pro 15",
                Description = "High-performance laptop with 16GB RAM",
                Category = ProductCategory.Electronics,
                Price = 1299.99m,
                Stock = 15,
                IsActive = true
            },
            new Product 
            { 
                Id = Guid.NewGuid(),
                SKU = "ELEC-002",
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with precision tracking",
                Category = ProductCategory.Electronics,
                Price = 29.99m,
                Stock = 50,
                IsActive = true
            },
            new Product 
            { 
                Id = Guid.NewGuid(),
                SKU = "BOOK-001",
                Name = "C# Programming Guide",
                Description = "Comprehensive guide to C# programming",
                Category = ProductCategory.Books,
                Price = 49.99m,
                Stock = 10,
                IsActive = true
            },
            new Product 
            { 
                Id = Guid.NewGuid(),
                SKU = "CLTH-001",
                Name = "Developer T-Shirt",
                Description = "Comfortable cotton t-shirt with code humor",
                Category = ProductCategory.Clothing,
                Price = 19.99m,
                Stock = 0, // Out of stock
                IsActive = true
            },
            new Product 
            { 
                Id = Guid.NewGuid(),
                SKU = "FOOD-001",
                Name = "Energy Drink Pack",
                Description = "Pack of 12 energy drinks for long coding sessions",
                Category = ProductCategory.Food,
                Price = 24.99m,
                Stock = 30,
                IsActive = true
            },
            new Product 
            { 
                Id = Guid.NewGuid(),
                SKU = "ELEC-003",
                Name = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard with blue switches",
                Category = ProductCategory.Electronics,
                Price = 89.99m,
                Stock = 8,
                IsActive = true
            }
        };
    }
}

// ========== RESULT ==========

public class ProductSelectionResult : IDialogResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Product? SelectedProduct { get; init; }
    public int Quantity { get; init; }

    public Guid Key => throw new NotImplementedException();

    public DialogStatus Status => throw new NotImplementedException();

    public bool IsComplete => throw new NotImplementedException();

    public bool IsCancelled => throw new NotImplementedException();
}

public class Product
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ProductCategory
{
    Electronics,
    Clothing,
    Food,
    Books,
    Toys,
    Other
}