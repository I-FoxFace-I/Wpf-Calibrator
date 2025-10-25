using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Demo.ViewModels;

// ========== STEP 1: SELECT CUSTOMER (with non-modal detail windows) ==========

public partial class DemoWorkflowStep1ViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllCustomersHandler;
    private readonly INavigationService _navigator;
    private readonly IWindowService _windowService;
    
    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();
    
    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;
    
    private bool _disposed;
    
    public DemoWorkflowStep1ViewModel(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllCustomersHandler,
        INavigationService navigator,
        IWindowService WindowService,
        ILogger<DemoWorkflowStep1ViewModel> logger) : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _navigator = navigator;
        _windowService = WindowService;
        
        Logger.LogInformation("[WORKFLOW] Step1 ViewModel created");
    }
    
    public async Task InitializeAsync()
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
        
        // Open non-modal child window
        // NOTE: OpenChildWindow needs parent WINDOW ID, not ViewModel ID
        // In current architecture, we don't have direct access to window ID
        // This is a limitation of the original (non-refactored) approach
        // For now, open as regular window (not child)
        _windowService.OpenWindow<DemoCustomerDetailViewModel, DemoCustomerDetailParams>(
            new DemoCustomerDetailParams { CustomerId = customer.Id }
        );
    }
    
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        if (SelectedCustomer == null) return;
        
        Logger.LogInformation("[WORKFLOW] Moving to Step 2 with customer {CustomerId}", SelectedCustomer.Id);
        
        // Pass selected customer to next step
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
        
        // NOTE: CloseAllChildWindows needs window ID, not ViewModel ID
        // In non-session context, we don't track this properly
        // Windows will close when their parent scope disposes anyway
        
        _disposed = true;
    }
}
