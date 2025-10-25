using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfEngine.Core.ViewModels;

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

    protected BaseViewModel(ILogger logger)
    {
        Logger = logger;
        Id = Guid.NewGuid();
        Logger.LogDebug("[{ViewModelType}] Created with ID {Id}", GetType().Name, Id);
    }

    public Guid Id { get; }

    /// <summary>
    /// Initialize ViewModel - override for custom initialization logic
    /// </summary>
    public virtual Task InitializeAsync()
    {
        Logger.LogDebug("[{ViewModelType}] Initialized", GetType().Name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes async operation with busy indicator
    /// </summary>
    protected async Task ExecuteWithBusyAsync(Func<Task> operation, string? busyMessage = null)
    {
        try
        {
            IsBusy = true;
            BusyMessage = busyMessage;
            await operation();
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
    where TParameter : IVmParameters
{
    [ObservableProperty]
    private TParameter? _parameter;

    protected BaseViewModel(ILogger logger) : base(logger)
    {
        // Parameter will be set via InitializeAsync
    }

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
