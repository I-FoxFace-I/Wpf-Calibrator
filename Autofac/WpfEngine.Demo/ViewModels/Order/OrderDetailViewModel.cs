using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Addresses;
using WpfEngine.Demo.Application.Orders;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Abstract;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Order detail ViewModel with ability to change shipping address
/// </summary>
public partial class OrderDetailViewModel : BaseViewModel, IInitializable
{
    private readonly IQueryHandler<GetDemoOrderByIdQuery, DemoOrder?> _getOrderHandler;
    private readonly IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> _getShippingAddressesHandler;
    private readonly ICommandHandler<CreateAddressCommand> _createAddressHandler;
    private readonly ICommandHandler<UpdateDemoOrderCommand> _updateOrderHandler;
    private readonly int _orderId;
    
    [ObservableProperty]
    private DemoOrder? _order;
    
    [ObservableProperty]
    private ObservableCollection<DemoAddress> _availableAddresses = new();
    
    [ObservableProperty]
    private DemoAddress? _selectedAddress;
    
    [ObservableProperty]
    private bool _isEditingAddress;
    
    [ObservableProperty]
    private bool _isCreatingNewAddress;
    
    [ObservableProperty]
    private string _newStreet = string.Empty;
    
    [ObservableProperty]
    private string _newCity = string.Empty;
    
    [ObservableProperty]
    private string _newZipCode = string.Empty;
    
    [ObservableProperty]
    private string _newCountry = string.Empty;
    
    [ObservableProperty]
    private OrderStatus _selectedStatus;
    
    public Array OrderStatuses => Enum.GetValues(typeof(OrderStatus));
    
    public OrderDetailViewModel(
        IQueryHandler<GetDemoOrderByIdQuery, DemoOrder?> getOrderHandler,
        IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> getShippingAddressesHandler,
        ICommandHandler<CreateAddressCommand> createAddressHandler,
        ICommandHandler<UpdateDemoOrderCommand> updateOrderHandler,
        ILogger<OrderDetailViewModel> logger,
        OrderDetailParameters parameters) : base(logger)
    {
        _getOrderHandler = getOrderHandler;
        _getShippingAddressesHandler = getShippingAddressesHandler;
        _createAddressHandler = createAddressHandler;
        _updateOrderHandler = updateOrderHandler;
        _orderId = parameters.OrderId;
        
        Logger.LogInformation("OrderDetailViewModel created for order {OrderId}", _orderId);
    }
    
    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        await LoadOrderAsync();
    }
    
    private async Task LoadOrderAsync()
    {
        try
        {
            IsBusy = true;
            
            var order = await _getOrderHandler.HandleAsync(new GetDemoOrderByIdQuery(_orderId));
            
            if (order == null)
            {
                Logger.LogWarning("Order {OrderId} not found", _orderId);
                return;
            }
            
            Order = order;
            SelectedStatus = order.Status;
            
            var addresses = await _getShippingAddressesHandler.HandleAsync(
                new GetShippingAddressesQuery(order.CustomerId)
            );
            
            AvailableAddresses.Clear();
            foreach (var address in addresses)
            {
                AvailableAddresses.Add(address);
            }
            
            if (order.ShippingAddressId.HasValue)
            {
                SelectedAddress = AvailableAddresses.FirstOrDefault(a => a.Id == order.ShippingAddressId.Value);
            }
            
            Logger.LogInformation("Loaded order {OrderNumber} with {ItemCount} items", 
                order.OrderNumber, order.Items.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading order");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void StartEditingAddress()
    {
        IsEditingAddress = true;
        Logger.LogInformation("Started editing shipping address");
    }
    
    [RelayCommand]
    private void CancelEditingAddress()
    {
        IsEditingAddress = false;
        IsCreatingNewAddress = false;
        NewStreet = string.Empty;
        NewCity = string.Empty;
        NewZipCode = string.Empty;
        NewCountry = string.Empty;
        
        if (Order?.ShippingAddressId.HasValue == true)
        {
            SelectedAddress = AvailableAddresses.FirstOrDefault(a => a.Id == Order.ShippingAddressId.Value);
        }
        
        Logger.LogInformation("Cancelled address editing");
    }
    
    [RelayCommand]
    private void ToggleNewAddress()
    {
        IsCreatingNewAddress = !IsCreatingNewAddress;
        
        if (!IsCreatingNewAddress)
        {
            NewStreet = string.Empty;
            NewCity = string.Empty;
            NewZipCode = string.Empty;
            NewCountry = string.Empty;
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private async Task SaveChangesAsync()
    {
        if (Order == null) return;
        
        try
        {
            IsBusy = true;
            
            int? addressId = null;
            
            if (IsCreatingNewAddress)
            {
                if (string.IsNullOrWhiteSpace(NewStreet) || 
                    string.IsNullOrWhiteSpace(NewCity) ||
                    string.IsNullOrWhiteSpace(NewZipCode) ||
                    string.IsNullOrWhiteSpace(NewCountry))
                {
                    Logger.LogWarning("Cannot save - incomplete address");
                    return;
                }
                
                await _createAddressHandler.HandleAsync(new CreateAddressCommand(
                    Order.CustomerId,
                    NewStreet,
                    NewCity,
                    NewZipCode,
                    NewCountry,
                    AddressType.Shipping
                ));
                
                var addresses = await _getShippingAddressesHandler.HandleAsync(
                    new GetShippingAddressesQuery(Order.CustomerId)
                );
                
                AvailableAddresses.Clear();
                foreach (var address in addresses)
                {
                    AvailableAddresses.Add(address);
                }
                
                addressId = AvailableAddresses.Last().Id;
                
                Logger.LogInformation("Created new shipping address");
            }
            else if (SelectedAddress != null)
            {
                addressId = SelectedAddress.Id;
            }
            
            await _updateOrderHandler.HandleAsync(new UpdateDemoOrderCommand(
                Order.Id,
                addressId,
                SelectedStatus
            ));
            
            Logger.LogInformation("Updated order {OrderId}", Order.Id);
            
            IsEditingAddress = false;
            IsCreatingNewAddress = false;
            
            await LoadOrderAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving changes");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private bool CanSaveChanges()
    {
        if (IsCreatingNewAddress)
        {
            return !string.IsNullOrWhiteSpace(NewStreet) &&
                   !string.IsNullOrWhiteSpace(NewCity) &&
                   !string.IsNullOrWhiteSpace(NewZipCode) &&
                   !string.IsNullOrWhiteSpace(NewCountry);
        }
        
        return true;
    }
    
    partial void OnIsCreatingNewAddressChanged(bool value)
    {
        SaveChangesCommand.NotifyCanExecuteChanged();
    }
    
    partial void OnNewStreetChanged(string value) => SaveChangesCommand.NotifyCanExecuteChanged();
    partial void OnNewCityChanged(string value) => SaveChangesCommand.NotifyCanExecuteChanged();
    partial void OnNewZipCodeChanged(string value) => SaveChangesCommand.NotifyCanExecuteChanged();
    partial void OnNewCountryChanged(string value) => SaveChangesCommand.NotifyCanExecuteChanged();
}
