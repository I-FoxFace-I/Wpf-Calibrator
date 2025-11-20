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
/// Workflow Step 2: Product Selection
/// Uses Repository pattern with Fluent API for data access
/// </summary>
public partial class WorkflowStep2ViewModel : StepViewModel, IInitializable, IDisposable
{
    private readonly IScopeManager _scopeManager;
    private readonly INavigator _navigator;
    private readonly IOrderBuilderService _orderBuilder;
    private readonly IWindowContext _windowContext;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DemoProduct> _products = new();

    [ObservableProperty]
    private int _quantity = 1;

    public ObservableCollection<WorkflowOrderItem> OrderItems => _orderBuilder.OrderItems;
    public decimal OrderTotal => _orderBuilder.Total;

    private bool _disposed;

    public WorkflowStep2ViewModel(
        IScopeManager scopeManager,
        INavigator navigator,
        IOrderBuilderService orderBuilder,
        IWindowContext windowContext,
        ILogger<WorkflowStep2ViewModel> logger)
        : base(logger, navigator)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _navigator = navigator;
        _orderBuilder = orderBuilder;
        _windowContext = windowContext;
        
        CustomerName = orderBuilder.CustomerName;
        _orderBuilder.OrderItemsChanged += OnOrderItemsChanged;
        Logger.LogInformation("[WORKFLOW_STEP2] ViewModel created");
    }


    public override async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            
            // Use Fluent API to load products
            var products = await _scopeManager
                .CreateDatabaseSession()
                .WithService<IRepository<DemoProduct>>()
                .ExecuteWithResultAsync(async (repo) =>
                {
                    return await repo.GetAllAsync();
                });

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            Logger.LogInformation("[WORKFLOW_STEP2] Loaded {Count} products, order has {OrderCount} items",
                Products.Count, OrderItems.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[WORKFLOW_STEP2] Error loading products");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // Save state to shared service (already done via direct manipulation)
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenProductSelector()
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Opening product selector as child");

        // Opens child window - will see same IOrderBuilderService from parent window scope!
        _windowContext.OpenWindow<ProductSelectorViewModel>();
    }

    [RelayCommand]
    private void ViewProductInfo(DemoProduct? product)
    {
        if (product == null) return;

        Logger.LogInformation("[WORKFLOW_STEP2] Opening product info for {ProductId}", product.Id);

        // Opens child window from shell
        _windowContext.OpenWindow<Product.Repository.ProductInfoViewModel, Parameters.Repository.ProductDetailParameters>(
            new Parameters.Repository.ProductDetailParameters { ProductId = product.Id }
        );
    }

    [RelayCommand]
    private void AddItemWithProduct(DemoProduct? product)
    {
        if (product == null || Quantity <= 0) return;

        // Add to SHARED service
        _orderBuilder.AddItem(product.Id, product.Name, product.Price, Quantity);

        Logger.LogInformation("[WORKFLOW_STEP2] Added {Quantity}x {Product} to shared order",
            Quantity, product.Name);
    }

    [RelayCommand]
    private void RemoveItem(WorkflowOrderItem? item)
    {
        if (item == null) return;

        _orderBuilder.RemoveItem(item);

        Logger.LogInformation("[WORKFLOW_STEP2] Removed {Product} from shared order", item.ProductName);
    }

    [RelayCommand]
    private async Task BackAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Going back to Step 1");
        await _navigator.NavigateBackAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private async Task NextAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("[WORKFLOW_STEP2] Moving to Step 3 - Review ({Count} items in shared order)",
            _orderBuilder.OrderItems.Count);

        await _navigator.NavigateToAsync<WorkflowStep3ViewModel>();
    }

    private bool CanGoNext() => _orderBuilder.OrderItems.Any();

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        base.Dispose(disposing);


        Logger.LogInformation("[WORKFLOW_STEP2] ViewModel disposed");

        // Unsubscribe from events
        if (_orderBuilder != null)
        {
            _orderBuilder.OrderItemsChanged -= OnOrderItemsChanged;
        }

        _disposed = true;
    }

    private void OnOrderItemsChanged(object? sender, EventArgs? args)
    {
        OnPropertyChanged(nameof(OrderItems));
        OnPropertyChanged(nameof(OrderTotal));
        NextCommand.NotifyCanExecuteChanged();
    }

    public override Task<bool> ValidateStepAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<bool>(CanGoNext());
    }
}