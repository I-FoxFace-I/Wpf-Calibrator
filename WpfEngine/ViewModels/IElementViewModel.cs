namespace WpfEngine.ViewModels;

// ========== SPECIALIZED INTERFACES ==========


/// <summary>
/// ViewModel representing detail view of single entity (read-only)
/// </summary>
public interface IElementViewModel<T> : IViewModel
{
    /// <summary>
    /// Entity being displayed
    /// </summary>
    T? Entity { get; }

    /// <summary>
    /// Indicates if entity is in read-only mode
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Closes the detail view
    /// </summary>
    void Close();
}

/// <summary>
/// ViewModel for editing entity (extends detail with save/revert)
/// </summary>
public interface IEditViewModel<T> : IElementViewModel<T>
{
    /// <summary>
    /// Indicates if entity is in edit mode
    /// </summary>
    bool IsEditMode { get; }

    /// <summary>
    /// Saves changes to entity
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Reverts all unsaved changes to original state
    /// </summary>
    void Revert();

    /// <summary>
    /// Indicates if entity has unsaved changes
    /// </summary>
    bool HasChanges { get; }
}

/// <summary>
/// ViewModel for selecting entity (extends detail with selection)
/// </summary>
public interface ISelectViewModel<T> : IElementViewModel<T>
{
    /// <summary>
    /// Indicates if entity is selected
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Confirms/performs the selection
    /// </summary>
    void Select();
}

/// <summary>
/// ViewModel representing a list of items
/// Disposable to cleanup resources (subscriptions, etc.)
/// </summary>
public interface IListingViewModel<T> : IViewModel, IDisposable
{
    /// <summary>
    /// Collection of items
    /// </summary>
    IEnumerable<T> Items { get; }

    /// <summary>
    /// Currently selected item
    /// </summary>
    T? SelectedItem { get; set; }

    /// <summary>
    /// Refreshes specific item by ID
    /// </summary>
    Task RefreshAsync(object id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes entire list
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
