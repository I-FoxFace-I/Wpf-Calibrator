using System.Collections.ObjectModel;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using WpfEngine.Demo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Demo.ViewModels.Parameters;
using WpfEngine.Abstract;
using WpfEngine.Services;

namespace WpfEngine.Demo.ViewModels;

public partial class WorkflowStep1ViewModelRefactored : BaseViewModel, IInitializable, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllCustomersHandler;
    private readonly INavigator _navigator;
    private readonly IWindowContext _windowContext;
    private readonly IOrderBuilderService _orderBuilder;
    
    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();
    
    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;
    
    private bool _disposed;
    
    public WorkflowStep1ViewModelRefactored(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllCustomersHandler,
        INavigator navigator,
        IOrderBuilderService orderBuilder,
        IWindowContext windowContext,
        ILogger<WorkflowStep1ViewModelRefactored> logger) : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _navigator = navigator;
        _orderBuilder = orderBuilder;
        _windowContext = windowContext;
        Logger.LogInformation("[WORKFLOW_STEP1] ViewModel created");
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

            Logger.LogInformation("[WORKFLOW_STEP1] Loaded {Count} customers", Customers.Count);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateCustomer()
    {
        var result = await _windowContext.ShowDialogAsync<CreateCustomerViewModel>();

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
        var detailParams = new CustomerDetailParameters { CustomerId = customer.Id };
        _windowContext.OpenWindow<CustomerDetailViewModel, CustomerDetailParameters>(detailParams);
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
        await _navigator.NavigateToAsync<WorkflowStep2ViewModelRefactored>();
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
