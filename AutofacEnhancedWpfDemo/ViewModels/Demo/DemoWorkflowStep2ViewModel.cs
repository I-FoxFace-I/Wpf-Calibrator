using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Demo.Products;
using AutofacEnhancedWpfDemo.Models.Demo;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

/// <summary>
/// Step 2: Add products to order
/// </summary>
public partial class DemoWorkflowStep2ViewModel : BaseViewModel, IAsyncInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoProductsQuery, List<DemoProduct>> _getAllProductsHandler;
    private readonly INavigator _navigator;
    private readonly IWindowManager _windowManager;
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
        INavigator navigator,
        IWindowManager windowManager,
        ILogger<DemoWorkflowStep2ViewModel> logger,
        WorkflowState state) : base(logger)
    {
        _getAllProductsHandler = getAllProductsHandler;
        _navigator = navigator;
        _windowManager = windowManager;
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

        _windowManager.ShowChildWindow<DemoProductInfoViewModel>(
            Guid.NewGuid(),
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

        await _navigator.NavigateToAsync<DemoWorkflowStep3ViewModel>(_state);
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

        Logger.LogInformation("[WORKFLOW] Step2 ViewModel disposed - closing child windows");

        _windowManager.CloseAllChildWindows();

        _disposed = true;
    }
}