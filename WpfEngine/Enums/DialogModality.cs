namespace WpfEngine.Enums;

/// <summary>
/// Dialog modality options
/// </summary>
public enum DialogModality
{
    /// <summary>
    /// Application-modal dialog (ShowDialog)
    /// </summary>
    AppModal,
    
    /// <summary>
    /// Window-modal dialog (disables owner tree, shows non-modal)
    /// </summary>
    WindowModal
}

