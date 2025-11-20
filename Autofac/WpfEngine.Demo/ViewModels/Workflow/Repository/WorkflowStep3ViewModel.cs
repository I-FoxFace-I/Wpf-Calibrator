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
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Demo.ViewModels.Workflow.Repository;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;
using WpfEngine.ViewModels.Managed;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.ViewModels.Workflow.Repository;

/// <summary>
/// Workflow Step 3: Order Review and Completion
/// Uses Unit of Work pattern with Fluent API for complex transactional operations
/// DEMONSTRATES: Creating order with address and order items in a single transaction
/// </summary>
public partial class WorkflowStep3ViewModel : BaseViewModel, IInitializable
{
    private readonly IScopeManager _scopeManager;
    private readonly INavigator _navigator;
    private readonly IOrderBuilderService _orderBuilder;

    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private ObservableCollection<DemoAddress> _shippingAddresses = new();
    [ObservableProperty] private DemoAddress? _selectedShippingAddress;
    [ObservableProperty] private bool _isCreatingNewAddress;
    [ObservableProperty] private string _newStreet = string.Empty;
    [ObservableProperty] private string _newCity = string.Empty;
    [ObservableProperty] private string _newZipCode = string.Empty;
    [ObservableProperty] private string _newCountry = string.Empty;

    public ObservableCollection<WorkflowOrderItem> OrderItems => _orderBuilder.OrderItems;
    public decimal Subtotal => _orderBuilder.Subtotal;
    public decimal Tax => _orderBuilder.Tax;
    public decimal Total => _orderBuilder.Total;

    public WorkflowStep3ViewModel(
        IScopeManager scopeManager,
        INavigator navigator,
        IOrderBuilderService orderBuilder,
        ILogger<WorkflowStep3ViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _navigator = navigator;
        _orderBuilder = orderBuilder;
        
        CustomerName = orderBuilder.CustomerName;
        Logger.LogInformation("[WORKFLOW_STEP3] ViewModel created");
    }

    public override async Task InitializeAsync()
    {
        await LoadShippingAddressesAsync();
    }

    private async Task LoadShippingAddressesAsync()
    {
        try
        {
            IsBusy = true;

            if (!_orderBuilder.CustomerId.HasValue)
            {
                Logger.LogWarning("[WORKFLOW_STEP3] No customer selected");
                SetError("No customer selected");
                return;
            }

            // Use Fluent API to load shipping addresses
            var addresses = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.FindAsync(a => 
                        a.CustomerId == _orderBuilder.CustomerId.Value &&
                        (a.Type == AddressType.Shipping || a.Type == AddressType.Both));
                });

            ShippingAddresses.Clear();
            foreach (var address in addresses)
            {
                ShippingAddresses.Add(address);
            }

            if (ShippingAddresses.Any())
            {
                SelectedShippingAddress = ShippingAddresses.First();
            }

            Logger.LogInformation("[WORKFLOW_STEP3] Loaded {Count} shipping addresses", addresses.Count());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[WORKFLOW_STEP3] Error loading addresses");
            SetError("Failed to load addresses: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        Logger.LogInformation("[WORKFLOW_STEP3] Going back to Step 2");
        await _navigator.NavigateBackAsync();
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

    [RelayCommand(CanExecute = nameof(CanComplete))]
    private async Task CompleteAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Creating order...";

            // DEMONSTRATES Unit of Work pattern for complex transactional operation
            // Creating address (if needed) and order with items in a single transaction
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IUnitOfWork>()
                .ExecuteAsync(async (uow) =>
                {
                    int? shippingAddressId = null;

                    // Step 1: Create new address if needed
                    if (IsCreatingNewAddress)
                    {
                        if (string.IsNullOrWhiteSpace(NewStreet) ||
                            string.IsNullOrWhiteSpace(NewCity) ||
                            string.IsNullOrWhiteSpace(NewZipCode) ||
                            string.IsNullOrWhiteSpace(NewCountry))
                        {
                            Logger.LogWarning("[WORKFLOW_STEP3] Cannot create order - incomplete address");
                            throw new InvalidOperationException("Incomplete address information");
                        }

                        var addressRepo = uow.GetRepository<DemoAddress>();
                        var newAddress = new DemoAddress
                        {
                            CustomerId = _orderBuilder.CustomerId!.Value,
                            Street = NewStreet,
                            City = NewCity,
                            ZipCode = NewZipCode,
                            Country = NewCountry,
                            Type = AddressType.Shipping
                        };

                        await addressRepo.AddAsync(newAddress);
                        await uow.SaveChangesAsync();
                        shippingAddressId = newAddress.Id;

                        Logger.LogInformation("[WORKFLOW_STEP3] Created new shipping address");
                    }
                    else if (SelectedShippingAddress != null)
                    {
                        shippingAddressId = SelectedShippingAddress.Id;
                    }

                    // Step 2: Create order
                    var orderRepo = uow.GetRepository<DemoOrder>();
                    var orderItemRepo = uow.GetRepository<DemoOrderItem>();

                    var order = new DemoOrder
                    {
                        CustomerId = _orderBuilder.CustomerId!.Value,
                        OrderDate = DateTime.Now,
                        OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                        ShippingAddressId = shippingAddressId,
                        Status = OrderStatus.Pending,
                        CustomerName = _orderBuilder.CustomerName,
                        ShippingAddressText = SelectedShippingAddress?.FullAddress ?? string.Empty,
                        // Note: Subtotal, Tax, Total are computed properties based on Items
                    };

                    await orderRepo.AddAsync(order);
                    await uow.SaveChangesAsync();

                    // Step 3: Create order items
                    foreach (var item in _orderBuilder.OrderItems)
                    {
                        var orderItem = new DemoOrderItem
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };

                        await orderItemRepo.AddAsync(orderItem);
                        await uow.SaveChangesAsync();
                    }


                    // UnitOfWork will automatically commit the transaction (SaveChangesAsync)
                    Logger.LogInformation("[WORKFLOW_STEP3] ✅ Order created! Order: {OrderNumber}, Items: {ItemCount}, Total: {Total:C}",
                        order.OrderNumber, OrderItems.Count, Total);

                    await uow.SaveChangesAsync();
                });

            // Request shell close via ContentManager
            await _navigator.RequestCloseAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[WORKFLOW_STEP3] Error creating order");
            SetError($"Failed to create order: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    private bool CanComplete()
    {
        if (IsCreatingNewAddress)
        {
            return !string.IsNullOrWhiteSpace(NewStreet) &&
                   !string.IsNullOrWhiteSpace(NewCity) &&
                   !string.IsNullOrWhiteSpace(NewZipCode) &&
                   !string.IsNullOrWhiteSpace(NewCountry);
        }

        return SelectedShippingAddress != null;
    }

    partial void OnIsCreatingNewAddressChanged(bool value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnSelectedShippingAddressChanged(DemoAddress? value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnNewStreetChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnNewCityChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnNewZipCodeChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnNewCountryChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
}