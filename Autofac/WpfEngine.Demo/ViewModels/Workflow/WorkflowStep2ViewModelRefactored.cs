using System.Collections.ObjectModel;
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
using WpfEngine.ViewModels.Managed;

namespace WpfEngine.Demo.ViewModels;

public partial class WorkflowStep2ViewModelRefactored : StepViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllProductsHandler;
    private readonly INavigator _navigator;
    private readonly IOrderBuilderService _orderBuilder;
    private readonly IWindowContext _windowContext;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();

    [ObservableProperty]
    private int _quantity = 1;

    public ObservableCollection<WorkflowOrderItem> OrderItems => _orderBuilder.OrderItems;
    public decimal OrderTotal => _orderBuilder.Total;

    private bool _disposed;

    public WorkflowStep2ViewModelRefactored(
        IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> getAllProductsHandler,
        INavigator navigator,
        IOrderBuilderService orderBuilder,
        IWindowContext windowContext,
        ILogger<WorkflowStep2ViewModelRefactored> logger)
        : base(logger, navigator)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _navigator = navigator;
        _orderBuilder = orderBuilder;
        _windowContext = windowContext;
        
        CustomerName = orderBuilder.CustomerName;
        _orderBuilder.OrderItemsChanged += OnOrderItemsChanged;
        Logger.LogInformation("[WORKFLOW_STEP2] ViewModel created");
    }


    public override Task InitializeAsync()
    {
        return InitializeAsync(CancellationToken.None);
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

            Logger.LogInformation("[WORKFLOW_STEP2] Loaded {Count} products, order has {OrderCount} items",
                Products.Count, OrderItems.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // Save state to shared service (already done via direct manipulation)
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenProductSelector()
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Opening product selector as child");

        // Opens child window - will see same IOrderBuilderService from parent window scope!
        _windowContext.OpenWindow<ProductSelectorViewModel>();
    }

    [RelayCommand]
    private void ViewProductInfo(DemoProduct? product)
    {
        if (product == null) return;

        Logger.LogInformation("[WORKFLOW_STEP2] Opening product info for {ProductId}", product.Id);

        // Opens child window from shell
        _windowContext.OpenWindow<ProductInfoViewModel, ProductDetailParameters>(
            new ProductDetailParameters { ProductId = product.Id }
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
    private async Task BackAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Going back to Step 1");
        await _navigator.NavigateBackAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Moving to Step 3 - Review ({Count} items in shared order)",
            _orderBuilder.OrderItems.Count);

        await _navigator.NavigateToAsync<WorkflowStep3ViewModelRefactored>();
    }

    private bool CanGoNext() => _orderBuilder.OrderItems.Any();

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        base.Dispose(disposing);


        Logger.LogInformation("[WORKFLOW_STEP2] ViewModel disposed");

        // Unsubscribe from events
        if (_orderBuilder != null)
        {
            _orderBuilder.OrderItemsChanged -= OnOrderItemsChanged;
        }

        _disposed = true;
    }

    private void OnOrderItemsChanged(object? sender, EventArgs? args)
    {
        OnPropertyChanged(nameof(OrderItems));
        OnPropertyChanged(nameof(OrderTotal));
        NextCommand.NotifyCanExecuteChanged();
    }

    public override Task<bool> ValidateStepAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<bool>(CanGoNext());
    }
}
