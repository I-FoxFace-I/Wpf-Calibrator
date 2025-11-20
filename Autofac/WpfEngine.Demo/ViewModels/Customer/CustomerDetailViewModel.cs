using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Abstract;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Addresses;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Customer Detail ViewModel - REFACTORED to use DialogService
/// Demonstrates:
/// - Opening modal dialogs
/// - Getting results from dialogs
/// - Closing window via WindowContext
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Demo project")]
public partial class CustomerDetailViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly int _customerId;
    private readonly IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> _getCustomerHandler;
    private readonly IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> _getAddressesHandler;
    private readonly ICommandHandler<UpdateDemoCustomerCommand> _updateHandler;
    private readonly ICommandHandler<CreateAddressCommand> _createAddressHandler;
    private readonly ICommandHandler<DeleteAddressCommand> _deleteAddressHandler;
    private readonly IWindowContext _windowContext;
    private readonly IDialogService _dialogService; // NEW: Dialog service

    [ObservableProperty] private DemoCustomer? _customer;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _taxId = string.Empty;
    [ObservableProperty] private CustomerType _type = CustomerType.Individual;
    [ObservableProperty] private ObservableCollection<DemoAddress> _addresses = new();

    [ObservableProperty]
    private DemoAddress? _selectedAddress;

    private bool _disposed;

    public CustomerDetailViewModel(
        IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> getCustomerHandler,
        IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> getAddressesHandler,
        ICommandHandler<CreateAddressCommand> createAddressHandler,
        ICommandHandler<DeleteAddressCommand> deleteAddressHandler,
        ICommandHandler<UpdateDemoCustomerCommand> updateHandler,
        IWindowContext windowContext,
        IDialogService dialogService, // NEW: Inject dialog service
        ILogger<CustomerDetailViewModel> logger,
        CustomerDetailParameters parameters) : base(logger)
    {
        _getCustomerHandler = getCustomerHandler;
        _getAddressesHandler = getAddressesHandler;
        _updateHandler = updateHandler;
        _createAddressHandler = createAddressHandler;
        _deleteAddressHandler = deleteAddressHandler;
        _windowContext = windowContext;
        _dialogService = dialogService; // NEW
        _customerId = parameters.CustomerId;

        Logger.LogInformation("[CUSTOMER_DETAIL] ViewModel created");
    }

    public async Task InitializeAsync(CancellationToken cancelationToken = default)
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading customer...";

            // Load customer
            var customer = await _getCustomerHandler.HandleAsync(
                new GetDemoCustomerByIdQuery(_customerId));

            if (customer == null)
            {
                Logger.LogWarning("[CUSTOMER_DETAIL] Customer {CustomerId} not found", _customerId);
                await _dialogService.ShowErrorAsync($"Customer {_customerId} not found");
                _windowContext.CloseWindow();
                return;
            }
            Customer = customer;
            Name = customer.Name;
            Email = customer.Email;
            Phone = customer.Phone;
            CompanyName = customer.CompanyName ?? string.Empty;
            TaxId = customer.TaxId ?? string.Empty;
            Type = customer.Type;
            Addresses.Clear();
            // Load addresses
            await LoadAddressesAsync();

            Logger.LogInformation("[CUSTOMER_DETAIL] Loaded customer {CustomerId}", Customer.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[CUSTOMER_DETAIL] Error loading customer");
            await _dialogService.ShowErrorAsync($"Error loading customer: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (Customer == null) return;
        try
        {
            IsBusy = true;

            ClearError();

            await _updateHandler.HandleAsync(new UpdateDemoCustomerCommand(
                _customerId, Name, Email, Phone, CompanyName, TaxId, Type
            ));

            Logger.LogInformation("[DEMO] Customer {CustomerId} updated", _customerId);

            _windowContext.CloseWindow();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to save customer");
            SetError("Failed to save: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave()
    {
        if (Customer == null)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(Name))
        {
            return false;
        }


        var result = false;

        result |= Customer?.Name != Name;
        result |= Customer?.Email != Email;
        result |= Customer?.Phone != Phone;
        result |= Customer?.CompanyName != CompanyName;
        result |= Customer?.TaxId != TaxId;
        result |= Customer?.Type != Type;

        return result;
    }

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO] Cancelling customer edit");
        _windowContext.CloseWindow();
    }

    partial void OnNameChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnCompanyNameChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnPhoneChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnTaxIdChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnEmailChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnTypeChanged(CustomerType value) { SaveCommand.NotifyCanExecuteChanged(); }

    /// <summary>
    /// Opens dialog to create new address - NEW with DialogService
    /// </summary>
    [RelayCommand]
    private async Task CreateAddressAsync()
    {
        if (Customer == null) return;

        try
        {
            Logger.LogInformation("[CUSTOMER_DETAIL] Opening CreateAddress dialog");

            // Open dialog with parameters and get result
            var dialogResult = await _windowContext.ShowDialogAsync<CreateAddressDialogViewModel, CreateAddressDialogParams, CreateAddressDialogResult>(
                    new CreateAddressDialogParams
                    {
                        CustomerId = Customer.Id,
                        CustomerName = Customer.Name
                    });

            // Check if user confirmed (result not null)
            if (dialogResult == null || dialogResult?.Result == null)
            {
                Logger.LogInformation("[CUSTOMER_DETAIL] Address creation cancelled");
                return;
            }
            var result = dialogResult.Result;

            // Create address via command handler
            IsBusy = true;
            BusyMessage = "Creating address...";

            await _createAddressHandler.HandleAsync(new CreateAddressCommand(
                Customer.Id, result.Street, result.City, result.PostalCode, result.Country, result.AddressType
            ));

            Logger.LogInformation("[CUSTOMER_DETAIL] Address created: {Street}, {City}",
                result.Street, result.City);

            // Reload addresses
            await LoadAddressesAsync();

            // Show success message
            await _dialogService.ShowMessageAsync("Address created successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[CUSTOMER_DETAIL] Error creating address");
            await _dialogService.ShowErrorAsync($"Error creating address: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    /// <summary>
    /// Deletes selected address with confirmation - NEW with DialogService
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteAddress))]
    private async Task DeleteAddressAsync()
    {
        if (SelectedAddress == null) return;

        try
        {
            // Show confirmation dialog
            var confirmed = await _dialogService.ShowConfirmationAsync(
                $"Are you sure you want to delete address:\n{SelectedAddress.Street}, {SelectedAddress.City}?",
                "Confirm Delete");

            if (!confirmed)
            {
                Logger.LogInformation("[CUSTOMER_DETAIL] Address deletion cancelled");
                return;
            }

            // Delete address
            IsBusy = true;
            BusyMessage = "Deleting address...";

            await _deleteAddressHandler.HandleAsync(new DeleteAddressCommand(SelectedAddress.Id));

            Logger.LogInformation("[CUSTOMER_DETAIL] Address deleted: {AddressId}", SelectedAddress.Id);

            // Reload addresses
            await LoadAddressesAsync();

            await _dialogService.ShowMessageAsync("Address deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            // Specific handling for business logic errors (e.g., address used in orders)
            Logger.LogWarning(ex, "[CUSTOMER_DETAIL] Cannot delete address: {Message}", ex.Message);
            await _dialogService.ShowErrorAsync(ex.Message, "Cannot Delete Address");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[CUSTOMER_DETAIL] Error deleting address");
            await _dialogService.ShowErrorAsync($"Error deleting address: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    private bool CanDeleteAddress() => SelectedAddress != null;

    /// <summary>
    /// Closes the window - uses WindowContext
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        Logger.LogInformation("[CUSTOMER_DETAIL] Closing window");
        _windowContext.CloseWindow();
    }

    private async Task LoadAddressesAsync()
    {
        if (Customer == null) return;

        var addresses = await _getAddressesHandler.HandleAsync(
            new GetShippingAddressesQuery (Customer.Id ));

        Addresses.Clear();
        foreach (var address in addresses)
        {
            Addresses.Add(address);
        }

        Logger.LogInformation("[CUSTOMER_DETAIL] Loaded {Count} addresses", Addresses.Count);
    }

    partial void OnSelectedAddressChanged(DemoAddress? value)
    {
        DeleteAddressCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[CUSTOMER_DETAIL] ViewModel disposed");
        _disposed = true;
    }
}

