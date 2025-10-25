using System;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Product detail selector - opened from ProductSelectorViewModel
/// 
/// DEMONSTRATES HIERARCHY:
/// WorkflowHostWindow (session scope)
///   └─ ProductSelectorWindow (window scope, parent: session)
///        └─ ProductDetailSelectorWindow (window scope, parent: session)
///             └─ ALL see SAME IOrderBuilderService instance!
/// 
/// User can select product directly from detail view
/// </summary>
public partial class ProductDetailSelectorViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> _getProductHandler;
    private readonly IOrderBuilderService _orderBuilder; // ← SAME shared instance!
    private readonly IWindowService _windowService;
    private readonly int _productId;
    
    [ObservableProperty]
    private DemoProduct? _product;
    
    [ObservableProperty]
    private string _categoryName = "N/A";
    
    [ObservableProperty]
    private bool _isInStock;
    
    [ObservableProperty]
    private string _stockStatus = string.Empty;
    
    [ObservableProperty]
    private int _quantityToAdd = 1;
    
    private bool _disposed;

    public ProductDetailSelectorViewModel(
        IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> getProductHandler,
        IOrderBuilderService orderBuilder,  // ← INJECTED from session scope!
        IWindowService windowService,
        ILogger<ProductDetailSelectorViewModel> logger,
        ProductDetailSelectorParams parameters) : base(logger)
    {
        _getProductHandler = getProductHandler;
        _orderBuilder = orderBuilder;
        _windowService = windowService;
        _productId = parameters.ProductId;
        
        Logger.LogInformation("[PRODUCT_DETAIL_SELECTOR] Created for product {ProductId} - sees shared OrderBuilder",
            _productId);
    }
    
    public string CurrentOrderInfo => 
        $"Order: {_orderBuilder.OrderItems.Count} items, {_orderBuilder.Total:C}";

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            
            var product = await _getProductHandler.HandleAsync(new GetDemoProductByIdQuery(_productId));
            
            if (product == null)
            {
                Logger.LogWarning("[PRODUCT_DETAIL_SELECTOR] Product {ProductId} not found", _productId);
                return;
            }
            
            Product = product;
            CategoryName = product.Category?.Name ?? "Uncategorized";
            IsInStock = product.Stock > 0;
            
            StockStatus = product.Stock switch
            {
                0 => "Out of Stock",
                < 10 => $"Low Stock ({product.Stock} available)",
                _ => $"In Stock ({product.Stock} available)"
            };
            
            Logger.LogInformation("[PRODUCT_DETAIL_SELECTOR] Loaded product {ProductName}", product.Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PRODUCT_DETAIL_SELECTOR] Error loading product");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddToOrder))]
    private void AddToOrder()
    {
        if (Product == null) return;
        
        // Add to SHARED service - workflow host and selector window will BOTH see this change!
        _orderBuilder.AddItem(Product.Id, Product.Name, Product.Price, QuantityToAdd);
        
        Logger.LogInformation("[PRODUCT_DETAIL_SELECTOR] Added {Quantity}x {Product} to SHARED order",
            QuantityToAdd, Product.Name);
        
        // Update display
        OnPropertyChanged(nameof(CurrentOrderInfo));
        
        // Close this detail window after adding
        CloseWindow();
    }

    private bool CanAddToOrder() => Product != null && IsInStock && QuantityToAdd > 0;

    [RelayCommand]
    private void CloseWindow()
    {
        Logger.LogInformation("[PRODUCT_DETAIL_SELECTOR] Closing window");
        _windowService.Close(this.GetVmKey());
    }

    partial void OnQuantityToAddChanged(int value)
    {
        AddToOrderCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        Logger.LogInformation("[PRODUCT_DETAIL_SELECTOR] Disposing");
        _disposed = true;
    }
}

public record ProductDetailSelectorParams : ViewModelOptions
{
    public int ProductId { get; init; }
}

