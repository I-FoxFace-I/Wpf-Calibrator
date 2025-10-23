using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Demo.Customers;
using AutofacEnhancedWpfDemo.Models.Demo;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

// ========== STEP 1: SELECT CUSTOMER (with non-modal detail windows) ==========

public partial class DemoWorkflowStep1ViewModel : BaseViewModel, IAsyncInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllCustomersHandler;
    private readonly INavigator _navigator;
    private readonly IWindowManager _windowManager;
    
    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();
    
    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;
    
    private bool _disposed;
    
    public DemoWorkflowStep1ViewModel(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllCustomersHandler,
        INavigator navigator,
        IWindowManager windowManager,
        ILogger<DemoWorkflowStep1ViewModel> logger) : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _navigator = navigator;
        _windowManager = windowManager;
        
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
        _windowManager.ShowChildWindow<DemoCustomerDetailViewModel>(
            Guid.NewGuid(),
            new DemoCustomerDetailParams { CustomerId = customer.Id }
        );
    }
    
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        if (SelectedCustomer == null) return;
        
        Logger.LogInformation("[WORKFLOW] Moving to Step 2 with customer {CustomerId}", SelectedCustomer.Id);
        
        // Pass selected customer to next step
        await _navigator.NavigateToAsync<DemoWorkflowStep2ViewModel>(
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
        
        Logger.LogInformation("[WORKFLOW] Step1 ViewModel disposed - closing child windows");
        
        // Close all child windows opened from this step
        _windowManager.CloseAllChildWindows();
        
        _disposed = true;
    }
}
