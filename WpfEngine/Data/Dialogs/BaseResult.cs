using WpfEngine.Data.Abstract;
using WpfEngine.Enums;

namespace WpfEngine.Data.Dialogs;

/// <summary>
/// Base record for ViewModel results
/// Provides abstract getter for Guid Key 
/// </summary>
public abstract record BaseResult : IDialogResult
{
    /// <summary>
    /// Default key created by Guid.NewGuid() method.
    /// </summary>
    protected Guid DefaultKey { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Defines the result status of dialog opperation.
    /// </summary>
    public DialogStatus Status { get; init; }

    /// <summary>
    /// Key identifying the result
    /// </summary>
    public virtual Guid Key => DefaultKey;
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public virtual bool IsSuccess => Status == DialogStatus.Success;

    /// <summary>
    /// Indicates if the operation was cancelled.
    /// </summary>
    public virtual bool IsCancelled => Status == DialogStatus.Cancel;

    /// <summary>
    /// Indicates if the operation was completed (Success or Cancel)
    /// </summary>
    public bool IsComplete => Status switch
    {
        DialogStatus.Success => true,
        DialogStatus.Cancel => true,
        DialogStatus.Error => false,
        DialogStatus.Pending => false,
        _ => false
    };

    /// <summary>
    /// Optional error message if operation failed
    /// </summary>
    public virtual string? ErrorMessage { get; init; } = null;
}
