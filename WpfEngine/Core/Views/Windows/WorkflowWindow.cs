using Autofac;
using Microsoft.Extensions.Logging;
using System.Windows;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Windows;

/// <summary>
/// Workflow host window with navigation shell
/// Has workflow scope and changeable content
/// </summary>
public abstract class WorkflowWindow : ScopedWindow, IWorkflowView
{
    protected WorkflowWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        string? workflowName = null) : base(parentScope, logger, $"workflow-{workflowName ?? Guid.NewGuid().ToString()}")
    {
    }

    /// <summary>
    /// Current workflow step ViewModel
    /// </summary>
    public object? CurrentContent
    {
        get => GetValue(CurrentContentProperty);
        set => SetValue(CurrentContentProperty, value);
    }

    public static readonly DependencyProperty CurrentContentProperty =
        DependencyProperty.Register(
            nameof(CurrentContent),
            typeof(object),
            typeof(WorkflowWindow),
            new PropertyMetadata(null));
}

/// <summary>
/// Workflow window with strongly-typed host ViewModel
/// </summary>
public abstract class WorkflowWindow<TViewModel> : WorkflowWindow, IWorkflowView<TViewModel>
    where TViewModel : IViewModel
{
    protected WorkflowWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        string? workflowName = null) : base(parentScope, logger, workflowName)
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}
