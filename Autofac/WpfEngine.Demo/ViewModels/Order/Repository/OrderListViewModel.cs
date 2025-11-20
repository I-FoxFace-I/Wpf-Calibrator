using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using WpfEngine.Abstract;
using WpfEngine.Data.Dialogs;
using WpfEngine.Data.Sessions;
using WpfEngine.Data.Windows.Events;
using WpfEngine.Data;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Repositories;
using WpfEngine.Demo.Services;
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Demo.ViewModels.Workflow.Repository;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;
using WpfEngine.ViewModels.Managed;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.ViewModels.Order.Repository;

/// <summary>
/// Order list ViewModel using Repository + Unit of Work pattern with Fluent API
/// </summary>
public partial class OrderListViewModel : BaseViewModel, IInitializable
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowService;
    
    [ObservableProperty]
    private ObservableCollection<DemoOrder> _orders = new();
    
    [ObservableProperty]
    private DemoOrder? _selectedOrder;
    
    public OrderListViewModel(
        IScopeManager scopeManager,
        IWindowContext windowService,
        ILogger<OrderListViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        
        Logger.LogInformation("[DEMO_V2] OrderListViewModel created");
    }
    
    public override async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }
    
    [RelayCommand]
    private async Task LoadOrdersAsync(CancellationToken cancelationToken = default)
    {
        try
        {
            IsBusy = true;
            ClearError();

            // Use Fluent API with database session
            var orders = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IOrderRepository>()
                .ExecuteWithResultAsync(async (repo) => await repo.GetAllOrdersAsync(cancelationToken));
            
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
            
            Logger.LogInformation("[DEMO_V2] Loaded {Count} orders", orders.Count());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error loading orders");
            SetError("Failed to load orders: " + ex.Message);
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
    private void ViewDetail()
    {
        if (SelectedOrder == null) return;
        
        Logger.LogInformation("[DEMO_V2] Opening order detail for order {OrderId}", SelectedOrder.Id);
        
        _windowService.OpenWindow<OrderDetailViewModel, OrderDetailParameters>(
            new OrderDetailParameters { OrderId = SelectedOrder.Id }
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
            ClearError();
            
            // Use Fluent API to delete order
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoOrder>>()
                .ExecuteAsync(async (repo) =>
                {
                    var order = await repo.GetByIdAsync(SelectedOrder.Id);
                    if (order != null)
                    {
                        await repo.DeleteAsync(order);
                        Logger.LogInformation("[DEMO_V2] Deleted order {OrderId}", order.Id);
                    }
                });
            
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error deleting order");
            SetError("Failed to delete order: " + ex.Message);
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