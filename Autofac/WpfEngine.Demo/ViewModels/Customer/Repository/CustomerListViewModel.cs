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
using WpfEngine.Demo.ViewModels.Parameters.Repository;
using WpfEngine.Demo.ViewModels.Workflow.Repository;
using WpfEngine.Extensions;
using WpfEngine.Services;
using WpfEngine.ViewModels.Dialogs;
using WpfEngine.ViewModels.Managed;
using WpfEngine.ViewModels;
using WpfEngine.Views.Windows;

namespace WpfEngine.Demo.ViewModels.Customer.Repository;

/// <summary>
/// Customer List ViewModel using Repository + Unit of Work pattern
/// Demonstrates database session usage
/// </summary>
public partial class CustomerListViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IScopeManager _scopeManager;
    private readonly IWindowContext _windowContext;
    private readonly Dictionary<int, Guid> _openDetailWindows = new();
    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();

    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;

    public CustomerListViewModel(
        IScopeManager scopeManager,
        IWindowContext windowContext,
        ILogger<CustomerListViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _windowContext = windowContext ?? throw new ArgumentNullException(nameof(windowContext));
        
        _windowContext.ChildClosed += OnChildWindowClosed;
        Logger.LogInformation("[DEMO_V2] CustomerListViewModel created");
    }

    public override async Task InitializeAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadCustomersAsync(CancellationToken cancellationToken=default)
    {
        try
        {
            IsBusy = true;
            ClearError();

            // Use Fluent API with database session - auto-save enabled by default
            var customers = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoCustomer>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.GetAllAsync(cancellationToken);
                });

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }

            Logger.LogInformation("[DEMO_V2] Loaded {Count} customers", Customers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to load customers");
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
            Logger.LogInformation("[DEMO_V2] Customer {CustomerId} detail already open", customerId);
            return;
        }

        Logger.LogInformation("[DEMO_V2] Opening detail for customer {CustomerId}", customerId);
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
            Logger.LogInformation("[DEMO_V2] Deleting customer {CustomerId}", customerId);

            // Use Fluent API with database session - auto-save enabled by default
            await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoCustomer>>()
                .ExecuteAsync(async (repo) =>
                {
                    var customer = await repo.GetByIdAsync(customerId);
                    if (customer != null)
                    {
                        await repo.DeleteAsync(customer);
                        Logger.LogInformation("[DEMO_V2] Customer {CustomerId} deleted", customerId);
                    }
                });

            await LoadCustomersAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DEMO_V2] Failed to delete customer");
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
        Logger.LogInformation("[DEMO_V2] Closing customer list");
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
                    Logger.LogInformation("[DEMO_V2] Detail closed, refreshing list");
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