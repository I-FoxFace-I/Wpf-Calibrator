using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutofacEnhancedWpfDemo.Application;
using AutofacEnhancedWpfDemo.Application.Customers;
using AutofacEnhancedWpfDemo.Application.Orders;
using AutofacEnhancedWpfDemo.Application.Products;
using AutofacEnhancedWpfDemo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutofacEnhancedWpfDemo.ViewModels;

/// <summary>
/// ViewModel for Order Creation Workflow
/// Multi-step process: 1) Select Customer 2) Add Products 3) Review & Confirm
/// </summary>
public partial class OrderWorkflowViewModel : BaseViewModel
{
    private readonly IQueryHandler<GetAllCustomersQuery, List<Customer>> _getAllCustomersHandler;
    private readonly IQueryHandler<GetAllProductsQuery, List<Product>> _getAllProductsHandler;
    private readonly ICommandHandler<CreateOrderCommand> _createOrderHandler;

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<WorkflowOrderItem> _orderItems = new();

    public decimal OrderTotal => OrderItems.Sum(i => i.Total);

    public OrderWorkflowViewModel(
        IQueryHandler<GetAllCustomersQuery, List<Customer>> getAllCustomersHandler,
        IQueryHandler<GetAllProductsQuery, List<Product>> getAllProductsHandler,
        ICommandHandler<CreateOrderCommand> createOrderHandler,
        ILogger<OrderWorkflowViewModel> logger) : base(logger)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _getAllProductsHandler = getAllProductsHandler;
        _createOrderHandler = createOrderHandler;
    }

    public async Task InitializeAsync()
    {
        await LoadCustomersAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            Logger.LogInformation("Loading customers for workflow");
            var customers = await _getAllCustomersHandler.HandleAsync(new GetAllCustomersQuery());

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load customers: {ex.Message}");
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            Logger.LogInformation("Loading products for workflow");
            var products = await _getAllProductsHandler.HandleAsync(new GetAllProductsQuery());

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load products: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextStep()
    {
        if (CurrentStep < 3)
        {
            CurrentStep++;
            Logger.LogInformation("Moved to step {Step}", CurrentStep);

            NextStepCommand.NotifyCanExecuteChanged();
            PreviousStepCommand.NotifyCanExecuteChanged();
            CompleteOrderCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanGoNext()
    {
        return CurrentStep switch
        {
            1 => SelectedCustomer != null,
            2 => OrderItems.Count > 0,
            _ => false
        };
    }

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            Logger.LogInformation("Moved back to step {Step}", CurrentStep);

            NextStepCommand.NotifyCanExecuteChanged();
            PreviousStepCommand.NotifyCanExecuteChanged();
            CompleteOrderCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanGoPrevious() => CurrentStep > 1;

    [RelayCommand]
    private void AddItem(Product product)
    {
        var existingItem = OrderItems.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity++;
            Logger.LogInformation("Increased quantity for {Product}", product.Name);
        }
        else
        {
            OrderItems.Add(new WorkflowOrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = 1
            });
            Logger.LogInformation("Added {Product} to order", product.Name);
        }

        OnPropertyChanged(nameof(OrderTotal));
        NextStepCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RemoveItem(WorkflowOrderItem item)
    {
        OrderItems.Remove(item);
        Logger.LogInformation("Removed {Product} from order", item.ProductName);

        OnPropertyChanged(nameof(OrderTotal));
        NextStepCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCompleteOrder))]
    private async Task CompleteOrderAsync()
    {
        if (SelectedCustomer == null || OrderItems.Count == 0)
            return;

        try
        {
            IsBusy = true;
            ClearError();

            Logger.LogInformation("Completing order for customer {Customer}", SelectedCustomer.Name);

            var command = new CreateOrderCommand(
                SelectedCustomer.Id,
                OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            );

            await _createOrderHandler.HandleAsync(command);

            Logger.LogInformation("Order completed successfully");

            // Reset workflow
            CurrentStep = 1;
            SelectedCustomer = null;
            OrderItems.Clear();
            OnPropertyChanged(nameof(OrderTotal));

            // Show success message (in real app, use MessageBox or Notification)
            Logger.LogInformation("✅ Order created successfully!");
        }
        catch (Exception ex)
        {
            SetError($"Failed to complete order: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanCompleteOrder() => CurrentStep == 3 && SelectedCustomer != null && OrderItems.Count > 0;

    [RelayCommand]
    private void CancelOrder()
    {
        Logger.LogInformation("Order workflow cancelled");

        CurrentStep = 1;
        SelectedCustomer = null;
        OrderItems.Clear();
        OnPropertyChanged(nameof(OrderTotal));
    }

    partial void OnCurrentStepChanged(int value)
    {
        NextStepCommand.NotifyCanExecuteChanged();
        PreviousStepCommand.NotifyCanExecuteChanged();
        CompleteOrderCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        NextStepCommand.NotifyCanExecuteChanged();
    }
}

/// <summary>
/// Workflow order item (in-memory, not persisted until completion)
/// </summary>
public partial class WorkflowOrderItem : ObservableObject
{
    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private int _quantity;

    public decimal Total => UnitPrice * Quantity;

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(Total));
    }

    partial void OnUnitPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(Total));
    }
}