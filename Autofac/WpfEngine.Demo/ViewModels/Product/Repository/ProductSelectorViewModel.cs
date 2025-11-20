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
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;
using WpfEngine.ViewModels.Managed;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.ViewModels.Product.Repository;

/// <summary>
/// Product Selector ViewModel using Repository pattern with Fluent API
/// Used in workflow for product selection
/// </summary>
public partial class ProductSelectorViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IScopeManager _scopeManager;
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
        IScopeManager scopeManager,
        IOrderBuilderService orderBuilder,
        IWindowContext windowContext,
        ILogger<ProductSelectorViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _orderBuilder = orderBuilder;
        _windowContext = windowContext;
        Logger.LogInformation("[PRODUCT_SELECTOR] ViewModel created");
    }

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            
            // Use Fluent API to load products
            var products = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.GetAllAsync();
                });
            
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            
            Logger.LogInformation("[PRODUCT_SELECTOR] Loaded {Count} products", Products.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[PRODUCT_SELECTOR] Error loading products");
            SetError("Failed to load products: " + ex.Message);
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