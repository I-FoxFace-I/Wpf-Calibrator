using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Products;
using AutofacEnhancedWpfDemo.Models;
using AutofacEnhancedWpfDemo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Products management window
/// </summary>
public partial class ProductsViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetAllProductsQuery, List<Product>> _getAllProductsHandler;
    private readonly ICommandHandler<DeleteProductCommand> _deleteProductHandler;
    private readonly IWindowNavigator _navigator;
    
    [ObservableProperty]
    private ObservableCollection<Product> _products = new();
    
    [ObservableProperty]
    private Product? _selectedProduct;
    
    public ProductsViewModel(
        IQueryHandler<GetAllProductsQuery, List<Product>> getAllProductsHandler,
        ICommandHandler<DeleteProductCommand> deleteProductHandler,
        IWindowNavigator navigator,
        ILogger<ProductsViewModel> logger) : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _deleteProductHandler = deleteProductHandler;
        _navigator = navigator;
    }
    
    /// <summary>
    /// Loads products on initialization
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }
    
    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            
            Logger.LogInformation("Loading products");
            var products = await _getAllProductsHandler.HandleAsync(new GetAllProductsQuery());
            
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            
            Logger.LogInformation("Loaded {Count} products", Products.Count);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load products: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task EditProductAsync()
    {
        if (SelectedProduct == null) return;
        
        Logger.LogInformation("Opening edit dialog for product {ProductId}", SelectedProduct.Id);
        
        var result = await _navigator.ShowDialogAsync<EditProductViewModel, EditProductResult>(
            new EditProductParams { ProductId = SelectedProduct.Id }
        );
        
        if (result?.Success == true)
        {
            Logger.LogInformation("Product edited successfully, refreshing list");
            await LoadProductsAsync();
        }
    }
    
    private bool CanEdit() => SelectedProduct != null && !IsBusy;
    
    [RelayCommand]
    private async Task AddProductAsync()
    {
        Logger.LogInformation("Opening add product dialog");
        
        var result = await _navigator.ShowDialogAsync<EditProductViewModel, EditProductResult>();
        
        if (result?.Success == true)
        {
            Logger.LogInformation("Product added successfully, refreshing list");
            await LoadProductsAsync();
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;
        
        var productName = SelectedProduct.Name;
        Logger.LogInformation("Deleting product {ProductId}", SelectedProduct.Id);
        
        try
        {
            IsBusy = true;
            await _deleteProductHandler.HandleAsync(new DeleteProductCommand(SelectedProduct.Id));
            
            Logger.LogInformation("Product {Name} deleted successfully", productName);
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete product: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool CanDelete() => SelectedProduct != null && !IsBusy;
    
    partial void OnSelectedProductChanged(Product? value)
    {
        // Refresh command can-execute states
        EditProductCommand.NotifyCanExecuteChanged();
        DeleteProductCommand.NotifyCanExecuteChanged();
    }
}

// ========== DTOs ==========

/// <summary>
/// Parameters for editing a product
/// </summary>
public record EditProductParams
{
    public int? ProductId { get; init; }
}

/// <summary>
/// Result from product edit dialog
/// </summary>
public record EditProductResult
{
    public bool Success { get; init; }
    public int? ProductId { get; init; }
}