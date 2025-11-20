using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using WpfEngine.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Sessions;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Data;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Repositories;
using WpfEngine.Demo.Services;
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Demo.ViewModels.Workflow.Repository;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;
using WpfEngine.ViewModels.Managed;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.ViewModels.Product.Repository;

/// <summary>
/// Product List ViewModel using Repository + Unit of Work pattern with Fluent API
/// </summary>
public partial class ProductListViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly Dictionary<int, Guid> _openDetailWindows = new();
    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();

    [ObservableProperty]
    private DemoProduct? _selectedProduct;

    public ProductListViewModel(
        IScopeManager scopeManager,
        IWindowContext windowContext,
        ILogger<ProductListViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        _windowContext.ChildClosed += OnChildWindowClosed;
        Logger.LogInformation("[DEMO_V2] ProductListViewModel created");
    }

    public override async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync(CancellationToken cancelationToken = default)
    {
        try
        {
            IsBusy = true;
            ClearError();

            // Use Fluent API with database session
            var products = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .ExecuteWithResultAsync(async (repo) =>  await repo.GetAllAsync(cancelationToken));

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            Logger.LogInformation("[DEMO_V2] Loaded {Count} products", Products.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to load products");
            SetError("Failed to load products: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateProductAsync(CancellationToken cancellationToken=default)
    {
        try
        {
            IsBusy = true;
            ClearError();

            var maxId = Products.Any() ? Products.Select(x => x.Id).Max() : 0;
            var dialogResult = await _windowContext.ShowDialogAsync<ProductCreateViewModel, ProductCreateParameters, DemoProduct>(
                new ProductCreateParameters { ProductId = maxId + 1 });

            if (dialogResult.IsSuccess && dialogResult.Result is DemoProduct newProduct)
            {
                // Use Fluent API to create product
                await _scopeManager
                    .CreateDatabaseSession()
                    .WithService<IRepository<DemoProduct>>()
                    .ExecuteAsync(async (repo) =>
                    {
                        await repo.AddAsync(newProduct, cancellationToken);
                        Logger.LogInformation("[DEMO_V2] Product {ProductId} created", newProduct.Id);
                    });

                await LoadProductsAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to create product");
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
            Logger.LogInformation("[DEMO_V2] Product {ProductId} detail already open", productId);
            return;
        }

        Logger.LogInformation("[DEMO_V2] Opening detail for product {ProductId}", productId);
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
            Logger.LogInformation("[DEMO_V2] Deleting product {ProductId}", productId);

            // Use Fluent API to delete product
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .ExecuteAsync(async (repo) =>
                {
                    var product = await repo.GetByIdAsync(productId);
                    if (product != null)
                    {
                        await repo.DeleteAsync(product);
                        Logger.LogInformation("[DEMO_V2] Product {ProductId} deleted", productId);
                    }
                });

            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to delete product");
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
        Logger.LogInformation("[DEMO_V2] Closing product list");
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
            Logger.LogInformation("[DEMO_V2] Product detail closed, refreshing list");
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