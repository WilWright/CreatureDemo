namespace Navigation
{
    public class NavigationNode : IHeapElement<NavigationNode, float>
    {
        public NavigationNode Parent { get; set; }

        public bool IsClosed { get; set; }

        public int HeapIndex { get; set; } = -1;

        public float F => _f;

        public float G
        {
            get => _g;
            set
            {
                _g = value;
                CacheF();
            }
        }

        public float H
        {
            get => _h;
            set
            {
                _h = value;
                CacheF();
            }
        }

        public Coordinates3D Id { get; private set; }

        public NavigationPoint Point { get; private set; }

        public int SearchIndex { get; private set; }

        float _f;
        float _g;
        float _h;
        void CacheF() { _f = _g + _h; }

        public NavigationNode(Coordinates3D id, NavigationPoint point)
        {
            Id = id;
            Point = point;
        }

        public void InitSearch(int searchIndex, bool isStart = false)
        {
            SearchIndex = searchIndex;
            HeapIndex = -1;
            IsClosed = false;
            G = isStart ? 0 : int.MaxValue;
            H = 0;
            Parent = null;
        }

        public float GetHeapValue() => _f;

        public int CompareTo(NavigationNode other)
        {
            int compare = _f.CompareTo(other._f);
            return compare == 0 ? _h.CompareTo(other._h) : compare;
        }
    }
}
