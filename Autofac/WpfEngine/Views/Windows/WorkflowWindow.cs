using Autofac;
using Microsoft.Extensions.Logging;
using System.Windows;
using WpfEngine.ViewModels;
using WpfEngine.Views;

namespace WpfEngine.Views.Windows;

/// <summary>
/// Workflow host window with navigation shell
/// Has workflow scope and changeable content
/// </summary>
public abstract class WorkflowWindow : ScopedWindow, IWorkflowView
{
    protected WorkflowWindow(ILogger logger) : base(logger)
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

    //public Guid WindowId => AssignedWindowId;

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
    protected WorkflowWindow(ILogger logger) : base(logger)
    {
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}
