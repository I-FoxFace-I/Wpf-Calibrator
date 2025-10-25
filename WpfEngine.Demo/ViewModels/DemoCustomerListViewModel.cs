using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WpfEngine.Demo.Application;
using WpfEngine.Demo.Application.Customers;
using WpfEngine.Demo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;

namespace WpfEngine.Demo.ViewModels;

/// <summary>
/// Customer List ViewModel with non-modal detail windows support
/// Subscribes to WindowService events for real-time updates
/// </summary>
public partial class DemoCustomerListViewModel : BaseViewModel, IDisposable
{
    private readonly IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> _getAllHandler;
    private readonly ICommandHandler<DeleteDemoCustomerCommand> _deleteHandler;
    private readonly IWindowService _windowService;

    // Track open detail windows by customer ID
    private readonly Dictionary<int, Guid> _openDetailWindows = new();

    private bool _disposed;

    [ObservableProperty]
    private ObservableCollection<DemoCustomer> _customers = new();

    [ObservableProperty]
    private DemoCustomer? _selectedCustomer;

    public DemoCustomerListViewModel(
        IQueryHandler<GetAllDemoCustomersQuery, List<DemoCustomer>> getAllHandler,
        ICommandHandler<DeleteDemoCustomerCommand> deleteHandler,
        IWindowService WindowService,
        ILogger<DemoCustomerListViewModel> logger) : base(logger)
    {
        _getAllHandler = getAllHandler;
        _deleteHandler = deleteHandler;
        _windowService = WindowService;

        // Subscribe to window events for real-time updates
        _windowService.WindowClosed += OnWindowClosed;

        Logger.LogInformation("[DEMO] CustomerListViewModel created with event subscription");
    }

    public new async Task InitializeAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var customers = await _getAllHandler.HandleAsync(new GetAllDemoCustomersQuery());

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }

            Logger.LogInformation("[DEMO] Loaded {Count} customers", Customers.Count);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load customers: {ex.Message}");
            Logger.LogError(ex, "[DEMO] Error loading customers");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanViewDetail))]
    private void ViewDetail()
    {
        if (SelectedCustomer == null) return;

        var customerId = SelectedCustomer.Id;

        // Check if detail window already open for this customer
        if (_openDetailWindows.ContainsKey(customerId))
        {
            Logger.LogInformation("[DEMO] Detail window already open for customer {CustomerId}", customerId);
            // Could focus existing window here
            return;
        }

        Logger.LogInformation("[DEMO] Opening non-modal customer detail for {CustomerId}", customerId);

        // Generate unique window ID
        var itemParams = new DemoCustomerDetailParams { CustomerId = customerId };
        var windowId = itemParams.CorrelationId;
        
        _openDetailWindows[customerId] = windowId;

        // Open non-modal child window
        _windowService.OpenChildWindow<DemoCustomerDetailViewModel, DemoCustomerDetailParams>(
            windowId,
            itemParams
        );
    }

    private bool CanViewDetail() => SelectedCustomer != null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        var customerId = SelectedCustomer.Id;

        // Check if detail window is open
        if (_openDetailWindows.ContainsKey(customerId))
        {
            Logger.LogWarning("[DEMO] Cannot delete customer {CustomerId} - detail window is open", customerId);
            SetError("Cannot delete customer while detail window is open. Please close it first.");
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            await _deleteHandler.HandleAsync(new DeleteDemoCustomerCommand(customerId));
            await LoadCustomersAsync();

            Logger.LogInformation("[DEMO] Deleted customer {CustomerId}", customerId);
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete customer: {ex.Message}");
            Logger.LogError(ex, "[DEMO] Error deleting customer {CustomerId}", customerId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDelete() => SelectedCustomer != null && !IsBusy;

    partial void OnSelectedCustomerChanged(DemoCustomer? value)
    {
        ViewDetailCommand.NotifyCanExecuteChanged();
        DeleteCustomerCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Handles window closed event from WindowService
    /// Refreshes list when detail window closes (after potential updates)
    /// </summary>
    private async void OnWindowClosed(object? sender, WindowClosedEventArgs e)
    {
        // Check if it's a CustomerDetail window
        if (e.ViewModelType == typeof(DemoCustomerDetailViewModel))
        {
            Logger.LogInformation("[DEMO] CustomerDetail window {WindowId} closed, refreshing list", e.WindowId);

            // Remove from tracking
            var customerId = _openDetailWindows.FirstOrDefault(kvp => kvp.Value == e.WindowId).Key;
            if (customerId != 0)
            {
                _openDetailWindows.Remove(customerId);
            }

            // Refresh list to show any updates made in detail window
            await LoadCustomersAsync();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Logger.LogInformation("[DEMO] CustomerListViewModel disposing - closing {Count} detail windows",
            _openDetailWindows.Count);

        // Unsubscribe from events
        _windowService.WindowClosed -= OnWindowClosed;

        // Close all open detail windows
        foreach (var windowId in _openDetailWindows.Values.ToList())
        {
            _windowService.CloseWindow<DemoCustomerListViewModel>(windowId);
        }

        _openDetailWindows.Clear();

        _disposed = true;
    }
}
