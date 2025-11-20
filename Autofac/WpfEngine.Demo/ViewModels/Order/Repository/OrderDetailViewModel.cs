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
/// Order detail ViewModel using Repository pattern with Fluent API
/// </summary>
public partial class OrderDetailViewModel : BaseViewModel, IInitializable
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly int _orderId;
    
    [ObservableProperty] private DemoOrder? _order;
    [ObservableProperty] private ObservableCollection<DemoAddress> _availableAddresses = new();
    [ObservableProperty] private DemoAddress? _selectedAddress;
    [ObservableProperty] private bool _isEditingAddress;
    [ObservableProperty] private bool _isCreatingNewAddress;
    [ObservableProperty] private string _newStreet = string.Empty;
    [ObservableProperty] private string _newCity = string.Empty;
    [ObservableProperty] private string _newZipCode = string.Empty;
    [ObservableProperty] private string _newCountry = string.Empty;
    [ObservableProperty] private OrderStatus _selectedStatus;
    
    public Array OrderStatuses => Enum.GetValues(typeof(OrderStatus));
    
    public OrderDetailViewModel(
        IScopeManager scopeManager,
        IWindowContext windowContext,
        ILogger<OrderDetailViewModel> logger,
        OrderDetailParameters parameters) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        _orderId = parameters.OrderId;
        
        Logger.LogInformation("[DEMO_V2] OrderDetailViewModel created for order {OrderId}", _orderId);
    }
    
    public override async Task InitializeAsync()
    {
        await LoadOrderAsync();
    }
    
    private async Task LoadOrderAsync(CancellationToken cancelationToken = default)
    {
        try
        {
            IsBusy = true;
            ClearError();
            
            // Use Fluent API to load order with related data
            (var order, var addresses) = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IOrderRepository>()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteWithResultAsync(async (orderRepo, addressRepo) =>
                {
                    var ord = await orderRepo.GetOrderAsync(_orderId, cancelationToken);
                    
                    if (ord == null)
                    {
                        return (ord, Enumerable.Empty<DemoAddress>());
                    }
                    
                    var addrs = await addressRepo.FindAsync(a => a.CustomerId == ord.CustomerId, cancelationToken);
                    
                    return (ord, addrs);
                });
            
            if (order == null)
            {
                Logger.LogWarning("[DEMO_V2] Order {OrderId} not found", _orderId);
                SetError($"Order {_orderId} not found");
                return;
            }
            
            Order = order;
            SelectedStatus = order.Status;
            
            AvailableAddresses.Clear();
            foreach (var address in order.Customer.Addresses)
            {
                AvailableAddresses.Add(address);
            }
            
            if (order.ShippingAddress != null)
            {
                SelectedAddress = order.ShippingAddress;
            }
            
            Logger.LogInformation("[DEMO_V2] Loaded order {OrderNumber} with {ItemCount} items", 
                order.OrderNumber, order.Items.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error loading order");
            SetError("Failed to load order: " + ex.Message);
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
        Logger.LogInformation("[DEMO_V2] Started editing shipping address");
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
        
        Logger.LogInformation("[DEMO_V2] Cancelled address editing");
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
            ClearError();
            
            int? addressId = null;
            
            // Use Fluent API to save changes
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoOrder>>()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteAsync(async (orderRepo, addressRepo) =>
                {
                    if (IsCreatingNewAddress)
                    {
                        if (string.IsNullOrWhiteSpace(NewStreet) || 
                            string.IsNullOrWhiteSpace(NewCity) ||
                            string.IsNullOrWhiteSpace(NewZipCode) ||
                            string.IsNullOrWhiteSpace(NewCountry))
                        {
                            Logger.LogWarning("[DEMO_V2] Cannot save - incomplete address");
                            return;
                        }
                        
                        var newAddress = new DemoAddress
                        {
                            CustomerId = Order.CustomerId,
                            Street = NewStreet,
                            City = NewCity,
                            ZipCode = NewZipCode,
                            Country = NewCountry,
                            Type = AddressType.Shipping
                        };
                        
                        await addressRepo.AddAsync(newAddress);
                        addressId = newAddress.Id;
                        
                        Logger.LogInformation("[DEMO_V2] Created new shipping address");
                    }
                    else if (SelectedAddress != null)
                    {
                        addressId = SelectedAddress.Id;
                    }
                    
                    var order = await orderRepo.GetByIdAsync(Order.Id);
                    if (order != null)
                    {
                        order.ShippingAddressId = addressId;
                        order.Status = SelectedStatus;
                        await orderRepo.UpdateAsync(order);
                        Logger.LogInformation("[DEMO_V2] Updated order {OrderId}", Order.Id);
                    }
                });
            
            IsEditingAddress = false;
            IsCreatingNewAddress = false;
            
            await LoadOrderAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error saving changes");
            SetError("Failed to save changes: " + ex.Message);
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