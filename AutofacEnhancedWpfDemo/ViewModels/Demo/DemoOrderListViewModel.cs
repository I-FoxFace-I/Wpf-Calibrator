using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Demo.Orders;
using AutofacEnhancedWpfDemo.Models.Demo;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

/// <summary>
/// Order list ViewModel with navigation to detail
/// </summary>
public partial class DemoOrderListViewModel : BaseViewModel, IAsyncInitializable
{
    private readonly IQueryHandler<GetAllDemoOrdersQuery, List<DemoOrder>> _getAllOrdersHandler;
    private readonly ICommandHandler<DeleteDemoOrderCommand> _deleteOrderHandler;
    private readonly IWindowManager _windowManager;
    
    [ObservableProperty]
    private ObservableCollection<DemoOrder> _orders = new();
    
    [ObservableProperty]
    private DemoOrder? _selectedOrder;
    
    public DemoOrderListViewModel(
        IQueryHandler<GetAllDemoOrdersQuery, List<DemoOrder>> getAllOrdersHandler,
        ICommandHandler<DeleteDemoOrderCommand> deleteOrderHandler,
        IWindowManager windowManager,
        ILogger<DemoOrderListViewModel> logger) : base(logger)
    {
        _getAllOrdersHandler = getAllOrdersHandler;
        _deleteOrderHandler = deleteOrderHandler;
        _windowManager = windowManager;
        
        Logger.LogInformation("DemoOrderListViewModel created");
    }
    
    public async Task InitializeAsync()
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
        
        _windowManager.ShowChildWindow<DemoOrderDetailViewModel>(
            Guid.NewGuid(),
            new DemoOrderDetailParams { OrderId = order.Id }
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

public record DemoOrderDetailParams
{
    public int OrderId { get; init; }
}
