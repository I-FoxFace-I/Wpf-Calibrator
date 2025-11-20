using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Abstract;
using WpfEngine.ViewModels;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Addresses;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.ViewModels.Dialogs;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Customer Detail ViewModel - REFACTORED to use DialogService
/// Demonstrates:
/// - Opening modal dialogs
/// - Getting results from dialogs
/// - Closing window via WindowContext
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Demo project")]
public partial class CreateCustomerViewModel : DialogViewModel, IInitializable, IDisposable
{
    private readonly int _customerId;
    private readonly IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> _getCustomerHandler;
    private readonly IQueryHandler<GetShippingAddressesQuery, List<DemoAddress>> _getAddressesHandler;
    private readonly ICommandHandler<CreateDemoCustomerCommand> _createHandler;
    private readonly ICommandHandler<CreateAddressCommand> _createAddressHandler;
    private readonly ICommandHandler<DeleteAddressCommand> _deleteAddressHandler;
    private readonly IWindowContext _windowContext;
    private readonly IDialogService _dialogService; // NEW: Dialog service

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

    public CreateCustomerViewModel(
        IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> getCustomerHandler,
        ICommandHandler<CreateDemoCustomerCommand> createHandler,
        IDialogHost dialogHost,
        IWindowContext windowContext,
        IDialogService dialogService, // NEW: Inject dialog service
        ILogger<CreateCustomerViewModel> logger) : base(logger, dialogHost)
    {
        _getCustomerHandler = getCustomerHandler;
        _createHandler = createHandler;
        _windowContext = windowContext;
        _dialogService = dialogService; // NEW

        Logger.LogInformation("[CUSTOMER_DETAIL] ViewModel created");
    }

    private static Random s_random = new Random();

    public static string RandomTaxId(int length)
    {
        const string chars = "0123456789";
        return $"CZ{string.Join("", Enumerable.Repeat(chars, length).Select(s => s[s_random.Next(s.Length - 1)]))}";
    }

    public override async Task InitializeAsync() => await InitializeAsync(CancellationToken.None);

    public async Task InitializeAsync(CancellationToken cancelationToken=default)
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading customer...";
            Name = "John Doe";
            Email = "john.doe@gmail.com";
            CompanyName = string.Empty;
            TaxId = RandomTaxId(8);
            Type = CustomerType.Regular;

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

    protected override bool CanCompleteDialog()
    {
        return CanSave();
    }

    private bool CanSave()
    {
        var result = true;

        result &= !string.IsNullOrEmpty(Name);
        result &= !string.IsNullOrEmpty(Email);
        result &= !string.IsNullOrEmpty(Phone);
        result &= !string.IsNullOrEmpty(TaxId);

        return result;
    }

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO] Cancelling customer edit");
        _windowContext.CloseWindow();
    }

    partial void OnNameChanged(string value) { CompleteDialogCommand.NotifyCanExecuteChanged(); }
    partial void OnCompanyNameChanged(string value) { CompleteDialogCommand.NotifyCanExecuteChanged(); }
    partial void OnPhoneChanged(string value) { CompleteDialogCommand.NotifyCanExecuteChanged(); }
    partial void OnTaxIdChanged(string value) { CompleteDialogCommand.NotifyCanExecuteChanged(); }
    partial void OnEmailChanged(string value) { CompleteDialogCommand.NotifyCanExecuteChanged(); }
    partial void OnTypeChanged(CustomerType value) { CompleteDialogCommand.NotifyCanExecuteChanged(); }

    /// <summary>
    /// Opens dialog to create new address - NEW with DialogService
    /// </summary>
    [RelayCommand]
    private async Task CreateAddressAsync()
    {
        try
        {
            Logger.LogInformation("[CUSTOMER_DETAIL] Opening CreateAddress dialog");

            // Open dialog with parameters and get result
            var dialogResult = await _windowContext.ShowDialogAsync<CreateAddressDialogViewModel, CreateAddressDialogParams, CreateAddressDialogResult>(
                    new CreateAddressDialogParams
                    {
                        CustomerId = 0,
                        CustomerName = Name
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

            Addresses.Add(new DemoAddress
            {
                Street = result.Street,
                City = result.City,
                ZipCode = result.PostalCode,
                Country = result.Country,
                Type = result.AddressType
            });


            Logger.LogInformation("[CUSTOMER_DETAIL] Address created: {Street}, {City}",
                result.Street, result.City);
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
    /// Closes the window - uses WindowContext
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        Logger.LogInformation("[CUSTOMER_DETAIL] Closing window");
        _windowContext.CloseWindow();
    }

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[CUSTOMER_DETAIL] ViewModel disposed");
        _disposed = true;
    }


    protected override async Task CompleteDialogAsync()
    {
        try
        {
            await _createHandler.HandleAsync(new CreateDemoCustomerCommand(Name, Email, Phone, CompanyName, TaxId, Type));

            base.OnComplete();
        }
        catch(Exception ex)
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

