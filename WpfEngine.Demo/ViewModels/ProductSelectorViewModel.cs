using System;
using System.Collections.ObjectModel;
using System.Linq;
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
/// Product selector window - opened IN WORKFLOW SESSION
/// 
/// KEY FEATURE:
/// - Shares IOrderBuilderService with workflow host
/// - Can add products directly to shared order
/// - Can open product detail which ALSO sees same service
/// 
/// DEMONSTRATES:
/// Multiple windows in session hierarchy all seeing same shared service instance!
/// </summary>
public partial class ProductSelectorViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllProductsHandler;
    private readonly IWorkflowSession _workflowSession; // From session scope
    private readonly IOrderBuilderService _orderBuilder; // SHARED from session scope!
    private readonly IWindowService _windowService;
    
    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();
    
    [ObservableProperty]
    private DemoProduct? _selectedProduct;
    
    [ObservableProperty]
    private int _quantity = 1;
    
    private bool _disposed;

    public ProductSelectorViewModel(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllProductsHandler,
        IWorkflowSession workflowSession,  // ← Injected from session!
        IOrderBuilderService orderBuilder,  // ← SAME instance as in workflow steps!
        IWindowService windowService,
        ILogger<ProductSelectorViewModel> logger) : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _workflowSession = workflowSession;
        _orderBuilder = orderBuilder;
        _windowService = windowService;
        
        // Subscribe to order changes from OTHER windows
        _orderBuilder.OrderItemsChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(CurrentOrderSummary));
        };
        
        Logger.LogInformation("[PRODUCT_SELECTOR] Created in session {SessionId} - sees shared OrderBuilder",
            workflowSession.SessionId);
    }

    public string CurrentOrderSummary => 
        $"Current Order: {_orderBuilder.OrderItems.Count} items, Total: {_orderBuilder.Total:C}";

    public override async Task InitializeAsync()
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
            
            Logger.LogInformation("[PRODUCT_SELECTOR] Loaded {Count} products", Products.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ViewProductDetail(DemoProduct? product)
    {
        if (product == null) return;
        
        Logger.LogInformation("[PRODUCT_SELECTOR] Opening product detail for {ProductId}", product.Id);
        
        // Opens CHILD window IN SESSION - also sees shared IOrderBuilderService!
        _workflowSession.OpenChildWindow<ProductDetailSelectorViewModel, ProductDetailSelectorParams>(
            this.Id,
            new ProductDetailSelectorParams { ProductId = product.Id }
        );
    }

    [RelayCommand]
    private void AddToOrder(DemoProduct? product)
    {
        if (product == null || Quantity <= 0) return;
        
        // Adds to SHARED service - all windows will see the change!
        _orderBuilder.AddItem(product.Id, product.Name, product.Price, Quantity);
        
        Logger.LogInformation("[PRODUCT_SELECTOR] Added {Quantity}x {Product} to shared order",
            Quantity, product.Name);
    }

    [RelayCommand]
    private void CloseWindow()
    {
        Logger.LogInformation("[PRODUCT_SELECTOR] Closing window");
        _windowService.Close(this.GetVmKey());
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        Logger.LogInformation("[PRODUCT_SELECTOR] Disposing");
        
        // Unsubscribe from events
        if (_orderBuilder != null)
        {
            _orderBuilder.OrderItemsChanged -= (s, e) => { };
        }
        
        _disposed = true;
    }
}

