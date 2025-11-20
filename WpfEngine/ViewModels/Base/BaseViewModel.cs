using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using WpfEngine.Data.Abstract;
using WpfEngine.ViewModels;

namespace WpfEngine.ViewModels.Base;

// ========== BASE VIEWMODEL ==========

/// <summary>
/// Base implementation of IViewModel with logging and busy state
/// All ViewModels must implement InitializeAsync
/// </summary>
public abstract partial class BaseViewModel : ObservableObject, IViewModel
{
    protected readonly ILogger Logger;

    [ObservableProperty]
    private string? _displayName;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _busyMessage;
    
    [ObservableProperty]
    private string _name = "";


    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _errorMessage;

    protected void SetError(string errorMessage)
    {
        HasError = true;
        ErrorMessage = errorMessage;
        Logger.LogError("[{ViewModelType}] Error: {ErrorMessage}", GetType().Name, errorMessage);
    }

    public void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    protected BaseViewModel(ILogger<BaseViewModel> logger)
    {
        ViewModelId = Guid.NewGuid();
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Logger.LogDebug("[{ViewModelType}] Created with ID {Id}", GetType().Name, ViewModelId);
    }

    public Guid ViewModelId { get; private set; }


    /// <summary>
    /// Initialize ViewModel - override for custom initialization logic
    /// </summary>
    public virtual Task InitializeAsync()
    {
        Logger.LogDebug("[{ViewModelType}] Initialized", GetType().Name);
        return Task.CompletedTask;
    }

    public virtual void Reload()
    {
        
    }

    public virtual Task ReloadAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes async operation with busy indicator
    /// </summary>
    protected async Task ExecuteWithBusyAsync(Func<Task> operation, string? busyMessage = null)
    {
        try
        {
            ClearError();
            IsBusy = true;
            BusyMessage = busyMessage;
            await operation();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }
}

/// <summary>
/// Base ViewModel with parameters
/// Parameters are set via InitializeAsync after construction
/// </summary>
public abstract partial class BaseViewModel<TParameter> : BaseViewModel, IViewModel<TParameter>
    where TParameter : IViewModelParameters
{
    

    protected BaseViewModel(ILogger<BaseViewModel<TParameter>> logger) : base(logger)
    {
        // Parameter will be set via InitializeAsync
    }

    protected BaseViewModel(ILogger<BaseViewModel<TParameter>> logger, TParameter parameter) : base(logger)
    {
        _parameter = parameter;
    }

    [ObservableProperty]
    private TParameter? _parameter;
    /// <summary>
    /// Initialize ViewModel with parameters
    /// Called after construction by factory
    /// </summary>
    public virtual Task InitializeAsync(TParameter parameter)
    {
        Parameter = parameter;
        Logger.LogDebug("[{ViewModelType}] Initialized with parameters (CorrelationId: {CorrelationId})",
            GetType().Name, parameter.CorrelationId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Base InitializeAsync - calls InitializeAsync(Parameter) if Parameter is set
    /// </summary>
    public override async Task InitializeAsync()
    {
        if (Parameter != null)
        {
            await InitializeAsync(Parameter);
        }
        else
        {
            await base.InitializeAsync();
        }
    }
}

/// <summary>
/// USAGE EXAMPLES:
/// 
/// 1. Simple ViewModel (no navigation):
/// 
///    public partial class SimpleViewModel : BaseViewModel
///    {
///        public SimpleViewModel(ILogger<SimpleViewModel> logger)
///            : base(logger)
///        {
///        }
///        
///        [RelayCommand]
///        private void DoSomething()
///        {
///            // Simple action
///        }
///    }
/// 
/// 2. ViewModel with Navigation:
/// 
///    public partial class DashboardViewModel : BaseViewModel
///    {
///        public DashboardViewModel(
///            ILogger<DashboardViewModel> logger,
///            INavigator navigator)
///            : base(logger, navigator)
///        {
///        }
///        
///        [RelayCommand]
///        private async Task ShowSettings()
///        {
///            await Navigator!.NavigateToAsync<SettingsViewModel>();
///        }
///        
///        [RelayCommand]
///        private async Task GoBack()
///        {
///            await Navigator!.NavigateBackAsync();
///        }
///        
///        [RelayCommand]
///        private async Task CloseWindow()
///        {
///            await Navigator!.RequestCloseAsync(showConfirmation: true);
///        }
///    }
/// 
/// 3. ViewModel with Parameters:
/// 
///    public class OrderEditParameters : IViewModelParameters
///    {
///        public Guid CorrelationId { get; init; } = Guid.NewGuid();
///        public int OrderId { get; init; }
///    }
///    
///    public partial class OrderEditViewModel : BaseViewModel<OrderEditParameters>
///    {
///        public OrderEditViewModel(
///            ILogger<OrderEditViewModel> logger,
///            INavigator navigator,
///            OrderEditParameters parameters)
///            : base(logger, navigator, parameters)
///        {
///        }
///        
///        public override async Task InitializeAsync(OrderEditParameters parameters)
///        {
///            await base.InitializeAsync(parameters);
///            
///            // Load order data
///            var order = await LoadOrderAsync(parameters.OrderId);
///            // ... initialize view
///        }
///        
///        [RelayCommand]
///        private async Task Save()
///        {
///            // Save changes
///            await SaveOrderAsync();
///            
///            // Navigate back
///            await Navigator!.NavigateBackAsync();
///        }
///    }
/// 
/// 4. Shell ViewModel (hosts Navigator):
/// 
///    public partial class MainShellViewModel : BaseViewModel
///    {
///        public MainShellViewModel(
///            ILogger<MainShellViewModel> logger,
///            INavigator navigator)
///            : base(logger, navigator)
///        {
///            // Configure close handler
///            navigator.SetCloseHandler(HandleCloseRequestAsync);
///        }
///        
///        // Expose Navigator for XAML binding
///        public INavigator ShellNavigator => Navigator!;
///        
///        public override async Task InitializeAsync()
///        {
///            await base.InitializeAsync();
///            
///            // Navigate to initial view
///            await Navigator!.NavigateToAsync<DashboardViewModel>();
///        }
///        
///        private async Task<bool> HandleCloseRequestAsync(bool showConfirmation, string? message)
///        {
///            if (showConfirmation)
///            {
///                message ??= "Are you sure you want to close?";
///                // Show confirmation dialog
///                var dialogService = // ... get from DI
///                var result = await dialogService.ShowConfirmationAsync(message);
///                if (!result)
///                    return false;
///            }
///            
///            // Close window
///            Application.Current.Shutdown(); // or specific window close logic
///            return true;
///        }
///    }
///    
///    <!-- MainShellWindow.xaml -->
///    <Window>
///        <ContentControl Content="{Binding ShellNavigator.CurrentViewModel}" />
///    </Window>
/// </summary>