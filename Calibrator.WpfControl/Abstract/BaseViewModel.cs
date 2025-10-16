using System;
using System.Threading.Tasks;
using Calibrator.WpfControl.Abstract;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Calibrator.WpfControl.Abstract;

/// <summary>
/// Base view model, which supports loading indicator and manual loading state management.
/// NOTE: This version doesn't use PostSharp/Metalama [WithLoading] attribute - loading state
/// is managed manually in derived classes.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject, IInitializable
{
    [ObservableProperty] private bool _isLoading;

    /// <summary>
    /// Initializes the view model asynchronously
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation</returns>
    public abstract Task InitializeAsync();

    /// <summary>
    /// Helper method to execute an action with loading state management
    /// </summary>
    protected async Task ExecuteWithLoading(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsLoading) return; // Prevent multiple concurrent operations

        IsLoading = true;
        try
        {
            await action().ConfigureAwait(false);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Helper method to execute a function with loading state management.
    /// </summary>
    /// <typeparam name="T">The return type of the function</typeparam>
    /// <param name="function">The function to execute</param>
    /// <returns>The result of the function execution</returns>
    protected async Task<T> ExecuteWithLoading<T>(Func<Task<T>> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (IsLoading)
        {
            return default(T)!; // Prevent multiple concurrent operations
        }

        this.IsLoading = true;
        try
        {
            return await function().ConfigureAwait(false);
        }
        finally
        {
            this.IsLoading = false;
        }
    }
}