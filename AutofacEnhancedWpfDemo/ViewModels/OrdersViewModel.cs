using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Orders;
using AutofacEnhancedWpfDemo.Models;
using AutofacEnhancedWpfDemo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Orders list window
/// </summary>
public partial class OrdersViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetAllOrdersQuery, List<Order>> _getAllOrdersHandler;
    private readonly IWindowNavigator _navigator;

    [ObservableProperty]
    private ObservableCollection<Order> _orders = new();

    [ObservableProperty]
    private Order? _selectedOrder;

    public OrdersViewModel(
        IQueryHandler<GetAllOrdersQuery, List<Order>> getAllOrdersHandler,
        IWindowNavigator navigator,
        ILogger<OrdersViewModel> logger) : base(logger)
    {
        _getAllOrdersHandler = getAllOrdersHandler;
        _navigator = navigator;
    }

    public async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            Logger.LogInformation("Loading orders");
            var orders = await _getAllOrdersHandler.HandleAsync(new GetAllOrdersQuery());

            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }

            Logger.LogInformation("Loaded {Count} orders", Orders.Count);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load orders: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanViewDetail))]
    private async Task ViewOrderDetailAsync()
    {
        if (SelectedOrder == null) return;

        Logger.LogInformation("Opening order detail dialog for order {OrderId}", SelectedOrder.Id);

        await _navigator.ShowDialogAsync<OrderDetailViewModel, object?>(
            new OrderDetailParams { OrderId = SelectedOrder.Id }
        );
    }

    private bool CanViewDetail() => SelectedOrder != null && !IsBusy;

    partial void OnSelectedOrderChanged(Order? value)
    {
        ViewOrderDetailCommand.NotifyCanExecuteChanged();
    }
}

// ========== DTOs ==========

public record OrderDetailParams
{
    public int OrderId { get; init; }
}