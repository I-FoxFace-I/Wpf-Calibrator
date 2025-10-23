using System.Collections.ObjectModel;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Demo.Products;
using AutofacEnhancedWpfDemo.Models.Demo;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

// ========== STEP 2: ADD PRODUCTS ==========

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
            
            // Restore items if going back
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
    private void AddItem(object? parameter)
    {
        if (parameter is not DemoProduct product)
            return;

        var existingItem = OrderItems.FirstOrDefault(i => i.ProductId == product.Id);
        
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            OrderItems.Add(new WorkflowOrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = 1
            });
        }
        
        OnPropertyChanged(nameof(OrderTotal));
        NextCommand.NotifyCanExecuteChanged();
        
        Logger.LogInformation("[WORKFLOW] Added product {ProductName} to order", product.Name);
    }
    
    [RelayCommand]
    private void RemoveItem(object? parameter)
    {
        if (parameter is not WorkflowOrderItem item)
            return;

        OrderItems.Remove(item);
        OnPropertyChanged(nameof(OrderTotal));
        NextCommand.NotifyCanExecuteChanged();
        
        Logger.LogInformation("[WORKFLOW] Removed {ProductName} from order", item.ProductName);
    }
    
    [RelayCommand]
    private async Task BackAsync()
    {
        Logger.LogInformation("[WORKFLOW] Going back to Step 1");
        await _navigator.NavigateBackAsync();
    }
    
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        Logger.LogInformation("[WORKFLOW] Moving to Step 3 for review");
        
        _state.OrderItems = OrderItems.ToList();
        
        await _navigator.NavigateToAsync<DemoWorkflowStep3ViewModel>(_state);
    }
    
    private bool CanGoNext() => OrderItems.Count > 0;
    
    public void Dispose()
    {
        if (_disposed) return;
        
        Logger.LogInformation("[WORKFLOW] Step2 ViewModel disposed - closing child windows");
        
        // Close all child windows opened from this step
        _windowManager.CloseAllChildWindows();
        
        _disposed = true;
    }
}
