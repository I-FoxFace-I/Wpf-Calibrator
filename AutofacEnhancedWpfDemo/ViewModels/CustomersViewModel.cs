using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Customers;
using AutofacEnhancedWpfDemo.Models;
using AutofacEnhancedWpfDemo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Customers management window
/// </summary>
public partial class CustomersViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetAllCustomersQuery, List<Customer>> _getAllCustomersHandler;
    private readonly ICommandHandler<DeleteCustomerCommand> _deleteCustomerHandler;
    private readonly IWindowNavigator _navigator;

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    public CustomersViewModel(
        IQueryHandler<GetAllCustomersQuery, List<Customer>> getAllCustomersHandler,
        ICommandHandler<DeleteCustomerCommand> deleteCustomerHandler,
        IWindowNavigator navigator,
        ILogger<CustomersViewModel> logger) : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _deleteCustomerHandler = deleteCustomerHandler;
        _navigator = navigator;
    }

    public async Task InitializeAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            Logger.LogInformation("Loading customers");
            var customers = await _getAllCustomersHandler.HandleAsync(new GetAllCustomersQuery());

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }

            Logger.LogInformation("Loaded {Count} customers", Customers.Count);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load customers: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task EditCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        Logger.LogInformation("Opening edit dialog for customer {CustomerId}", SelectedCustomer.Id);

        var result = await _navigator.ShowDialogAsync<EditCustomerViewModel, EditCustomerResult>(
            new EditCustomerParams { CustomerId = SelectedCustomer.Id }
        );

        if (result?.Success == true)
        {
            Logger.LogInformation("Customer edited successfully, refreshing list");
            await LoadCustomersAsync();
        }
    }

    private bool CanEdit() => SelectedCustomer != null && !IsBusy;

    [RelayCommand]
    private async Task AddCustomerAsync()
    {
        Logger.LogInformation("Opening add customer dialog");

        var result = await _navigator.ShowDialogAsync<EditCustomerViewModel, EditCustomerResult>();

        if (result?.Success == true)
        {
            Logger.LogInformation("Customer added successfully, refreshing list");
            await LoadCustomersAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        var customerName = SelectedCustomer.Name;
        Logger.LogInformation("Deleting customer {CustomerId}", SelectedCustomer.Id);

        try
        {
            IsBusy = true;
            await _deleteCustomerHandler.HandleAsync(new DeleteCustomerCommand(SelectedCustomer.Id));

            Logger.LogInformation("Customer {Name} deleted successfully", customerName);
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete customer: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDelete() => SelectedCustomer != null && !IsBusy;

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        EditCustomerCommand.NotifyCanExecuteChanged();
        DeleteCustomerCommand.NotifyCanExecuteChanged();
    }
}

// ========== DTOs ==========

public record EditCustomerParams
{
    public int? CustomerId { get; init; }
}

public record EditCustomerResult
{
    public bool Success { get; init; }
    public int? CustomerId { get; init; }
}