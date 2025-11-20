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
using WpfEngine.Demo.ViewModels.Workflow;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;
using WpfEngine.ViewModels.Managed;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.ViewModels.Customer.Repository;

/// <summary>
/// Customer Detail ViewModel using Repository + Unit of Work pattern with Fluent API
/// Demonstrates database sessions with multiple repositories
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Demo project")]
public partial class CustomerDetailViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly int _customerId;
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly IDialogService _dialogService;
    private bool _disposed;

    [ObservableProperty] private DemoCustomer? _customer;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _taxId = string.Empty;
    [ObservableProperty] private CustomerType _type = CustomerType.Individual;
    [ObservableProperty] private ObservableCollection<DemoAddress> _addresses = new();
    [ObservableProperty] private DemoAddress? _selectedAddress;

    public CustomerDetailViewModel(
        IScopeManager scopeManager,
        IWindowContext windowContext,
        IDialogService dialogService,
        ILogger<CustomerDetailViewModel> logger,
        CustomerDetailParameters parameters) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _customerId = parameters.CustomerId;

        Logger.LogInformation("[DEMO_V2] CustomerDetailViewModel created for {CustomerId}", _customerId);
    }

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading customer...";
            ClearError();

            // Use Fluent API to load customer with addresses
            var customer = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoCustomer>>()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteWithResultAsync(async (customerRepo, addressRepo) =>
                {
                    var cust = await customerRepo.GetByIdAsync(_customerId);
                    if (cust == null) return null;

                    // Load addresses for this customer
                    var addrs = await addressRepo.FindAsync(a => a.CustomerId == _customerId);
                    cust.Addresses = addrs.ToList();
                    
                    return cust;
                });

            if (customer == null)
            {
                Logger.LogWarning("[DEMO_V2] Customer {CustomerId} not found", _customerId);
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
            foreach (var address in customer.Addresses)
            {
                Addresses.Add(address);
            }

            Logger.LogInformation("[DEMO_V2] Loaded customer {CustomerId} with {Count} addresses", 
                Customer.Id, Addresses.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error loading customer");
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

            // Use Fluent API to update customer
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoCustomer>>()
                .ExecuteAsync(async (repo) =>
                {
                    var customer = await repo.GetByIdAsync(_customerId);
                    if (customer == null) return;

                    customer.Name = Name;
                    customer.Email = Email;
                    customer.Phone = Phone;
                    customer.CompanyName = CompanyName;
                    customer.TaxId = TaxId;
                    customer.Type = Type;

                    await repo.UpdateAsync(customer);
                    Logger.LogInformation("[DEMO_V2] Customer {CustomerId} updated", _customerId);
                });

            _windowContext.CloseWindow();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to save customer");
            SetError("Failed to save: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave()
    {
        if (Customer == null || string.IsNullOrWhiteSpace(Name))
            return false;

        return Customer.Name != Name ||
               Customer.Email != Email ||
               Customer.Phone != Phone ||
               Customer.CompanyName != CompanyName ||
               Customer.TaxId != TaxId ||
               Customer.Type != Type;
    }

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO_V2] Cancelling customer edit");
        _windowContext.CloseWindow();
    }

    partial void OnNameChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnCompanyNameChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnPhoneChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnTaxIdChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnEmailChanged(string value) { SaveCommand.NotifyCanExecuteChanged(); }
    partial void OnTypeChanged(CustomerType value) { SaveCommand.NotifyCanExecuteChanged(); }

    [RelayCommand]
    private async Task CreateAddressAsync()
    {
        if (Customer == null) return;

        try
        {
            Logger.LogInformation("[DEMO_V2] Opening CreateAddress dialog");

            var dialogResult = await _windowContext.ShowDialogAsync<CreateAddressDialogViewModel, CreateAddressDialogParams, CreateAddressDialogResult>(
                new CreateAddressDialogParams
                {
                    CustomerId = Customer.Id,
                    CustomerName = Customer.Name
                });

            if (dialogResult == null || dialogResult?.Result == null)
            {
                Logger.LogInformation("[DEMO_V2] Address creation cancelled");
                return;
            }

            var result = dialogResult.Result;

            // Use Fluent API to create address
            IsBusy = true;
            BusyMessage = "Creating address...";

            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteAsync(async (repo) =>
                {
                    var address = new DemoAddress
                    {
                        CustomerId = Customer.Id,
                        Street = result.Street,
                        City = result.City,
                        ZipCode = result.PostalCode,
                        Country = result.Country,
                        Type = result.AddressType
                    };

                    await repo.AddAsync(address);
                    Logger.LogInformation("[DEMO_V2] Address created: {Street}, {City}", 
                        result.Street, result.City);
                });

            // Reload addresses
            await LoadAddressesAsync();

            await _dialogService.ShowMessageAsync("Address created successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error creating address");
            await _dialogService.ShowErrorAsync($"Error creating address: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteAddress))]
    private async Task DeleteAddressAsync()
    {
        if (SelectedAddress == null) return;

        try
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                $"Are you sure you want to delete address:\n{SelectedAddress.Street}, {SelectedAddress.City}?",
                "Confirm Delete");

            if (!confirmed)
            {
                Logger.LogInformation("[DEMO_V2] Address deletion cancelled");
                return;
            }

            IsBusy = true;
            BusyMessage = "Deleting address...";

            // Use Fluent API to delete address
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteAsync(async (repo) =>
                {
                    var address = await repo.GetByIdAsync(SelectedAddress.Id);
                    if (address != null)
                    {
                        await repo.DeleteAsync(address);
                        Logger.LogInformation("[DEMO_V2] Address deleted: {AddressId}", address.Id);
                    }
                });

            await LoadAddressesAsync();
            await _dialogService.ShowMessageAsync("Address deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "[DEMO_V2] Cannot delete address: {Message}", ex.Message);
            await _dialogService.ShowErrorAsync(ex.Message, "Cannot Delete Address");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error deleting address");
            await _dialogService.ShowErrorAsync($"Error deleting address: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    private bool CanDeleteAddress() => SelectedAddress != null;

    [RelayCommand]
    private void Close()
    {
        Logger.LogInformation("[DEMO_V2] Closing window");
        _windowContext.CloseWindow();
    }

    private async Task LoadAddressesAsync()
    {
        if (Customer == null) return;

        try
        {
            // Use Fluent API to load addresses
            var addresses = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.FindAsync(a => a.CustomerId == Customer.Id);
                });

            Addresses.Clear();
            foreach (var address in addresses)
            {
                Addresses.Add(address);
            }

            Logger.LogInformation("[DEMO_V2] Loaded {Count} addresses", Addresses.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error loading addresses");
        }
    }

    partial void OnSelectedAddressChanged(DemoAddress? value)
    {
        DeleteAddressCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        if (_disposed) return;
        Logger.LogInformation("[DEMO_V2] CustomerDetailViewModel disposed");
        _disposed = true;
    }
}