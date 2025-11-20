using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using WpfEngine.Abstract;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

public partial class ProductListViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllHandler;
    private readonly ICommandHandler<DeleteDemoProductCommand> _deleteHandler;
    private readonly ICommandHandler<CreateDemoProductCommand> _createHandler;
    private readonly IWindowContext _windowContext;
    private readonly Dictionary<int, Guid> _openDetailWindows = new();
    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();

    [ObservableProperty]
    private DemoProduct? _selectedProduct;

    public ProductListViewModel(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllHandler,
        ICommandHandler<DeleteDemoProductCommand> deleteHandler,
        ICommandHandler<CreateDemoProductCommand> createHandler,
        IWindowContext windowContext,
        ILogger<ProductListViewModel> logger) : base(logger)
    {
        _getAllHandler = getAllHandler;
        _deleteHandler = deleteHandler;
        _windowContext = windowContext;
        _createHandler = createHandler;
        _windowContext.ChildClosed += OnChildWindowClosed;
        Logger.LogInformation("[DEMO] ProductListViewModel created");
    }

    public async Task InitializeAsync(CancellationToken cancelationToken = default)
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
            var products = await _getAllHandler.HandleAsync(new GetAllDemoProductsQuery());
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            Logger.LogInformation("[DEMO] Loaded {Count} products", Products.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to load products");
            SetError("Failed to load products: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateProductAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            var dialogResult = await _windowContext.ShowDialogAsync<ProductCreateViewModel, ProductCreateParameters, DemoProduct>(
                new ProductCreateParameters { ProductId = Products.Select(x => x.Id).Max() + 1 } );

            if(dialogResult.IsSuccess && dialogResult.Result is DemoProduct result)
            {
                await _createHandler.HandleAsync(new CreateDemoProductCommand(
                      result.Name, result.Description, result.Barcode, result.Price, 0, result.Weight, result.Unit, result.Category?.Id));
                await LoadProductsAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to create product");
            SetError("Failed to create product: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanViewDetail))]
    private void ViewDetail()
    {
        if (SelectedProduct == null) return;
        var productId = SelectedProduct.Id;

        if (_openDetailWindows.ContainsKey(productId))
        {
            Logger.LogInformation("[DEMO] Product {ProductId} detail already open", productId);
            return;
        }

        Logger.LogInformation("[DEMO] Opening detail for product {ProductId}", productId);
        var windowId = _windowContext.OpenWindow<ProductDetailViewModel, ProductDetailParameters>(
            new ProductDetailParameters { ProductId = productId }
        );
        _openDetailWindows[productId] = windowId;
    }

    private bool CanViewDetail() => SelectedProduct != null;

    [RelayCommand(CanExecute = nameof(CanDeleteProduct))]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;
        try
        {
            IsBusy = true;
            ClearError();
            var productId = SelectedProduct.Id;
            Logger.LogInformation("[DEMO] Deleting product {ProductId}", productId);
            await _deleteHandler.HandleAsync(new DeleteDemoProductCommand (productId));
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to delete product");
            SetError("Failed to delete product: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDeleteProduct() => SelectedProduct != null;

    [RelayCommand]
    private void CloseWindow()
    {
        Logger.LogInformation("[DEMO] Closing product list");
        _windowContext.CloseWindow();
    }

    partial void OnSelectedProductChanged(DemoProduct? value)
    {
        ViewDetailCommand.NotifyCanExecuteChanged();
        DeleteProductCommand.NotifyCanExecuteChanged();
    }

    private async void OnChildWindowClosed(object? sender, ChildWindowClosedEventArgs e)
    {
        if (e.ViewModelType == typeof(ProductDetailViewModel))
        {
            Logger.LogInformation("[DEMO] Product detail closed, refreshing list");
            var productId = _openDetailWindows.FirstOrDefault(kvp => kvp.Value == e.ChildWindowId).Key;
            if (productId != 0)
            {
                _openDetailWindows.Remove(productId);
            }
            await LoadProductsAsync();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _windowContext.ChildClosed -= OnChildWindowClosed;
        _windowContext.CloseAllChildWindows();
        _openDetailWindows.Clear();
        _disposed = true;
    }
}
