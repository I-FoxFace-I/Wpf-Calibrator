using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Products;
using WpfEngine.Demo.Models;
using WpfEngine.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Step 2: Add products to order
/// </summary>
public partial class DemoWorkflowStep2ViewModel : BaseStepViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllProductsHandler;
    private readonly INavigationService _navigator;
    private readonly IWindowService _windowService;
    private readonly WorkflowState _state;

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
        IWindowService WindowService,
        ILogger<DemoWorkflowStep2ViewModel> logger,
        WorkflowState state) : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _navigator = navigator;
        _windowService = WindowService;
        _state = state;
        CustomerName = state.CustomerName;

        Logger.LogInformation("[WORKFLOW] Step2 ViewModel created");
    }

    public async Task InitializeAsync()
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

            if (_state.OrderItems != null)
            {
                OrderItems.Clear();
                foreach (var item in _state.OrderItems)
                {
                    OrderItems.Add(item);
                }
            }

            Logger.LogInformation("[WORKFLOW] Step2 loaded {Count} products", Products.Count);
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

        // Open as regular window (not child) - original approach doesn't track window IDs
        _windowService.OpenWindow<DemoProductInfoViewModel, DemoProductInfoParams>(
            new DemoProductInfoParams { ProductId = product.Id }
        );
    }

    [RelayCommand]
    private void AddItemWithProduct(DemoProduct? product)
    {
        if (product == null || Quantity <= 0) return;

        var existingItem = OrderItems.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += Quantity;
        }
        else
        {
            OrderItems.Add(new WorkflowOrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = Quantity
            });
        }

        Logger.LogInformation("[WORKFLOW] Added {Quantity}x {Product} to order", Quantity, product.Name);

        OnOrderTotalChanged();
    }

    [RelayCommand]
    private void RemoveItem(WorkflowOrderItem? item)
    {
        if (item == null) return;

        if (OrderItems.Remove(item))
        {
            Logger.LogInformation("[WORKFLOW] Removed {Product} from order", item.ProductName);
            OnOrderItemsChanged(OrderItems);
            OnOrderTotalChanged();
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        Logger.LogInformation("[WORKFLOW] Going back to Step 1");

        _state.OrderItems = OrderItems.ToList();

        await _navigator.NavigateBackAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        Logger.LogInformation("[WORKFLOW] Moving to Step 3 - Review");

        _state.OrderItems = OrderItems.ToList();

        await _navigator.NavigateToAsync<DemoWorkflowStep3ViewModel, WorkflowState>(_state);
    }

    private bool CanGoNext() => OrderItems.Any();

    partial void OnOrderItemsChanged(ObservableCollection<WorkflowOrderItem> value)
    {
        NextCommand.NotifyCanExecuteChanged();
    }

    private void OnOrderTotalChanged()
    {
        OnPropertyChanged(nameof(OrderTotal));
        NextCommand.NotifyCanExecuteChanged();
    }


    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[WORKFLOW] Step2 ViewModel disposed");

        // NOTE: CloseAllChildWindows needs window ID, not ViewModel ID
        // In non-session context, we don't track this properly
        // Windows will close when their parent scope disposes anyway

        _disposed = true;
    }

    public override Task SaveAsync()
    {
        throw new NotImplementedException();
    }
}