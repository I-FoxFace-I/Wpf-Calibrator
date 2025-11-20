using System.Windows;
using Autofac;
using Microsoft.Extensions.Logging;
using WpfEngine.Enums;
using WpfEngine.ViewModels;
using WpfEngine.Views;

namespace WpfEngine.Views.Windows;

// ========== SCOPED DIALOG WINDOW ==========

/// <summary>
/// Dialog window with its own scope
/// Useful for complex dialogs with child dependencies
/// </summary>
public abstract class ScopedDialogWindow : ScopedWindow, IDialogView
{
    protected ScopedDialogWindow(ILogger logger) : base(logger)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    /// <summary>
    /// Type/category of dialog
    /// </summary>
    public virtual DialogType DialogType { get; protected set; } = DialogType.Custom;

    /// <summary>
    /// Application module/section this dialog belongs to
    /// </summary>
    public virtual string? AppModule { get; protected set; } = string.Empty;

}

/// <summary>
/// Scoped dialog window with strongly-typed ViewModel
/// </summary>
public abstract class ScopedDialogWindow<TViewModel> : ScopedDialogWindow, IDialogView<TViewModel>
    where TViewModel : IViewModel
{
    protected ScopedDialogWindow(ILogger logger) : base(logger)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}


