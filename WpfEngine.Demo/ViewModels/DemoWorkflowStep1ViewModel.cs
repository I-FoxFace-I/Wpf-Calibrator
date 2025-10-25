using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

// ========== STEP 1: SELECT CUSTOMER (with session support) ==========

public partial class DemoWorkflowStep1ViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllCustomersHandler;
    private readonly INavigationService _navigator;
    private readonly IOrderBuilderService _orderBuilder; // Shared session service!
    
    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();
    
    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;
    
    private bool _disposed;
    
    public DemoWorkflowStep1ViewModel(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllCustomersHandler,
        INavigationService navigator,
        IOrderBuilderService orderBuilder,  // Injected from session scope!
        ILogger<DemoWorkflowStep1ViewModel> logger) : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _navigator = navigator;
        _orderBuilder = orderBuilder;
        
        Logger.LogInformation("[WORKFLOW] Step1 ViewModel created with shared OrderBuilder service");
    }
    
    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            var customers = await _getAllCustomersHandler.HandleAsync(new GetAllDemoCustomersQuery());
            
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
            
            Logger.LogInformation("[WORKFLOW] Step1 loaded {Count} customers", Customers.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void ViewCustomerDetail(DemoCustomer customer)
    {
        if (customer == null) return;
        
        Logger.LogInformation("[WORKFLOW] Opening non-modal customer detail for {CustomerId}", customer.Id);
        
        // NOTE: Would open in session if we had session reference
        // For now, this is disabled - customer detail not needed in workflow
    }
    
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        if (SelectedCustomer == null) return;
        
        Logger.LogInformation("[WORKFLOW] Moving to Step 2 with customer {CustomerId}", SelectedCustomer.Id);
        
        // Save to shared service
        _orderBuilder.CustomerId = SelectedCustomer.Id;
        _orderBuilder.CustomerName = SelectedCustomer.Name;
        
        // Pass state for navigation
        await _navigator.NavigateToAsync<DemoWorkflowStep2ViewModel, WorkflowState>(
            new WorkflowState { CustomerId = SelectedCustomer.Id, CustomerName = SelectedCustomer.Name }
        );
    }
    
    private bool CanGoNext() => SelectedCustomer != null;
    
    partial void OnSelectedCustomerChanged(DemoCustomer? value)
    {
        NextCommand.NotifyCanExecuteChanged();
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        Logger.LogInformation("[WORKFLOW] Step1 ViewModel disposed");
        
        _disposed = true;
    }
}
