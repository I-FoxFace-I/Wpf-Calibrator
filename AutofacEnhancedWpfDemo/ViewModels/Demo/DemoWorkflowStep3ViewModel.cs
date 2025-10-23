using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Demo.Addresses;
using AutofacEnhancedWpfDemo.Application.Demo.Orders;
using AutofacEnhancedWpfDemo.Models.Demo;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

/// <summary>
/// Step 3: Review order and select/create shipping address
/// </summary>
public partial class DemoWorkflowStep3ViewModel : BaseViewModel, IAsyncInitializable
{
    private readonly INavigator _navigator;
    private readonly WorkflowState _state;
    private readonly IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> _getShippingAddressesHandler;
    private readonly ICommandHandler<CreateAddressCommand> _createAddressHandler;
    private readonly ICommandHandler<CreateDemoOrderCommand> _createOrderHandler;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<WorkflowOrderItem> _orderItems = new();

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

    public decimal Subtotal => OrderItems.Sum(i => i.Total);
    public decimal Tax => Subtotal * 0.21m;
    public decimal Total => Subtotal + Tax;

    public DemoWorkflowStep3ViewModel(
        INavigator navigator,
        ILogger<DemoWorkflowStep3ViewModel> logger,
        WorkflowState state,
        IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> getShippingAddressesHandler,
        ICommandHandler<CreateAddressCommand> createAddressHandler,
        ICommandHandler<CreateDemoOrderCommand> createOrderHandler) : base(logger)
    {
        _navigator = navigator;
        _state = state;
        _getShippingAddressesHandler = getShippingAddressesHandler;
        _createAddressHandler = createAddressHandler;
        _createOrderHandler = createOrderHandler;

        CustomerName = state.CustomerName;

        if (state.OrderItems != null)
        {
            foreach (var item in state.OrderItems)
            {
                OrderItems.Add(item);
            }
        }

        Logger.LogInformation("[WORKFLOW] Step3 ViewModel created - reviewing order");
    }

    public async Task InitializeAsync()
    {
        await LoadShippingAddressesAsync();
    }

    private async Task LoadShippingAddressesAsync()
    {
        try
        {
            IsBusy = true;

            var addresses = await _getShippingAddressesHandler.HandleAsync(
                new GetShippingAddressesQuery(_state.CustomerId)
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

            Logger.LogInformation("[WORKFLOW] Loaded {Count} shipping addresses", addresses.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        Logger.LogInformation("[WORKFLOW] Going back to Step 2");
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

            int? shippingAddressId = null;

            if (IsCreatingNewAddress)
            {
                if (string.IsNullOrWhiteSpace(NewStreet) ||
                    string.IsNullOrWhiteSpace(NewCity) ||
                    string.IsNullOrWhiteSpace(NewZipCode) ||
                    string.IsNullOrWhiteSpace(NewCountry))
                {
                    Logger.LogWarning("[WORKFLOW] Cannot create order - incomplete address");
                    return;
                }

                await _createAddressHandler.HandleAsync(new CreateAddressCommand(
                    _state.CustomerId,
                    NewStreet,
                    NewCity,
                    NewZipCode,
                    NewCountry,
                    AddressType.Shipping
                ));

                await LoadShippingAddressesAsync();
                shippingAddressId = ShippingAddresses.Last().Id;

                Logger.LogInformation("[WORKFLOW] Created new shipping address");
            }
            else if (SelectedShippingAddress != null)
            {
                shippingAddressId = SelectedShippingAddress.Id;
            }

            var orderItems = OrderItems.Select(i => new DemoOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList();

            await _createOrderHandler.HandleAsync(new CreateDemoOrderCommand(
                _state.CustomerId,
                shippingAddressId,
                orderItems
            ));

            Logger.LogInformation("[WORKFLOW] ✅ Order created successfully! {ItemCount} items, Total: {Total:C}",
                OrderItems.Count, Total);

            //var shouldClose = await _navigator.CloseWindowAsync(
            //    showConfirmation: true,
            //    confirmationMessage: $"Order created successfully!\n\nTotal: {Total:C}\n\nClose workflow window?"
            //);

            _navigator.RequestWindowClose();

            //if (!shouldClose)
            //{
            //    Logger.LogInformation("[WORKFLOW] User chose to keep window open");
            //}
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[WORKFLOW] Error creating order");
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

    partial void OnIsCreatingNewAddressChanged(bool value)
    {
        CompleteCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedShippingAddressChanged(DemoAddress? value)
    {
        CompleteCommand.NotifyCanExecuteChanged();
    }

    partial void OnNewStreetChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnNewCityChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnNewZipCodeChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
    partial void OnNewCountryChanged(string value) => CompleteCommand.NotifyCanExecuteChanged();
}