using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Orders;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Abstract;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Order list ViewModel with navigation to detail
/// </summary>
public partial class OrderListViewModel : BaseViewModel, IInitializable
{
    private readonly IQueryHandler<GetAllDemoOrdersQuery, List<DemoOrder>> _getAllOrdersHandler;
    private readonly ICommandHandler<DeleteDemoOrderCommand> _deleteOrderHandler;
    private readonly IWindowContext _windowService;
    
    [ObservableProperty]
    private ObservableCollection<DemoOrder> _orders = new();
    
    [ObservableProperty]
    private DemoOrder? _selectedOrder;
    
    public OrderListViewModel(
        IQueryHandler<GetAllDemoOrdersQuery, List<DemoOrder>> getAllOrdersHandler,
        ICommandHandler<DeleteDemoOrderCommand> deleteOrderHandler,
        IWindowContext WindowService,
        ILogger<OrderListViewModel> logger) : base(logger)
    {
        _getAllOrdersHandler = getAllOrdersHandler;
        _deleteOrderHandler = deleteOrderHandler;
        _windowService = WindowService;
        
        Logger.LogInformation("OrderListViewModel created");
    }
    
    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        await LoadOrdersAsync();
    }
    
    private async Task LoadOrdersAsync()
    {
        try
        {
            IsBusy = true;
            
            var orders = await _getAllOrdersHandler.HandleAsync(new GetAllDemoOrdersQuery());
            
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
            
            Logger.LogInformation("Loaded {Count} orders", orders.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading orders");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadOrdersAsync();
    }
    
    [RelayCommand]
    private void ViewOrderDetail(DemoOrder? order)
    {
        if (order == null) return;
        
        Logger.LogInformation("Opening order detail for order {OrderId}", order.Id);
        
        // Open as regular window (ViewModel.Id is not window ID in non-session context)
        _windowService.OpenWindow<OrderDetailViewModel, OrderDetailParameters>(
            new OrderDetailParameters { OrderId = order.Id }
        );
    }
    
    [RelayCommand(CanExecute = nameof(HasSelectedOrder))]
    private async Task DeleteOrderAsync()
    {
        if (SelectedOrder == null) return;
        
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete order {SelectedOrder.OrderNumber}?",
            "Delete Order",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        
        if (result != System.Windows.MessageBoxResult.Yes)
            return;
        
        try
        {
            IsBusy = true;
            
            await _deleteOrderHandler.HandleAsync(new DeleteDemoOrderCommand(SelectedOrder.Id));
            
            Logger.LogInformation("Deleted order {OrderId}", SelectedOrder.Id);
            
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting order");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool HasSelectedOrder() => SelectedOrder != null;
    
    partial void OnSelectedOrderChanged(DemoOrder? value)
    {
        DeleteOrderCommand.NotifyCanExecuteChanged();
    }
}
