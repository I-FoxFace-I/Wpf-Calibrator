using System;

namespace HierarchicalMvvm.Core
{
    /// <summary>
    /// Interface for change tracking
    /// </summary>
    public interface IChangeTracker
    {
        /// <summary>
        /// Sets callback to be invoked when changes occur
        /// </summary>
        void SetCallback(Action? callback);

        /// <summary>
        /// Removes the change callback
        /// </summary>
        void RemoveChangeCallback();

        /// <summary>
        /// Marks this object as changed and invokes callback
        /// </summary>
        void MarkChanged();

        /// <summary>
        /// Starts batching changes (callbacks are deferred)
        /// </summary>
        void BeginBatchMode();

        /// <summary>
        /// Ends batching and invokes single callback if needed
        /// </summary>
        void EndBatchMode();

        /// <summary>
        /// True if object has unsaved changes
        /// </summary>
        bool AnyChange();

        /// <summary>
        /// Resets the changes flag
        /// </summary>
        void ResetChanges();

        void UpdateBatch();

    }
}