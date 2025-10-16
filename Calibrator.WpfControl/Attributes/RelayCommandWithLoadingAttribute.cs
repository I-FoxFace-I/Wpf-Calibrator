using System;

namespace Calibrator.WpfControl.Attributes;

/// <summary>
/// Generates an async relay command with automatic loading state management.
/// Works like [RelayCommand] from CommunityToolkit.Mvvm but automatically wraps execution with IsLoading.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RelayCommandWithLoadingAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the method that determines if the command can execute.
    /// </summary>
    public string? CanExecute { get; set; }

    /// <summary>
    /// Gets or sets whether multiple concurrent executions are allowed.
    /// When false (default), the command checks IsLoading and prevents concurrent execution.
    /// </summary>
    public bool AllowConcurrentExecutions { get; set; }

    /// <summary>
    /// Gets or sets whether to generate a cancel command for operations with CancellationToken.
    /// </summary>
    public bool IncludeCancelCommand { get; set; }

    /// <summary>
    /// Gets or sets whether exceptions should flow to TaskScheduler.UnobservedTaskException.
    /// </summary>
    public bool FlowExceptionsToTaskScheduler { get; set; }
}

