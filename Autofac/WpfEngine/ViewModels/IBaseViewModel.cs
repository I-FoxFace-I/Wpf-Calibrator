using Microsoft.Extensions.Logging;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Navigation;
using WpfEngine.Abstract;
using WpfEngine.Data.Abstract;

namespace WpfEngine.ViewModels;

// ========== BASE VIEWMODEL INTERFACES ==========

public interface IBaseViewModel : INotifyPropertyChanged
{

    /// <summary>
    /// Indicates if entity is performing async operation
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Optional busy message
    /// </summary>
    string? BusyMessage { get; }

    /// <summary>
    /// Indicates if entity has error
    /// </summary>
    bool HasError { get; }

    /// <summary>
    /// Current error message
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// Clears error state
    /// </summary>
    void ClearError();

    /// <summary>
    /// Performs data reload opperation
    /// </summary>
    void Reload();

    /// <summary>
    /// Performs async data reload opperation
    /// </summary>
    Task ReloadAsync();
}

/// <summary>
/// Base interface for all ViewModels
/// All ViewModels must be initializable and support busy state
/// </summary>
public interface IViewModel : IBaseViewModel, IInitializable
{
    /// <summary>
    /// Unique identifier for this ViewModel instance
    /// </summary>
    Guid ViewModelId { get; }

    /// <summary>
    /// Display name for UI (window title, tab header, etc.)
    /// </summary>
    string? DisplayName { get; }
}

/// <summary>
/// ViewModel that accepts strongly-typed parameters
/// Parameters are set via InitializeAsync after construction
/// </summary>
public interface IViewModel<TParameter> : IViewModel, IInitializable<TParameter>
    where TParameter : IViewModelParameters
{
    /// <summary>
    /// Parameters for this ViewModel (set during initialization)
    /// </summary>
    TParameter? Parameter { get; }
}


/// <summary>
/// ViewModel representing a step in workflow
/// Disposable - disposed when navigating away from step
/// </summary>
public interface IStepViewModel : IViewModel, IDisposable
{
    /// <summary>
    /// Can navigate to next step
    /// </summary>
    bool CanNavigateNext { get; }

    /// <summary>
    /// Can navigate to previous step
    /// </summary>
    bool CanNavigateBack { get; }

    /// <summary>
    /// Saves current step data
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates current step before navigation
    /// </summary>
    Task<bool> ValidateStepAsync(CancellationToken cancellationToken = default);
}
