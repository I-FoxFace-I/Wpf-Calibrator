using System;

namespace HierarchicalMvvm.Core
{

    /// <summary>
    /// Abstraktní base třída pro change tracking funkcionalitu
    /// </summary>
    public abstract class ChangeTrackerBase : IChangeTracker, IDisposable
    {
        private bool _disposed = false;
        private bool _batchMode = false;
        private bool _pendingChange = false;
        private bool _hasChanges = false;

        protected Action? _changeCallback;

        public bool AnyChange() { return _hasChanges; }

        public void SetCallback(Action? callback)
        {
            _changeCallback = callback;
        }

        public void RemoveChangeCallback()
        {
            _changeCallback = null;
        }

        public virtual void MarkChanged()
        {
            _hasChanges = true;

            if (_batchMode)
            {
                _pendingChange = true;
                return;
            }

            OnChangeDetected();
        }

        public void BeginBatchMode()
        {
            _batchMode = true;
            _pendingChange = false;
        }

        public void EndBatchMode()
        {
            _batchMode = false;
            if (_pendingChange)
            {
                OnChangeDetected();
                _pendingChange = false;
            }
        }

        public void UpdateBatch()
        {
            if (_hasChanges)
            {
                OnChangeDetected();

                _hasChanges = false;
                _pendingChange = false;
            }
        }

        public void ResetChanges()
        {
            _hasChanges = false;
        }

        /// <summary>
        /// Volá se při detekci změny - potomci mohou override pro custom logiku
        /// </summary>
        protected virtual void OnChangeDetected()
        {
            _changeCallback?.Invoke();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposed)
                    return;

                _changeCallback = null;
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}