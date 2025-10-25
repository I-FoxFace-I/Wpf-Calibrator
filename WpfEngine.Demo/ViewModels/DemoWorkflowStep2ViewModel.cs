using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Services;
using WpfEngine.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Step 2: Add products to order
/// Uses shared IOrderBuilderService from session scope
/// </summary>
public partial class DemoWorkflowStep2ViewModel : BaseStepViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllProductsHandler;
    private readonly INavigationService _navigator;
    private readonly IOrderBuilderService _orderBuilder; // Shared session service!
    private readonly WorkflowState _state;
    
    // Property injection for optional IWorkflowSession
    public IWorkflowSession? Session { get; set; }

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();

    [ObservableProperty]
    private ObservableCollection<WorkflowOrderItem> _orderItems = new();

    [ObservableProperty]
    private int _quantity = 1;

    public decimal OrderTotal => OrderItems.Sum(i => i.Total);

    private bool _disposed;

    public DemoWorkflowStep2ViewModel(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllProductsHandler,
        INavigationService navigator,
        IOrderBuilderService orderBuilder,  // Injected from session scope!
        ILogger<DemoWorkflowStep2ViewModel> logger,
        WorkflowState state) : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _navigator = navigator;
        _orderBuilder = orderBuilder;
        _state = state;
        CustomerName = state.CustomerName;

        Logger.LogInformation("[WORKFLOW] Step2 ViewModel created with shared OrderBuilder service");
    }

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

            // Load order items from shared service
            OrderItems.Clear();
            foreach (var item in _orderBuilder.OrderItems)
            {
                OrderItems.Add(item);
            }

            Logger.LogInformation("[WORKFLOW] Step2 loaded {Count} products, {OrderCount} order items from shared service", 
                Products.Count, OrderItems.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ViewProductInfo(DemoProduct? product)
    {
        if (product == null) return;

        Logger.LogInformation("[WORKFLOW] Opening product info for {ProductId}", product.Id);

        // Open product info window in session (if session available)
        if (Session != null)
        {
            Session.OpenWindow<DemoProductInfoViewModel, DemoProductInfoParams>(
                new DemoProductInfoParams { ProductId = product.Id });
                
            Logger.LogInformation("[WORKFLOW] Opened ProductInfo in session {SessionId}", Session.SessionId);
        }
        else
        {
            Logger.LogWarning("[WORKFLOW] Cannot open product info - no session available (not injected)");
        }
    }

    [RelayCommand]
    private void AddItemWithProduct(DemoProduct? product)
    {
        if (product == null || Quantity <= 0) return;

        // Add to shared order builder service
        _orderBuilder.AddItem(product.Id, product.Name, product.Price, Quantity);

        Logger.LogInformation("[WORKFLOW] Added {Quantity}x {Product} to shared order", Quantity, product.Name);

        // Reload items from shared service
        OrderItems.Clear();
        foreach (var item in _orderBuilder.OrderItems)
        {
            OrderItems.Add(item);
        }

        OnOrderTotalChanged();
    }

    [RelayCommand]
    private void RemoveItem(WorkflowOrderItem? item)
    {
        if (item == null) return;

        _orderBuilder.RemoveItem(item);
        
        Logger.LogInformation("[WORKFLOW] Removed {Product} from shared order", item.ProductName);
        
        // Reload items from shared service
        OrderItems.Clear();
        foreach (var orderItem in _orderBuilder.OrderItems)
        {
            OrderItems.Add(orderItem);
        }
        
        OnOrderTotalChanged();
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        Logger.LogInformation("[WORKFLOW] Going back to Step 1");
        
        // No need to save to state - data is in shared service!
        await _navigator.NavigateBackAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        Logger.LogInformation("[WORKFLOW] Moving to Step 3 - Review (Order has {Count} items)", 
            _orderBuilder.OrderItems.Count);

        // No need to pass items - Step3 will get them from shared service!
        await _navigator.NavigateToAsync<DemoWorkflowStep3ViewModel, WorkflowState>(_state);
    }

    private bool CanGoNext() => _orderBuilder.OrderItems.Any();

    partial void OnOrderItemsChanged(ObservableCollection<WorkflowOrderItem> value)
    {
        NextCommand.NotifyCanExecuteChanged();
    }

    private void OnOrderTotalChanged()
    {
        OnPropertyChanged(nameof(OrderTotal));
        NextCommand.NotifyCanExecuteChanged();
    }

    public new void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[WORKFLOW] Step2 ViewModel disposed");

        // No need to close child windows - session handles that

        _disposed = true;
    }

    public override Task SaveAsync()
    {
        throw new NotImplementedException();
    }
}