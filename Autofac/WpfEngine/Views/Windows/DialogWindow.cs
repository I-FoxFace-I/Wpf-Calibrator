using System.Windows;
using Microsoft.Extensions.Logging;
using WpfEngine.Enums;
using WpfEngine.ViewModels;
using WpfEngine.Views;

namespace WpfEngine.Views.Windows;

/// <summary>
/// Dialog window with strongly-typed ViewModel
/// </summary>
public abstract class DialogWindow<TViewModel> : DialogWindow, IDialogView<TViewModel>
    where TViewModel : IViewModel
{
    protected DialogWindow(ILogger logger) : base(logger)
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public TViewModel? ContextModel
    {
        get => (TViewModel)DataContext;
        set => DataContext = value;
    }
}

/// <summary>
/// Dialog window without scope management
/// Can be used as modal or non-modal
/// </summary>
public abstract class DialogWindow : BaseWindow, IDialogView
{
    protected DialogWindow(ILogger logger) : base(logger)
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


