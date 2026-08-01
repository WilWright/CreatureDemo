using UnityEngine;

public class SerializableRaycastHit
{
    [System.Serializable]
    public struct SerializedHit
    {
        [field: SerializeField] public SerializableGameObject.SerializedId ColliderId { get; private set; }
        [field: SerializeField] public Vector3 Point    { get; private set; }
        [field: SerializeField] public Vector3 Normal   { get; private set; }
        [field: SerializeField] public float   Distance { get; private set; }

        public SerializedHit(SerializableRaycastHit s)
        {
            ColliderId = new SerializableGameObject.SerializedId(s.Collider);
            Point      = s.Point;
            Normal     = s.Normal;
            Distance   = s.Distance;
        }
    }

    public Collider Collider { get; private set; }
    public Vector3  Point    { get; private set; }
    public Vector3  Normal   { get; private set; }
    public float    Distance { get; private set; }

    public SerializableRaycastHit(RaycastHit hit)
    {
        Collider = hit.collider;
        Point    = hit.point;
        Normal   = hit.normal;
        Distance = hit.distance;
    }

    public SerializableRaycastHit(SerializedHit s)
    {
        Collider = s.ColliderId.GetFromContext<Collider>();
        Point    = s.Point;
        Normal   = s.Normal;
        Distance = s.Distance;
    }

    public SerializedHit GetSerialized()
    {
        return new SerializedHit(this);
    }
}
