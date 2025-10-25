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

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Step 2: Add products to order
/// 
/// SHARED STATE:
/// - IOrderBuilderService is SHARED across workflow session
/// - All windows in session see the SAME instance
/// - Can open product selector/detail windows that also see same service
/// </summary>
public partial class DemoWorkflowStep2ViewModelRefactored : BaseStepViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllProductsHandler;
    private readonly IContentManager _contentManager;
    private readonly IWorkflowSession? _workflowSession;
    private readonly IOrderBuilderService _orderBuilder; // ← SHARED from session!

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();

    [ObservableProperty]
    private int _quantity = 1;

    // OrderItems come from SHARED service
    public ObservableCollection<WorkflowOrderItem> OrderItems => _orderBuilder.OrderItems;
    public decimal OrderTotal => _orderBuilder.Total;

    private bool _disposed;

    public DemoWorkflowStep2ViewModelRefactored(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllProductsHandler,
        IContentManager contentManager,
        IOrderBuilderService orderBuilder,  // ← INJECTED from session scope!
        ILogger<DemoWorkflowStep2ViewModelRefactored> logger,
        IWorkflowSession? workflowSession = null)
        : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _contentManager = contentManager;
        _orderBuilder = orderBuilder;
        _workflowSession = workflowSession;
        
        CustomerName = orderBuilder.CustomerName;
        
        // Subscribe to order changes
        _orderBuilder.OrderItemsChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(OrderItems));
            OnPropertyChanged(nameof(OrderTotal));
            NextCommand.NotifyCanExecuteChanged();
        };

        Logger.LogInformation("[WORKFLOW_STEP2] ViewModel created (Session: {HasSession})", 
            workflowSession != null);
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

            Logger.LogInformation("[WORKFLOW_STEP2] Loaded {Count} products, order has {OrderCount} items", 
                Products.Count, OrderItems.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenProductSelector()
    {
        if (_workflowSession == null) return;

        Logger.LogInformation("[WORKFLOW_STEP2] Opening product selector in session");

        // Opens NEW window IN SESSION - will see same IOrderBuilderService!
        _workflowSession.OpenWindow<ProductSelectorViewModel>();
    }

    [RelayCommand]
    private void ViewProductInfo(DemoProduct? product)
    {
        if (product == null || _workflowSession == null) return;

        Logger.LogInformation("[WORKFLOW_STEP2] Opening product info for {ProductId}", product.Id);

        // Opens child window IN SESSION
        _workflowSession.OpenChildWindow<DemoProductInfoViewModel, DemoProductInfoParams>(
            this.Id,
            new DemoProductInfoParams { ProductId = product.Id }
        );
    }

    [RelayCommand]
    private void AddItemWithProduct(DemoProduct? product)
    {
        if (product == null || Quantity <= 0) return;

        // Add to SHARED service
        _orderBuilder.AddItem(product.Id, product.Name, product.Price, Quantity);

        Logger.LogInformation("[WORKFLOW_STEP2] Added {Quantity}x {Product} to shared order",
            Quantity, product.Name);
    }

    [RelayCommand]
    private void RemoveItem(WorkflowOrderItem? item)
    {
        if (item == null) return;

        _orderBuilder.RemoveItem(item);
        
        Logger.LogInformation("[WORKFLOW_STEP2] Removed {Product} from shared order", item.ProductName);
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Going back to Step 1");
        await _contentManager.NavigateBackAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Moving to Step 3 - Review ({Count} items in shared order)", 
            _orderBuilder.OrderItems.Count);

        await _contentManager.NavigateToAsync<DemoWorkflowStep3ViewModelRefactored>();
    }

    private bool CanGoNext() => _orderBuilder.OrderItems.Any();

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[WORKFLOW_STEP2] ViewModel disposed");

        // Unsubscribe from events
        if (_orderBuilder != null)
        {
            _orderBuilder.OrderItemsChanged -= (s, e) => { };
        }

        _disposed = true;
    }

    public override Task SaveAsync()
    {
        // Save state to shared service (already done via direct manipulation)
        return Task.CompletedTask;
    }
}

