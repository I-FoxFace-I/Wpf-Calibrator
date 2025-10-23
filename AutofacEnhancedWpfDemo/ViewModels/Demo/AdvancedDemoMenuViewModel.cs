using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Demo.Customers;
using AutofacEnhancedWpfDemo.Models.Demo;
using AutofacEnhancedWpfDemo.Services;
using AutofacEnhancedWpfDemo.Services.Demo;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels.Demo;

// ========== ADVANCED DEMO MENU ==========

public partial class AdvancedDemoMenuViewModel : BaseViewModel
{
    private readonly IWindowManager _windowManager;
    
    public AdvancedDemoMenuViewModel(
        IWindowManager windowManager,
        ILogger<AdvancedDemoMenuViewModel> logger) : base(logger)
    {
        _windowManager = windowManager;
    }
    
    [RelayCommand]
    private void OpenCustomerList()
    {
        Logger.LogInformation("[DEMO] Opening customer list");
        _windowManager.ShowWindow<DemoCustomerListViewModel>();
    }
    
    [RelayCommand]
    private void OpenProductList()
    {
        Logger.LogInformation("[DEMO] Opening product list");
        _windowManager.ShowWindow<DemoProductListViewModel>();
    }
    
    [RelayCommand]
    private void OpenWorkflow()
    {
        Logger.LogInformation("[DEMO] Opening workflow with Navigator");
        _windowManager.ShowWindow<DemoWorkflowHostViewModel>();
    }
}

// ========== DEMO CUSTOMER LIST ==========

//public partial class DemoCustomerListViewModel : BaseViewModel
//{
//    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllHandler;
//    private readonly ICommandHandler<DeleteDemoCustomerCommand> _deleteHandler;
//    private readonly IWindowManager _windowManager;
    
//    [ObservableProperty]
//    private ObservableCollection<DemoCustomer> _customers = new();
    
//    [ObservableProperty]
//    private DemoCustomer? _selectedCustomer;
    
//    public DemoCustomerListViewModel(
//        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllHandler,
//        ICommandHandler<DeleteDemoCustomerCommand> deleteHandler,
//        IWindowManager windowManager,
//        ILogger<DemoCustomerListViewModel> logger) : base(logger)
//    {
//        _getAllHandler = getAllHandler;
//        _deleteHandler = deleteHandler;
//        _windowManager = windowManager;
//    }
    
//    public async Task InitializeAsync()
//    {
//        await LoadCustomersAsync();
//    }
    
//    [RelayCommand]
//    private async Task LoadCustomersAsync()
//    {
//        try
//        {
//            IsBusy = true;
//            ClearError();
            
//            var customers = await _getAllHandler.HandleAsync(new GetAllDemoCustomersQuery());
            
//            Customers.Clear();
//            foreach (var customer in customers)
//            {
//                Customers.Add(customer);
//            }
            
//            Logger.LogInformation("[DEMO] Loaded {Count} customers", Customers.Count);
//        }
//        catch (Exception ex)
//        {
//            SetError($"Failed to load customers: {ex.Message}");
//        }
//        finally
//        {
//            IsBusy = false;
//        }
//    }
    
//    [RelayCommand(CanExecute = nameof(CanViewDetail))]
//    private async Task ViewDetailAsync()
//    {
//        if (SelectedCustomer == null) return;
        
//        Logger.LogInformation("[DEMO] Opening customer detail for {CustomerId}", SelectedCustomer.Id);
        
//        await _windowManager.ShowDialogAsync<DemoCustomerDetailViewModel, bool>(
//            new DemoCustomerDetailParams { CustomerId = SelectedCustomer.Id }
//        );
        
//        await LoadCustomersAsync();
//    }
    
//    private bool CanViewDetail() => SelectedCustomer != null && !IsBusy;
    
//    [RelayCommand(CanExecute = nameof(CanDelete))]
//    private async Task DeleteCustomerAsync()
//    {
//        if (SelectedCustomer == null) return;
        
