using System.ComponentModel;

namespace HierarchicalMvvm.Core
{

    /// <summary>
    /// Kombinované rozhraní pro modely s change trackingem
    /// </summary>
    public interface ITrackableModel : INotifyPropertyChanged, IHierarchicalChangeTracker
    {
        // Kombinuje základní model s change trackingem
    }
}