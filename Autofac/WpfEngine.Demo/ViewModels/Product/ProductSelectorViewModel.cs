using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Abstract;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;


public partial class ProductSelectorViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllProductsHandler;
    private readonly IOrderBuilderService _orderBuilder;
    private readonly IWindowContext _windowContext;
    
    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();
    
    [ObservableProperty]
    private DemoProduct? _selectedProduct;
    
    [ObservableProperty]
    private int _quantity = 1;
    
    private bool _disposed;

    public string CurrentOrderSummary => 
        $"Order: {_orderBuilder.OrderItems.Count} items, Total: ${_orderBuilder.Total:F2}";

    public ProductSelectorViewModel(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllProductsHandler,
        IOrderBuilderService orderBuilder,
        IWindowContext windowContext,
        ILogger<ProductSelectorViewModel> logger) : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _orderBuilder = orderBuilder;
        _windowContext = windowContext;
        Logger.LogInformation("[PRODUCT_SELECTOR] ViewModel created");
    }

    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        try
        {
            IsBusy = true;
            var products = await _getAllProductsHandler.HandleAsync(new GetAllDemoProductsQuery());
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddToOrder))]
    private void AddToOrder()
    {
        if (SelectedProduct == null) return;
        _orderBuilder.AddItem(SelectedProduct.Id, SelectedProduct.Name, SelectedProduct.Price, Quantity);
        Logger.LogInformation("[PRODUCT_SELECTOR] Added {Quantity}x {Product}", Quantity, SelectedProduct.Name);
        OnPropertyChanged(nameof(CurrentOrderSummary));
    }

    private bool CanAddToOrder() => SelectedProduct != null && Quantity > 0;

    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedProduct == null) return;
        _windowContext.OpenWindow<ProductInfoViewModel, ProductDetailParameters>(
            new ProductDetailParameters { ProductId = SelectedProduct.Id }
        );
    }

    [RelayCommand]
    private void CloseWindow()
    {
        _windowContext.CloseWindow();
    }

    partial void OnSelectedProductChanged(DemoProduct? value)
    {
        AddToOrderCommand.NotifyCanExecuteChanged();
    }

    partial void OnQuantityChanged(int value)
    {
        AddToOrderCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