//        try
//        {
//            IsBusy = true;
//            await _deleteHandler.HandleAsync(new DeleteDemoCustomerCommand(SelectedCustomer.Id));
//            await LoadCustomersAsync();
//        }
//        catch (Exception ex)
//        {
//            SetError($"Failed to delete customer: {ex.Message}");
//        }
//        finally
//        {
//            IsBusy = false;
//        }
//    }
    
//    private bool CanDelete() => SelectedCustomer != null && !IsBusy;
    
//    partial void OnSelectedCustomerChanged(DemoCustomer? value)
//    {
//        ViewDetailCommand.NotifyCanExecuteChanged();
//        DeleteCustomerCommand.NotifyCanExecuteChanged();
//    }
//}

// ========== DEMO CUSTOMER DETAIL ==========

//public partial class DemoCustomerDetailViewModel : BaseViewModel
//{
//    private readonly IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> _getCustomerHandler;
//    private readonly ICommandHandler<UpdateDemoCustomerCommand> _updateHandler;
//    private readonly IWindowManager _windowManager;
//    private readonly int _customerId;
    
//    [ObservableProperty]
//    private DemoCustomer? _customer;
    
//    [ObservableProperty]
//    private string _name = string.Empty;
    
//    [ObservableProperty]
//    private string _email = string.Empty;
    
//    [ObservableProperty]
//    private string _phone = string.Empty;
    
//    [ObservableProperty]
//    private string _companyName = string.Empty;
    
//    [ObservableProperty]
//    private string _taxId = string.Empty;
    
//    [ObservableProperty]
//    private CustomerType _type = CustomerType.Individual;
    
//    [ObservableProperty]
//    private ObservableCollection<DemoAddress> _addresses = new();
    
//    public DemoCustomerDetailViewModel(
//        IQueryHandler<GetDemoCustomerByIdQuery, DemoCustomer?> getCustomerHandler,
//        ICommandHandler<UpdateDemoCustomerCommand> updateHandler,
//        IWindowManager windowManager,
//        ILogger<DemoCustomerDetailViewModel> logger,
//        DemoCustomerDetailParams parameters) : base(logger)
//    {
//        _getCustomerHandler = getCustomerHandler;
//        _updateHandler = updateHandler;
//        _windowManager = windowManager;
//        _customerId = parameters.CustomerId;
//    }
    
//    public async Task InitializeAsync()
//    {
//        try
//        {
//            IsBusy = true;
            
//            var customer = await _getCustomerHandler.HandleAsync(new GetDemoCustomerByIdQuery(_customerId));
            
//            if (customer != null)
//            {
//                Customer = customer;
//                Name = customer.Name;
//                Email = customer.Email;
//                Phone = customer.Phone;
//                CompanyName = customer.CompanyName;
//                TaxId = customer.TaxId;
//                Type = customer.Type;
                
//                Addresses.Clear();
//                foreach (var address in customer.Addresses)
//                {
//                    Addresses.Add(address);
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            SetError($"Failed to load customer: {ex.Message}");
//        }
//        finally
//        {
//            IsBusy = false;
//        }
//    }
    
//    [RelayCommand(CanExecute = nameof(CanSave))]
//    private async Task SaveAsync()
//    {
//        try
//        {
//            IsBusy = true;
            
//            await _updateHandler.HandleAsync(new UpdateDemoCustomerCommand(
//                _customerId, Name, Email, Phone, CompanyName, TaxId, Type
//            ));
            
//            _windowManager.CloseDialog<DemoCustomerDetailViewModel>(true);
//        }
//        catch (Exception ex)
//        {
//            SetError($"Failed to save: {ex.Message}");
//        }
//        finally
//        {
//            IsBusy = false;
//        }
//    }
    
//    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && !IsBusy;
    
//    [RelayCommand]
//    private void Cancel()
//    {
//        _windowManager.CloseDialog<DemoCustomerDetailViewModel>(false);
//    }
    
//    partial void OnNameChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
//}

//public record DemoCustomerDetailParams
//{
//    public int CustomerId { get; init; }
//}
