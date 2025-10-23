using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Products;
using AutofacEnhancedWpfDemo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Child Demo window
/// Demonstrates shared scope - changes visible across all sibling windows
/// </summary>
public partial class ChildDemoViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetAllProductsQuery, List<Product>> _getAllProductsHandler;
    private readonly ICommandHandler<UpdateProductPriceCommand> _updatePriceHandler;

    [ObservableProperty]
    private string _windowTitle = "Child Window";

    [ObservableProperty]
    private string _windowColor = "#3B82F6";

    [ObservableProperty]
    private string _dbContextInfo = "DbContext: Shared with parent";

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    public ChildDemoViewModel(
        IQueryHandler<GetAllProductsQuery, List<Product>> getAllProductsHandler,
        ICommandHandler<UpdateProductPriceCommand> updatePriceHandler,
        ILogger<ChildDemoViewModel> logger,
        ChildDemoOptions? options = null) : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _updatePriceHandler = updatePriceHandler;

        if (options != null)
        {
            WindowTitle = $"Child Window {options.ChildNumber}";
            WindowColor = options.Color ?? "#3B82F6";
        }

        Logger.LogInformation("ChildDemoViewModel created: {Title}", WindowTitle);
    }

    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            Logger.LogInformation("Loading products in child window");
            var products = await _getAllProductsHandler.HandleAsync(new GetAllProductsQuery());

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load products");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyProduct))]
    private async Task ModifyProductAsync()
    {
        if (SelectedProduct == null) return;

        try
        {
            Logger.LogInformation("Modifying product {Product}", SelectedProduct.Name);

            var newPrice = SelectedProduct.Price * 1.05m; // Increase by 5%

            await _updatePriceHandler.HandleAsync(
                new UpdateProductPriceCommand(SelectedProduct.Id, newPrice)
            );

            Logger.LogInformation("Product price updated to {Price}", newPrice);

            // Refresh to show changes
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to modify product");
        }
    }

    private bool CanModifyProduct() => SelectedProduct != null;

    partial void OnSelectedProductChanged(Product? value)
    {
        ModifyProductCommand.NotifyCanExecuteChanged();
    }
}
