using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Orders;
using AutofacEnhancedWpfDemo.Models;
using AutofacEnhancedWpfDemo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Order Detail dialog
/// </summary>
public partial class OrderDetailViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetOrderByIdQuery, Order?> _getOrderHandler;
    private readonly IWindowNavigator _navigator;
    private readonly int _orderId;

    [ObservableProperty]
    private Order? _order;

    public OrderDetailViewModel(
        IQueryHandler<GetOrderByIdQuery, Order?> getOrderHandler,
        IWindowNavigator navigator,
        ILogger<OrderDetailViewModel> logger,
        OrderDetailParams parameters) : base(logger)
    {
        _getOrderHandler = getOrderHandler;
        _navigator = navigator;
        _orderId = parameters.OrderId;
    }

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            Logger.LogInformation("Loading order {OrderId}", _orderId);

            var order = await _getOrderHandler.HandleAsync(new GetOrderByIdQuery(_orderId));

            if (order != null)
            {
                Order = order;
            }
            else
            {
                SetError("Order not found");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load order: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Close()
    {
        Logger.LogInformation("Closing order detail");
        _navigator.CloseDialog<OrderDetailViewModel>();
    }
}