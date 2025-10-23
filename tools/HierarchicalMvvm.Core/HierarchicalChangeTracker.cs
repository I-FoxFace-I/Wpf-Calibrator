namespace HierarchicalMvvm.Core
{

    /// <summary>
    /// Konkrétní implementace hierarchického change trackingu
    /// </summary>
    public class HierarchicalChangeTracker : ChangeTrackerBase, IHierarchicalChangeTracker
    {
        private IChangeTracker? _parent;

        public void SetParent(IChangeTracker? parent)
        {
            _parent = parent;
        }

        public IChangeTracker? GetParent()
        {
            return _parent;
        }

        public void PropagateChange()
        {
            if (_parent != null)
            {
                _parent.MarkChanged();
            }
            else
            {
                // Jsme root - vyvoláme callback přímo z base třídy
                base.OnChangeDetected();
            }
        }

        protected override void OnChangeDetected()
        {
            PropagateChange();
        }
    }
}