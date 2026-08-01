using UnityEngine;

namespace Navigation
{
    public class NavigationPoint
    {
        [System.Serializable]
        public class SerializedNavigationPoint
        {
            [field: SerializeField] public SerializableGameObject.SerializedId  TerrainId  { get; private set; }
            [field: SerializeField] public SerializableRaycastHit.SerializedHit Hit        { get; private set; }
            [field: SerializeField] public Vector3                              Position   { get; private set; }
            [field: SerializeField] public bool                                 IsRestable { get; private set; }
            [field: SerializeField] public bool                                 IsLedge    { get; private set; }

            public SerializedNavigationPoint(NavigationPoint n)
            {
                TerrainId  = new SerializableGameObject.SerializedId(n.Terrain);
                Position   = n.Position;
                Hit        = n.Hit.GetSerialized();
                IsRestable = n.IsRestable;
                IsLedge    = n.IsLedge;
            }
        }

        public NavigationTerrain      Terrain    { get; private set; }
        public SerializableRaycastHit Hit        { get; private set; }
        public Vector3                Position   { get; private set; }
        public bool                   IsRestable { get; private set; }
        public bool                   IsLedge    { get; private set; }

        public NavigationPoint(NavigationTerrain terrain, RaycastHit hit, Vector3 position, bool isRestable, bool isLedge)
            : this(terrain, new SerializableRaycastHit(hit), position, isRestable, isLedge) {}

        public NavigationPoint(NavigationTerrain terrain, SerializableRaycastHit hit, Vector3 position, bool isRestable, bool isLedge)
        {
            Terrain    = terrain;
            Hit        = hit;
            Position   = position;
            IsRestable = isRestable;
            IsLedge    = isLedge;
        }

        public NavigationPoint(SerializedNavigationPoint s)
            : this(s.TerrainId.GetFromContext<NavigationTerrain>(), new SerializableRaycastHit(s.Hit), s.Position, s.IsRestable, s.IsLedge) {}

        public SerializedNavigationPoint GetSerialized()
        {
            return new SerializedNavigationPoint(this);
        }
    }
}
