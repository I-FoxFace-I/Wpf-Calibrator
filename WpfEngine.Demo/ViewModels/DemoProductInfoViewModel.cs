using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Product info ViewModel - readonly display of product details
/// </summary>
public partial class DemoProductInfoViewModel : BaseViewModel, IInitializable
{
    private readonly IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> _getProductHandler;
    private readonly int _productId;
    
    [ObservableProperty]
    private DemoProduct? _product;
    
    [ObservableProperty]
    private string _categoryName = "N/A";
    
    [ObservableProperty]
    private bool _isInStock;
    
    [ObservableProperty]
    private string _stockStatus = string.Empty;
    
    public DemoProductInfoViewModel(
        IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> getProductHandler,
        ILogger<DemoProductInfoViewModel> logger,
        DemoProductInfoParams parameters) : base(logger)
    {
        _getProductHandler = getProductHandler;
        _productId = parameters.ProductId;
        
        Logger.LogInformation("DemoProductInfoViewModel created for product {ProductId}", _productId);
    }
    
    public override async Task InitializeAsync()
    {
        await LoadProductAsync();
    }
    
    private async Task LoadProductAsync()
    {
        try
        {
            IsBusy = true;
            
            var product = await _getProductHandler.HandleAsync(new GetDemoProductByIdQuery(_productId));
            
            if (product == null)
            {
                Logger.LogWarning("Product {ProductId} not found", _productId);
                return;
            }
            
            Product = product;
            CategoryName = product.Category?.Name ?? "Uncategorized";
            IsInStock = product.Stock > 0;
            
            if (product.Stock == 0)
            {
                StockStatus = "Out of Stock";
            }
            else if (product.Stock < 10)
            {
                StockStatus = $"Low Stock ({product.Stock} available)";
            }
            else
            {
                StockStatus = $"In Stock ({product.Stock} available)";
            }
            
            Logger.LogInformation("Loaded product {ProductName}", product.Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading product");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public record DemoProductInfoParams : ViewModelOptions
{
    public int ProductId { get; init; }
}
