using System.Collections;

namespace HierarchicalMvvm.Core
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;


    /// <summary>
    /// Collection třída s automatickým parent/child managementem a change trackingem
    /// </summary>
    public class TrackableCollection<T> : TrackableObject, ICollection<T>, INotifyCollectionChanged, IList<T>
        where T : class
    {
        private readonly ObservableCollection<T> _items = new();

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add => _items.CollectionChanged += value;
            remove => _items.CollectionChanged -= value;
        }

        public TrackableCollection(IChangeTracker? parent = null)
        {
            _parent = parent;
            _items.CollectionChanged += OnCollectionChanged;
        }

        public TrackableCollection(IEnumerable<T> items, IChangeTracker? parent = null)
        {
            _parent = parent;
            _items.CollectionChanged += OnCollectionChanged;

            if (items != null && items.Any())
                AddRange(items);
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Handle parent assignment
            if (e.OldItems != null)
            {
                foreach (T item in e.OldItems)
                {
                    if (item is ITrackableModel trackable)
                    {
                        trackable.SetParent(null);
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (T item in e.NewItems)
                {
                    if (item is ITrackableModel trackable)
                    {
                        trackable.SetParent(this);
                    }
                }
            }

            MarkChanged();
        }

        // ICollection<T> implementation
        public void Add(T item)
        {
            _items.Add(item);
        }

        public bool Remove(T item) => _items.Remove(item);

        public void Clear()
        {
            // Manually clear parent before clearing collection
            // because ObservableCollection.Clear() sends Reset event with OldItems = null
            foreach (T item in _items)
            {
                if (item is ITrackableModel trackable)
                {
                    trackable.SetParent(null);
                }
            }
            _items.Clear();
        }

        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // IList<T> implementation
        public int IndexOf(T item) => _items.IndexOf(item);

        public void Insert(int index, T item) => _items.Insert(index, item);

        public void RemoveAt(int index) => _items.RemoveAt(index);

        public T this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        /// <summary>
        /// Batch operation pro přidání více items najednou
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            BeginBatchMode();
            try
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
            finally
            {
                EndBatchMode();
            }
        }
    }
}