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

namespace WpfEngine.Demo.ViewModels.Workflow.Repository;

/// <summary>
/// Workflow Step 1: Customer Selection
/// Uses Repository pattern with Fluent API for data access
/// </summary>
public partial class WorkflowStep1ViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IScopeManager _scopeManager;
    private readonly INavigator _navigator;
    private readonly IWindowContext _windowContext;
    private readonly IOrderBuilderService _orderBuilder;
    
    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();
    
    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;
    
    private bool _disposed;
    
    public WorkflowStep1ViewModel(
        IScopeManager scopeManager,
        INavigator navigator,
        IOrderBuilderService orderBuilder,
        IWindowContext windowContext,
        ILogger<WorkflowStep1ViewModel> logger) : base(logger)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _navigator = navigator;
        _orderBuilder = orderBuilder;
        _windowContext = windowContext;
        Logger.LogInformation("[WORKFLOW_STEP1] ViewModel created");
    }

    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            
            // Use Fluent API to load customers
            var customers = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoCustomer>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.GetAllAsync();
                });

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }

            Logger.LogInformation("[WORKFLOW_STEP1] Loaded {Count} customers", Customers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[WORKFLOW_STEP1] Error loading customers");
            SetError("Failed to load customers: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateCustomer()
    {
        var result = await _windowContext.ShowDialogAsync<Customer.Repository.CreateCustomerViewModel>();

        if(result.IsSuccess)
        {
            await InitializeAsync();
        }
    }

    [RelayCommand]
    private void ViewCustomerDetail(DemoCustomer customer)
    {
        if (customer == null) return;

        Logger.LogInformation("[WORKFLOW_STEP1] Opening customer detail as child for {CustomerId}",
            customer.Id);

        // Open detail window as child of shell - it will see same IOrderBuilderService!
        var detailParams = new Parameters.Repository.CustomerDetailParameters { CustomerId = customer.Id };
        _windowContext.OpenWindow<Customer.Repository.CustomerDetailViewModel, Parameters.Repository.CustomerDetailParameters>(detailParams);
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        if (SelectedCustomer == null) return;

        Logger.LogInformation("[WORKFLOW_STEP1] Moving to Step 2 with customer {CustomerId}",
            SelectedCustomer.Id);

        // Update shared service
        _orderBuilder.CustomerId = SelectedCustomer.Id;
        _orderBuilder.CustomerName = SelectedCustomer.Name;

        // Navigate to next step
        await _navigator.NavigateToAsync<WorkflowStep2ViewModel>();
    }

    private bool CanGoNext() => SelectedCustomer != null;

    partial void OnSelectedCustomerChanged(DemoCustomer? value)
    {
        NextCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[WORKFLOW_STEP1] ViewModel disposed");

        _disposed = true;
    }
}