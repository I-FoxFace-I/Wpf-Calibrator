namespace HierarchicalMvvm.Core
{
    /// <summary>
    /// Rozhraní pro objekty, které mohou mít parent pro change tracking
    /// </summary>
    public interface IHierarchicalChangeTracker : IChangeTracker
    {
        /// <summary>
        /// Sets the parent change tracker (null = root)
        /// </summary>
        void SetParent(IChangeTracker? parent);

        /// <summary>
        /// Gets the parent change tracker (null = root)
        /// </summary>
        IChangeTracker? GetParent();

        /// <summary>
        /// Propagates change to parent if exists, otherwise invokes local callback
        /// </summary>
        void PropagateChange();
    }
}