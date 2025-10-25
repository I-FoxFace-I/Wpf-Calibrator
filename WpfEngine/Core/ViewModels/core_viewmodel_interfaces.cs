using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace WpfEngine.Core.ViewModels;

// ========== BASE VIEWMODEL INTERFACES ==========

/// <summary>
/// Base interface for all ViewModels
/// All ViewModels must be initializable and support busy state
/// </summary>
public interface IViewModel : INotifyPropertyChanged, IInitializable, IBusyViewModel
{
    /// <summary>
    /// Unique identifier for this ViewModel instance
    /// </summary>
    Guid Id { get; }

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
    where TParameter : IVmParameters
{
    /// <summary>
    /// Parameters for this ViewModel (set during initialization)
    /// </summary>
    TParameter? Parameter { get; }
}

// ========== LIFECYCLE INTERFACES ==========

/// <summary>
/// Entity that requires async initialization
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Performs async initialization
    /// </summary>
    Task InitializeAsync();
}

/// <summary>
/// Entity that requires async initialization with parameters
/// </summary>
public interface IInitializable<in TParam>
{
    /// <summary>
    /// Performs async initialization with parameters
    /// </summary>
    Task InitializeAsync(TParam parameter);
}

// ========== DIALOG INTERFACES ==========

/// <summary>
/// ViewModel for dialog windows (no return value)
/// </summary>
public interface IDialogViewModel : IViewModel
{
    /// <summary>
    /// Indicates if dialog was completed successfully
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Indicates if dialog was cancelled
    /// </summary>
    bool IsCancelled { get; }
}

/// <summary>
/// Dialog ViewModel with parameters (no result)
/// </summary>
public interface IDialogViewModel<TParameter> : IViewModel<TParameter>, IDialogViewModel
    where TParameter : IVmParameters
{
}

/// <summary>
/// ViewModel for dialog windows with typed result
/// </summary>
public interface IDialogViewModel<TParameter, TResult> : IViewModel<TParameter>, IDialogViewModel
    where TParameter : IVmParameters
    where TResult : IVmResult
{
    /// <summary>
    /// Dialog result (null = cancelled/closed without result)
    /// </summary>
    TResult? DialogResult { get; }
}

// ========== SPECIALIZED INTERFACES ==========

/// <summary>
/// ViewModel representing a list of items
/// Disposable to cleanup resources (subscriptions, etc.)
/// </summary>
public interface IListingViewModel<T> : IViewModel, IDisposable
{
    /// <summary>
    /// Collection of items
    /// </summary>
    IEnumerable<T> Items { get; }

    /// <summary>
    /// Currently selected item
    /// </summary>
    T? SelectedItem { get; set; }

    /// <summary>
    /// Refreshes specific item by ID
    /// </summary>
    Task RefreshAsync(object id);

    /// <summary>
    /// Refreshes entire list
    /// </summary>
    Task RefreshAsync();
}

/// <summary>
/// ViewModel representing detail view of single entity (read-only)
/// </summary>
public interface IDetailViewModel<T> : IViewModel
{
    /// <summary>
    /// Entity being displayed
    /// </summary>
    T? Entity { get; }

    /// <summary>
    /// Indicates if entity is in read-only mode
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Closes the detail view
    /// </summary>
    void Close();
}

/// <summary>
/// ViewModel for editing entity (extends detail with save/revert)
/// </summary>
public interface IEditViewModel<T> : IDetailViewModel<T>
{
    /// <summary>
    /// Indicates if entity is in edit mode
    /// </summary>
    bool IsEditMode { get; }

    /// <summary>
    /// Saves changes to entity
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Reverts all unsaved changes to original state
    /// </summary>
    void Revert();

    /// <summary>
    /// Indicates if entity has unsaved changes
    /// </summary>
    bool HasChanges { get; }
}

/// <summary>
/// ViewModel for selecting entity (extends detail with selection)
/// </summary>
public interface ISelectViewModel<T> : IDetailViewModel<T>
{
    /// <summary>
    /// Indicates if entity is selected
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Confirms/performs the selection
    /// </summary>
    void Select();
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
    Task SaveAsync();

    /// <summary>
    /// Validates current step before navigation
    /// </summary>
    Task<bool> ValidateStepAsync();
}

// ========== STATE INTERFACES ==========

/// <summary>
/// Entity that tracks busy/loading state
/// </summary>
public interface IBusyViewModel
{
    /// <summary>
    /// Indicates if entity is performing async operation
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Optional busy message
    /// </summary>
    string? BusyMessage { get; }
}

/// <summary>
/// Entity that tracks error state
/// </summary>
public interface IErrorViewModel
{
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
}

/// <summary>
/// Entity that supports validation
/// </summary>
public interface IValidatable
{
    /// <summary>
    /// Validates entity state
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    bool Validate();

    /// <summary>
    /// Gets all validation errors
    /// </summary>
    IEnumerable<string> GetValidationErrors();
}
