using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using WpfEngine.Abstract;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

public partial class ProductDetailViewModel : BaseViewModel, IInitializable
{
    private readonly IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> _getProductHandler;
    private readonly ICommandHandler<UpdateDemoProductCommand> _updateHandler;
    private readonly IWindowContext _windowContext;
    private readonly int _productId;

    [ObservableProperty] private DemoProduct? _product;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private string _barcode = string.Empty;
    [ObservableProperty] private int _stock;
    [ObservableProperty] private decimal _weight;
    [ObservableProperty] private string _unit = "pcs";
    [ObservableProperty] private ObservableCollection<DemoProductCategory> _categories = new();
    [ObservableProperty] private DemoProductCategory? _selectedCategory;

    public ProductDetailViewModel(
        ProductDetailParameters parameters,
        IQueryHandler<GetDemoProductByIdQuery, DemoProduct?> getProductHandler,
        ICommandHandler<UpdateDemoProductCommand> updateHandler,
        IWindowContext windowContext,
        ILogger<ProductDetailViewModel> logger) : base(logger)
    {
        _getProductHandler = getProductHandler;
        _updateHandler = updateHandler;
        _windowContext = windowContext;
        _productId = parameters.ProductId;
        Logger.LogInformation("[DEMO] ProductDetailViewModel created for {ProductId}", _productId);
    }

    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        try
        {
            IsBusy = true;
            ClearError();
            var product = await _getProductHandler.HandleAsync(new GetDemoProductByIdQuery(_productId));
            
            if (product == null)
            {
                SetError($"Product {_productId} not found");
                return;
            }

            Product = product;
            Name = product.Name;
            Description = product.Description;
            Price = product.Price;
            Stock = product.Stock;
            Barcode = product.Barcode;
            Logger.LogInformation("[DEMO] Loaded product {ProductId}", _productId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to load product");
            SetError("Failed to load product: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (Product == null) return;
        try
        {
            IsBusy = true;
            ClearError();
            
            await _updateHandler.HandleAsync(new UpdateDemoProductCommand(
                _productId, Name, Description, Barcode, Price, Stock, Weight, Unit, SelectedCategory?.Id
            ));
            
            Logger.LogInformation("[DEMO] Product {ProductId} updated", _productId);
            _windowContext.CloseWindow();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to save product");
            SetError("Failed to save: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && Price >= 0;

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO] Cancelling product edit");
        _windowContext.CloseWindow();
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnPriceChanged(decimal value) => SaveCommand.NotifyCanExecuteChanged();
}
