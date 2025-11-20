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
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Abstract;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

// ========== STEP 1: SELECT CUSTOMER (with non-modal detail windows) ==========

public partial class WorkflowStep1ViewModel : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllCustomersHandler;
    private readonly INavigator _navigator;
    private readonly IWindowContext _windowService;
    
    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();
    
    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;
    
    private bool _disposed;
    
    public WorkflowStep1ViewModel(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllCustomersHandler,
        INavigator navigator,
        IWindowContext windowService,
        ILogger<WorkflowStep1ViewModel> logger) : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler ?? throw new ArgumentNullException(nameof(getAllCustomersHandler));
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        
        Logger.LogInformation("[WORKFLOW] Step1 ViewModel created");
    }

    public override async Task InitializeAsync()
    {
        await InitializeAsync(CancellationToken.None);
    }
    public async Task InitializeAsync(CancellationToken cancelationToken = default)
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
        _windowService.OpenWindow<CustomerDetailViewModel, CustomerDetailParameters>(
            new CustomerDetailParameters { CustomerId = customer.Id }
        );
    }
    
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync()
    {
        if (SelectedCustomer == null) return;
        
        Logger.LogInformation("[WORKFLOW] Moving to Step 2 with customer {CustomerId}", SelectedCustomer.Id);
        
        // Pass selected customer to next step
        await _navigator.NavigateToAsync<WorkflowStep2ViewModel, WorkflowState>(
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
