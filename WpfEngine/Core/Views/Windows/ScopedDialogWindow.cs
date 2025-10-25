using System.Windows;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Windows;

// ========== SCOPED DIALOG WINDOW ==========

/// <summary>
/// Dialog window with its own scope
/// Useful for complex dialogs with child dependencies
/// </summary>
public abstract class ScopedDialogWindow : ScopedWindow, IDialogView
{
    protected ScopedDialogWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        object? scopeTag = null) : base(parentScope, logger, scopeTag)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    /// <summary>
    /// Dialog result
    /// </summary>
    public virtual bool? DialogResult { get; set; }

    /// <summary>
    /// Type/category of dialog
    /// </summary>
    public abstract DialogType DialogType { get; }

    /// <summary>
    /// Application module/section this dialog belongs to
    /// </summary>
    public abstract string? AppModule { get; }
}

/// <summary>
/// Scoped dialog window with strongly-typed ViewModel
/// </summary>
public abstract class ScopedDialogWindow<TViewModel> : ScopedDialogWindow, IDialogView<TViewModel>
    where TViewModel : IViewModel
{
    protected ScopedDialogWindow(
        ILifetimeScope parentScope,
        ILogger logger,
        object? scopeTag = null) : base(parentScope, logger, scopeTag)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}


