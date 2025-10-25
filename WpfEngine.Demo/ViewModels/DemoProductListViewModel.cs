using System.Collections.ObjectModel;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;

namespace WpfEngine.Demo.ViewModels;

// ========== DEMO PRODUCT LIST ==========

public partial class DemoProductListViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllHandler;
    private readonly ICommandHandler<DeleteDemoProductCommand> _deleteHandler;
    private readonly IWindowService _windowService;

    // Track open detail windows by product ID
    private readonly Dictionary<int, Guid> _openDetailWindows = new();
    private bool _disposed;
    
    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();
    
    [ObservableProperty]
    private DemoProduct? _selectedProduct;
    
    public DemoProductListViewModel(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllHandler,
        ICommandHandler<DeleteDemoProductCommand> deleteHandler,
        IWindowService windowService,
        ILogger<DemoProductListViewModel> logger) : base(logger)
    {
        _getAllHandler = getAllHandler;
        _deleteHandler = deleteHandler;
        _windowService = windowService;

        // Subscribe to window events for real-time updates
        _windowService.WindowClosed += OnWindowClosed;

        Logger.LogInformation("[DEMO] ProductListViewModel created with event subscription");
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
            SetError($"Failed to load products: {ex.Message}");
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

        // Check if detail window already open for this product
        if (_openDetailWindows.ContainsKey(productId))
        {
            Logger.LogInformation("[DEMO] Detail window already open for product {ProductId}", productId);
            return;
        }

        Logger.LogInformation("[DEMO] Opening non-modal product detail for {ProductId}", productId);

        // Generate unique window params
        var detailParams = new DemoProductDetailParams { ProductId = productId };
        var windowId = detailParams.CorrelationId;

        _openDetailWindows[productId] = windowId;

        // Open as regular window
        _windowService.OpenWindow<DemoProductDetailViewModel, DemoProductDetailParams>(detailParams);
    }
    
    private bool CanViewDetail() => SelectedProduct != null && !IsBusy;
    
    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        var productId = SelectedProduct.Id;

        // Check if detail window is open
        if (_openDetailWindows.ContainsKey(productId))
        {
            Logger.LogWarning("[DEMO] Cannot delete product {ProductId} - detail window is open", productId);
            SetError("Cannot delete product while detail window is open. Please close it first.");
            return;
        }
        
        try
        {
            IsBusy = true;
            ClearError();
            
            await _deleteHandler.HandleAsync(new DeleteDemoProductCommand(productId));
            await LoadProductsAsync();

            Logger.LogInformation("[DEMO] Deleted product {ProductId}", productId);
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete product: {ex.Message}");
            Logger.LogError(ex, "[DEMO] Error deleting product {ProductId}", productId);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool CanDelete() => SelectedProduct != null && !IsBusy;
    
    partial void OnSelectedProductChanged(DemoProduct? value)
    {
        ViewDetailCommand.NotifyCanExecuteChanged();
        DeleteProductCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Handles window closed event from WindowService
    /// Refreshes list when detail window closes (after potential updates)
    /// </summary>
    private async void OnWindowClosed(object? sender, WindowClosedEventArgs e)
    {
        // Check if it's a ProductDetail window
        if (e.ViewModelType == typeof(DemoProductDetailViewModel))
        {
            Logger.LogInformation("[DEMO] ProductDetail window {WindowId} closed, refreshing list", e.WindowId);

            // Remove from tracking
            var productId = _openDetailWindows.FirstOrDefault(kvp => kvp.Value == e.WindowId).Key;
            if (productId != 0)
            {
                _openDetailWindows.Remove(productId);
            }

            // Refresh list to show any updates made in detail window
            await LoadProductsAsync();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[DEMO] ProductListViewModel disposing - closing {Count} detail windows",
            _openDetailWindows.Count);

        // Unsubscribe from events
        _windowService.WindowClosed -= OnWindowClosed;

        // Close all open detail windows
        foreach (var windowId in _openDetailWindows.Values.ToList())
        {
            _windowService.Close(windowId);
        }

        _openDetailWindows.Clear();

        _disposed = true;
    }
}
