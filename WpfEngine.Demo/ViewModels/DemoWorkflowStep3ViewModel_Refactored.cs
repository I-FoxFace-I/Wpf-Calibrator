using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Addresses;
using WpfEngine.Demo.Application.Orders;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Step 3: Review order and complete
/// Uses shared IOrderBuilderService from session
/// </summary>
public partial class DemoWorkflowStep3ViewModelRefactored : BaseViewModel, IInitializable
{
    private readonly IContentManager _contentManager;
    private readonly IOrderBuilderService _orderBuilder; // SHARED from session!
    private readonly IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> _getShippingAddressesHandler;
    private readonly ICommandHandler<CreateAddressCommand> _createAddressHandler;
    private readonly ICommandHandler<CreateDemoOrderCommand> _createOrderHandler;

    [ObservableProperty]
    private string _customerName = string.Empty;

    // OrderItems from SHARED service
    public ObservableCollection<WorkflowOrderItem> OrderItems => _orderBuilder.OrderItems;

    [ObservableProperty]
    private ObservableCollection<DemoAddress> _shippingAddresses = new();

    [ObservableProperty]
    private DemoAddress? _selectedShippingAddress;

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

    public decimal Subtotal => _orderBuilder.Subtotal;
    public decimal Tax => _orderBuilder.Tax;
    public decimal Total => _orderBuilder.Total;

    public DemoWorkflowStep3ViewModelRefactored(
        IContentManager contentManager,
        IOrderBuilderService orderBuilder,  // ← SHARED from session!
        ILogger<DemoWorkflowStep3ViewModelRefactored> logger,
        IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> getShippingAddressesHandler,
        ICommandHandler<CreateAddressCommand> createAddressHandler,
        ICommandHandler<CreateDemoOrderCommand> createOrderHandler) : base(logger)
    {
        _contentManager = contentManager;
        _orderBuilder = orderBuilder;
        _getShippingAddressesHandler = getShippingAddressesHandler;
        _createAddressHandler = createAddressHandler;
        _createOrderHandler = createOrderHandler;

        CustomerName = orderBuilder.CustomerName;

        Logger.LogInformation("[WORKFLOW_STEP3] ViewModel created - reviewing order with {Count} items",
            orderBuilder.OrderItems.Count);
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
                return;
            }

            var addresses = await _getShippingAddressesHandler.HandleAsync(
                new GetShippingAddressesQuery(_orderBuilder.CustomerId.Value)
            );

            ShippingAddresses.Clear();
            foreach (var address in addresses)
            {
                ShippingAddresses.Add(address);
            }

            if (ShippingAddresses.Any())
            {
                SelectedShippingAddress = ShippingAddresses.First();
            }

            Logger.LogInformation("[WORKFLOW_STEP3] Loaded {Count} shipping addresses", addresses.Count);
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
        await _contentManager.NavigateBackAsync();
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

            int? shippingAddressId = null;

            if (IsCreatingNewAddress)
            {
                if (string.IsNullOrWhiteSpace(NewStreet) ||
                    string.IsNullOrWhiteSpace(NewCity) ||
                    string.IsNullOrWhiteSpace(NewZipCode) ||
                    string.IsNullOrWhiteSpace(NewCountry))
                {
                    Logger.LogWarning("[WORKFLOW_STEP3] Cannot create order - incomplete address");
                    return;
                }

                await _createAddressHandler.HandleAsync(new CreateAddressCommand(
                    _orderBuilder.CustomerId!.Value,
                    NewStreet,
                    NewCity,
                    NewZipCode,
                    NewCountry,
                    AddressType.Shipping
                ));

                await LoadShippingAddressesAsync();
                shippingAddressId = ShippingAddresses.Last().Id;

                Logger.LogInformation("[WORKFLOW_STEP3] Created new shipping address");
            }
            else if (SelectedShippingAddress != null)
            {
                shippingAddressId = SelectedShippingAddress.Id;
            }

            // Create order from SHARED service data
            var orderItems = _orderBuilder.OrderItems.Select(i => new DemoOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList();

            await _createOrderHandler.HandleAsync(new CreateDemoOrderCommand(
                _orderBuilder.CustomerId!.Value,
                shippingAddressId,
                orderItems
            ));

            Logger.LogInformation("[WORKFLOW_STEP3] ✅ Order created! {ItemCount} items, Total: {Total:C}",
                OrderItems.Count, Total);

            // Request shell close via ContentManager
            _contentManager.RequestShellClose();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[WORKFLOW_STEP3] Error creating order");
        }
        finally
        {
            IsBusy = false;
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

