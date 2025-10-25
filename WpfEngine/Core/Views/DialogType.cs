namespace WpfEngine.Core.Views;

/// <summary>
/// Type/category of dialog
/// </summary>
public enum DialogType
{
    /// <summary>
    /// Simple confirmation dialog (Yes/No, OK/Cancel)
    /// </summary>
    Confirmation,

    /// <summary>
    /// Information display dialog
    /// </summary>
    Information,

    /// <summary>
    /// Warning dialog
    /// </summary>
    Warning,

    /// <summary>
    /// Error dialog
    /// </summary>
    Error,

    /// <summary>
    /// Input dialog (text entry, forms)
    /// </summary>
    Input,

    /// <summary>
    /// Selection dialog (pick from list)
    /// </summary>
    Selection,

    /// <summary>
    /// Detail/Edit dialog for entity
    /// </summary>
    Detail,

    /// <summary>
    /// Settings/Configuration dialog
    /// </summary>
    Settings,

    /// <summary>
    /// Custom dialog type
    /// </summary>
    Custom
}
