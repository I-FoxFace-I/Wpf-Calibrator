using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HierarchicalMvvm.Core
{
    /// <summary>
    /// Base třída pro observable objekty s change trackingem
    /// </summary>
    public abstract class TrackableObject : ObservableObject, ITrackableModel
    {


        protected Action? _changeCallback;
        protected IChangeTracker? _parent;

        protected bool _disposed = false;
        protected bool _batchMode = false;
        protected bool _hasChanges = false;
        protected bool _pendingChange = false;
        public IChangeTracker? GetParent() { return _parent; }
        public void RemoveChangeCallback() { _changeCallback = null; }
        public void SetParent(IChangeTracker? parent) { _parent = parent; }
        public void SetCallback(Action? callback) { _changeCallback = callback; }




        public bool HasChanges => _hasChanges;
        public bool AnyChange() { return _hasChanges; }
        protected TrackableObject()
        {
            PropertyChanged += OnInternalPropertyChanged;
        }

        public virtual void MarkChanged()
        {
            _hasChanges = true;

            if (_batchMode)
            {
                _pendingChange = true;
                
                return;
            }

            PropagateChange();
        }

        public void PropagateChange()
        {
            if (_parent != null)
            {
                _parent.MarkChanged();
            }
            else
            {
                // Jsme root - vyvoláme callback
                _changeCallback?.Invoke();
            }
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
                PropagateChange();
                _pendingChange = false;
            }
        }

        public void ResetChanges()
        {
            _hasChanges = false;
        }

        private void OnInternalPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            MarkChanged();
        }

        /// <summary>
        /// Utility metoda pro nastavování child objektů s automatickým parent assignment
        /// </summary>
        protected void SetChildProperty<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = "")
            where T : class?
        {
            if (!ReferenceEquals(backingField, newValue))
            {
                // Odpojit starý child
                if (backingField is IHierarchicalChangeTracker oldChild)
                {
                    oldChild.SetParent(null);
                }

                backingField = newValue;

                // Připojit nový child
                if (newValue is IHierarchicalChangeTracker newChild)
                {
                    newChild.SetParent(this);
                }

                base.OnPropertyChanged(propertyName);
            }
        }

        protected void OnPropertyChangedInternal([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        }


        protected virtual void StopTracking(bool disposing)
        {
            if (disposing)
            {
                if (_disposed)
                    return;

                _parent = null;
                _changeCallback = null;
                PropertyChanged -= OnInternalPropertyChanged;
            }
        }

        public virtual void Dispose()
        {
            StopTracking(true);
            GC.SuppressFinalize(this);
        }

        public void UpdateBatch()
        {
            if (_hasChanges)
            {
                _pendingChange = false;
                PropagateChange();
            }
        }

    }
}