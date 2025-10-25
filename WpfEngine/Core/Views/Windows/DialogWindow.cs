using System.Windows;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.ViewModels;

namespace WpfEngine.Core.Views.Windows;

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


