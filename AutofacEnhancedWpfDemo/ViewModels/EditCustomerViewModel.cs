using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Customers;
using AutofacEnhancedWpfDemo.Models;
using AutofacEnhancedWpfDemo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Customer edit/create dialog
/// </summary>
public partial class EditCustomerViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetCustomerByIdQuery, Customer?> _getCustomerHandler;
    private readonly ICommandHandler<CreateCustomerCommand> _createHandler;
    private readonly ICommandHandler<UpdateCustomerCommand> _updateHandler;
    private readonly IWindowNavigator _navigator;
    private readonly int? _customerId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _title = "Add Customer";

    public bool IsEditMode => _customerId.HasValue;

    public EditCustomerViewModel(
        IQueryHandler<GetCustomerByIdQuery, Customer?> getCustomerHandler,
        ICommandHandler<CreateCustomerCommand> createHandler,
        ICommandHandler<UpdateCustomerCommand> updateHandler,
        IWindowNavigator navigator,
        ILogger<EditCustomerViewModel> logger,
        EditCustomerParams? parameters = null) : base(logger)
    {
        _getCustomerHandler = getCustomerHandler;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _navigator = navigator;
        _customerId = parameters?.CustomerId;

        if (IsEditMode)
        {
            Title = "Edit Customer";
        }
    }

    public async Task InitializeAsync()
    {
        if (!IsEditMode) return;

        try
        {
            IsBusy = true;
            Logger.LogInformation("Loading customer {CustomerId}", _customerId);

            var customer = await _getCustomerHandler.HandleAsync(new GetCustomerByIdQuery(_customerId!.Value));

            if (customer != null)
            {
                Name = customer.Name;
                Email = customer.Email;
            }
            else
            {
                SetError("Customer not found");
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load customer: {ex.Message}");
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

            if (IsEditMode)
            {
                Logger.LogInformation("Updating customer {CustomerId}", _customerId);
                await _updateHandler.HandleAsync(new UpdateCustomerCommand(
                    _customerId!.Value, Name, Email
                ));
            }
            else
            {
                Logger.LogInformation("Creating new customer");
                await _createHandler.HandleAsync(new CreateCustomerCommand(Name, Email));
            }

            _navigator.CloseDialog<EditCustomerViewModel>(new EditCustomerResult
            {
                Success = true,
                CustomerId = _customerId
            });
        }
        catch (Exception ex)
        {
            SetError($"Failed to save customer: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !IsBusy;

    [RelayCommand]
    private void Cancel()
    {
        Logger.LogInformation("Edit cancelled");
        _navigator.CloseDialog<EditCustomerViewModel>(new EditCustomerResult { Success = false });
    }

    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
    partial void OnEmailChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}