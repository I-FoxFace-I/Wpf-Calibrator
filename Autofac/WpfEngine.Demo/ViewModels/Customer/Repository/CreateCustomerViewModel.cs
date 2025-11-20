using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Repositories;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;

namespace WpfEngine.Demo.ViewModels.Customer.Repository;

/// <summary>
/// Create Customer ViewModel using Repository pattern with Fluent API
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Demo project")]
public partial class CreateCustomerViewModel : DialogViewModel
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly IDialogService _dialogService;
    private bool _disposed;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _companyName = string.Empty;
    [ObservableProperty] private string _taxId = string.Empty;
    [ObservableProperty] private CustomerType _type = CustomerType.Individual;
    [ObservableProperty] private ObservableCollection<DemoAddress> _addresses = new();
    [ObservableProperty] private DemoAddress? _selectedAddress;

    public CreateCustomerViewModel(
        IScopeManager scopeManager,
        IWindowContext windowContext,
        IDialogHost dialogHost,
        IDialogService dialogService,
        ILogger<CreateCustomerViewModel> logger) : base(logger, dialogHost)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Logger.LogInformation("[DEMO_V2] CreateCustomerViewModel created");
    }

    private static Random s_random = new Random();

    public static string RandomTaxId(int length)
    {
        const string chars = "0123456789";
        return $"CZ{string.Join("", Enumerable.Repeat(chars, length).Select(s => s[s_random.Next(s.Length - 1)]))}";
    }

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Initializing...";
            
            Name = "John Doe";
            Email = "john.doe@gmail.com";
            CompanyName = string.Empty;
            TaxId = RandomTaxId(8);
            Type = CustomerType.Regular;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error initializing");
            await _dialogService.ShowErrorAsync($"Error initializing: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrEmpty(Name) &&
               !string.IsNullOrEmpty(Email) &&
               !string.IsNullOrEmpty(Phone) &&
               !string.IsNullOrEmpty(TaxId);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;

            // Use Fluent API to create customer with addresses
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoCustomer>>()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteAsync(async (customerRepo, addressRepo) =>
                {
                    var customer = new DemoCustomer
                    {
                        Name = Name,
                        Email = Email,
                        Phone = Phone,
                        CompanyName = CompanyName,
                        TaxId = TaxId,
                        Type = Type
                    };

                    await customerRepo.AddAsync(customer);
                    Logger.LogInformation("[DEMO_V2] Created customer {CustomerId}", customer.Id);

                    // Add addresses if any
                    foreach (var address in Addresses)
                    {
                        address.CustomerId = customer.Id;
                        await addressRepo.AddAsync(address);
                    }

                    Logger.LogInformation("[DEMO_V2] Created customer with {Count} addresses", Addresses.Count);
                });

            await _dialogService.ShowMessageAsync("Customer created successfully");
            _windowContext.CloseWindow();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error creating customer");
            await _dialogService.ShowErrorAsync($"Error creating customer: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO_V2] Cancelling customer creation");
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
        try
        {
            Logger.LogInformation("[DEMO_V2] Opening CreateAddress dialog");

            var dialogResult = await _windowContext.ShowDialogAsync<CreateAddressDialogViewModel, CreateAddressDialogParams, CreateAddressDialogResult>(
                new CreateAddressDialogParams
                {
                    CustomerId = 0,
                    CustomerName = Name
                });

            if (dialogResult == null || dialogResult?.Result == null)
            {
                Logger.LogInformation("[DEMO_V2] Address creation cancelled");
                return;
            }

            var result = dialogResult.Result;

            Addresses.Add(new DemoAddress
            {
                Street = result.Street,
                City = result.City,
                ZipCode = result.PostalCode,
                Country = result.Country,
                Type = result.AddressType
            });

            Logger.LogInformation("[DEMO_V2] Address added to collection: {Street}, {City}",
                result.Street, result.City);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Error creating address");
            await _dialogService.ShowErrorAsync($"Error creating address: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Close()
    {
        Logger.LogInformation("[DEMO_V2] Closing window");
        _windowContext.CloseWindow();
    }

    public void Dispose()
    {
        if (_disposed) return;
        Logger.LogInformation("[DEMO_V2] CreateCustomerViewModel disposed");
        _disposed = true;
    }

    protected override async Task CompleteDialogAsync()
    {
        try
        {
            // Use Fluent API to create customer with addresses
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoCustomer>>()
                .WithService<IRepository<DemoAddress>>()
                .ExecuteAsync(async (customerRepo, addressRepo) =>
                {
                    var customer = new DemoCustomer
                    {
                        Name = Name,
                        Email = Email,
                        Phone = Phone,
                        CompanyName = CompanyName,
                        TaxId = TaxId,
                        Type = Type
                    };

                    await customerRepo.AddAsync(customer);
                    await customerRepo.SaveChangesAsync();
                    Logger.LogInformation("[DEMO_V2] Created customer {CustomerId}", customer.Id);

                    // Add addresses if any
                    foreach (var address in Addresses)
                    {
                        address.CustomerId = customer.Id;
                        await addressRepo.AddAsync(address);
                    }
                    await addressRepo.SaveChangesAsync();
                    Logger.LogInformation("[DEMO_V2] Created customer with {Count} addresses", Addresses.Count);
                });

            await _dialogService.ShowMessageAsync("Customer created successfully");

            base.OnComplete();
        }
        catch (Exception ex)
        {

            base.OnCancel();

            Logger.LogError(ex.Message, "[CUSTOMER_DETAIL] ViewModel error when creating address");

            await _dialogService.ShowErrorAsync($"Error creating address: {ex.Message}");

        }

        Logger.LogInformation("[CUSTOMER_DETAIL] Closing dialog ViewModel as Complete.");

        CloseDialogWindow(null);


    }
    protected override async Task CancelDialogAsync()
    {
        await Task.CompletedTask;

        Logger.LogInformation("[CUSTOMER_DETAIL] Closing dialog ViewModel as Cancelled.");

        OnCancel();
        CloseDialogWindow(null);
    }
}