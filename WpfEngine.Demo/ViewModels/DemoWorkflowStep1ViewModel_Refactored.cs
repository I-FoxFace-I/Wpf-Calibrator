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

/// <summary>
/// Step 1: Select Customer
/// 
/// SCOPE RESOLUTION:
/// - This ViewModel is resolved from WINDOW scope
/// - But can access IWorkflowSession from PARENT (session) scope
/// - Can open detail windows IN SESSION (they see shared services)
/// </summary>
public partial class DemoWorkflowStep1ViewModelRefactored : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllCustomersHandler;
    private readonly IContentManager _contentManager;
    private readonly IWorkflowSession? _workflowSession; // From session scope (optional)
    private readonly IOrderBuilderService _orderBuilder; // SHARED service from session!
    
    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();
    
    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;
    
    private bool _disposed;
    
    public DemoWorkflowStep1ViewModelRefactored(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllCustomersHandler,
        IContentManager contentManager,
        IOrderBuilderService orderBuilder,  // ← INJECTED from session scope!
        ILogger<DemoWorkflowStep1ViewModelRefactored> logger,
        IWorkflowSession? workflowSession = null) // Optional - available if in session
        : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _contentManager = contentManager;
        _orderBuilder = orderBuilder;
        _workflowSession = workflowSession;
        
        Logger.LogInformation("[WORKFLOW_STEP1] ViewModel created (Session: {HasSession})", 
            workflowSession != null);
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
            
            Logger.LogInformation("[WORKFLOW_STEP1] Loaded {Count} customers", Customers.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private void ViewCustomerDetail(DemoCustomer customer)
    {
        if (customer == null || _workflowSession == null) return;
        
        Logger.LogInformation("[WORKFLOW_STEP1] Opening customer detail in session for {CustomerId}", 
            customer.Id);
        
        // Open detail window IN SESSION - it will see same IOrderBuilderService!
        var detailParams = new DemoCustomerDetailParams { CustomerId = customer.Id };
        _workflowSession.OpenChildWindow<DemoCustomerDetailViewModel, DemoCustomerDetailParams>(
            Id,
            detailParams
        );
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
        await _contentManager.NavigateToAsync<DemoWorkflowStep2ViewModelRefactored>();
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

