using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Abstract;
using WpfEngine.Services;
using WpfEngine.Data.Windows.Events;

namespace WpfEngine.Demo.ViewModels;

public partial class CustomerListViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllHandler;
    private readonly ICommandHandler<DeleteDemoCustomerCommand> _deleteHandler;
    private readonly IWindowContext _windowContext;
    private readonly Dictionary<int, Guid> _openDetailWindows = new();
    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();

    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;

    public CustomerListViewModel(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllHandler,
        ICommandHandler<DeleteDemoCustomerCommand> deleteHandler,
        IWindowContext windowContext,
        ILogger<CustomerListViewModel> logger) : base(logger)
    {
        _getAllHandler = getAllHandler ?? throw new ArgumentNullException(nameof(getAllHandler));
        _deleteHandler = deleteHandler ?? throw new ArgumentNullException(nameof(deleteHandler));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        
        _windowContext.ChildClosed += OnChildWindowClosed;
        Logger.LogInformation("[DEMO] CustomerListViewModel created");
    }

    public async Task InitializeAsync(CancellationToken cancelationToken = default)
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
            var customers = await _getAllHandler.HandleAsync(new GetAllDemoCustomersQuery());
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
            Logger.LogInformation("[DEMO] Loaded {Count} customers", Customers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to load customers");
            SetError("Failed to load customers: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanViewDetail))]
    private void ViewDetail()
    {
        if (SelectedCustomer == null) return;
        var customerId = SelectedCustomer.Id;

        if (_openDetailWindows.ContainsKey(customerId))
        {
            Logger.LogInformation("[DEMO] Customer {CustomerId} detail already open", customerId);
            return;
        }

        Logger.LogInformation("[DEMO] Opening detail for customer {CustomerId}", customerId);
        var windowId = _windowContext.OpenWindow<CustomerDetailViewModel, CustomerDetailParameters>(
            new CustomerDetailParameters { CustomerId = customerId }
        );
        _openDetailWindows[customerId] = windowId;
    }

    private bool CanViewDetail() => SelectedCustomer != null;

    [RelayCommand(CanExecute = nameof(CanDeleteCustomer))]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;
        try
        {
            IsBusy = true;
            ClearError();
            var customerId = SelectedCustomer.Id;
            Logger.LogInformation("[DEMO] Deleting customer {CustomerId}", customerId);
            await _deleteHandler.HandleAsync(new DeleteDemoCustomerCommand(customerId));
            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO] Failed to delete customer");
            SetError("Failed to delete customer: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDeleteCustomer() => SelectedCustomer != null;

    [RelayCommand]
    private void CloseWindow()
    {
        Logger.LogInformation("[DEMO] Closing customer list");
        _windowContext.CloseWindow();
    }

    partial void OnSelectedCustomerChanged(DemoCustomer? value)
    {
        ViewDetailCommand.NotifyCanExecuteChanged();
        DeleteCustomerCommand.NotifyCanExecuteChanged();
    }

    private async void OnChildWindowClosed(object? sender, ChildWindowClosedEventArgs? e)
    {
        if(e != null && e.ViewModelType is not null)
        {
            if (e?.ViewModelType is Type viewModelType)
            {
                if (viewModelType == typeof(CustomerDetailViewModel))
                {
                    Logger.LogInformation("[DEMO] Detail closed, refreshing list");
                    var customerId = _openDetailWindows.FirstOrDefault(kvp => kvp.Value == e.ChildWindowId).Key;
                    if (customerId != 0)
                    {
                        _openDetailWindows.Remove(customerId);
                    }
                    await LoadCustomersAsync();
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _windowContext.ChildClosed -= OnChildWindowClosed;
        _windowContext.CloseAllChildWindows();
        _openDetailWindows.Clear();
        _disposed = true;
    }
}
