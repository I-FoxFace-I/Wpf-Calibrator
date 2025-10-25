using System.Collections.ObjectModel;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Services.WindowTracking;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Customer Detail ViewModel - can be used as modal dialog or non-modal window
/// Closes itself via WindowService
/// </summary>
public partial class DemoCustomerDetailViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> _getCustomerHandler;
    private readonly ICommandHandler<UpdateDemoCustomerCommand> _updateHandler;
    private readonly IWindowService _windowService;
    private readonly int _customerId;

    [ObservableProperty]
    private DemoCustomer? _customer;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private string _taxId = string.Empty;

    [ObservableProperty]
    private CustomerType _type = CustomerType.Individual;

    [ObservableProperty]
    private ObservableCollection<DemoAddress> _addresses = new();

    public DemoCustomerDetailViewModel(
        IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> getCustomerHandler,
        ICommandHandler<UpdateDemoCustomerCommand> updateHandler,
        IWindowService WindowService,
        ILogger<DemoCustomerDetailViewModel> logger,
        DemoCustomerDetailParams parameters) : base(logger)
    {
        _getCustomerHandler = getCustomerHandler;
        _updateHandler = updateHandler;
        _windowService = WindowService;
        _customerId = parameters.CustomerId;

        Logger.LogInformation("[DEMO] CustomerDetailViewModel created for customer {CustomerId}", _customerId);
    }

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var customer = await _getCustomerHandler.HandleAsync(new GetDemoCustomerByIdQuery(_customerId));

            if (customer != null)
            {
                Customer = customer;
                Name = customer.Name;
                Email = customer.Email;
                Phone = customer.Phone;
                CompanyName = customer.CompanyName;
                TaxId = customer.TaxId;
                Type = customer.Type;

                Addresses.Clear();
                foreach (var address in customer.Addresses)
                {
                    Addresses.Add(address);
                }

                Logger.LogInformation("[DEMO] Loaded customer {CustomerId} details", _customerId);
            }
            else
            {
                SetError($"Customer {_customerId} not found");
                Logger.LogWarning("[DEMO] Customer {CustomerId} not found", _customerId);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load customer: {ex.Message}");
            Logger.LogError(ex, "[DEMO] Error loading customer {CustomerId}", _customerId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            await _updateHandler.HandleAsync(new UpdateDemoCustomerCommand(
                _customerId, Name, Email, Phone, CompanyName, TaxId, Type
            ));

            Logger.LogInformation("[DEMO] Customer {CustomerId} updated successfully", _customerId);

            // Close window via WindowService using VmKey
            _windowService.Close(this.GetVmKey());
        }
        catch (Exception ex)
        {
            SetError($"Failed to save: {ex.Message}");
            Logger.LogError(ex, "[DEMO] Error saving customer {CustomerId}", _customerId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name)
                            && !string.IsNullOrWhiteSpace(Email)
                            && !IsBusy;

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("[DEMO] Customer {CustomerId} edit cancelled", _customerId);

        // Close window without saving using VmKey
        _windowService.Close(this.GetVmKey());
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnEmailChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}
