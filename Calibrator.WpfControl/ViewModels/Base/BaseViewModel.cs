using System;
using System.Threading.Tasks;
using Calibrator.WpfControl.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Calibrator.WpfApplication.ViewModels;

/// <summary>
/// Base view model, which supports loading indicator and manual loading state management.
/// NOTE: This version doesn't use PostSharp/Metalama [WithLoading] attribute - 
/// loading state is managed manually in derived classes.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject, IInitializable
{
    [ObservableProperty] private bool _isLoading;
    
    public abstract Task InitializeAsync();
    
    /// <summary>
    /// Helper method to execute an action with loading state management
    /// </summary>
    protected async Task ExecuteWithLoading(Func<Task> action)
    {
        if (IsLoading) return; // Prevent multiple concurrent operations
        
        IsLoading = true;
        try
        {
            await action();
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Helper method to execute a function with loading state management
    /// </summary>
    protected async Task<T> ExecuteWithLoading<T>(Func<Task<T>> function)
    {
        if (IsLoading) return default(T)!; // Prevent multiple concurrent operations
        
        IsLoading = true;
        try
        {
            return await function();
        }
        finally
        {
            IsLoading = false;
        }
    }
}


