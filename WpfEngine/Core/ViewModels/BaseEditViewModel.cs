using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using WpfEngine.Core.Services;

namespace WpfEngine.Core.ViewModels;

// ========== EDIT VIEWMODEL ==========

/// <summary>
/// Base ViewModel for editing entities
/// </summary>
public abstract partial class BaseEditViewModel<T> : BaseDetailViewModel<T>, IEditViewModel<T>
{
    [ObservableProperty]
    private bool _isEditMode = true;

    [ObservableProperty]
    private bool _hasChanges;

    protected T? _originalEntity;

    protected BaseEditViewModel(ILogger logger, IWindowService? windowService = null) 
        : base(logger, windowService)
    {
        IsReadOnly = false;
    }

    /// <summary>
    /// Saves changes to entity
    /// </summary>
    public abstract Task SaveAsync();

    /// <summary>
    /// Reverts all unsaved changes to original state
    /// </summary>
    public virtual void Revert()
    {
        if (_originalEntity != null)
        {
            // Derived classes should implement actual revert logic
            Entity = CloneEntity(_originalEntity);
            HasChanges = false;
            Logger.LogInformation("[{ViewModelType}] Changes reverted", GetType().Name);
        }
    }

    /// <summary>
    /// Clone entity - override in derived classes for proper cloning
    /// </summary>
    protected virtual T? CloneEntity(T entity)
    {
        // Default implementation - derived classes should override
        return entity;
    }

    /// <summary>
    /// Track changes - call this when entity is modified
    /// </summary>
    protected void MarkAsChanged()
    {
        HasChanges = true;
    }
}
