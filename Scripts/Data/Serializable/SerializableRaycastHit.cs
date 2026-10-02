using System.IO;
using UnityEngine;

using Utils;

public class SerializableRaycastHit
{
    public Collider Collider { get; private set; }
    public Vector3  Point    { get; private set; }
    public Vector3  Normal   { get; private set; }
    public float    Distance { get; private set; }

    SerializableGameObject.SerializedId _colliderId;

    const int CURRENT_SERIALIZATION_VERSION = 1;

    public SerializableRaycastHit(RaycastHit hit)
    {
        Collider = hit.collider;
        Point    = hit.point;
        Normal   = hit.normal;
        Distance = hit.distance;

        _colliderId = new SerializableGameObject.SerializedId(Collider);
    }

    public SerializableRaycastHit(SerializableGameObject.SerializedId colliderId, Collider collider, Vector3 point, Vector3 normal, float distance)
    {
        Collider = collider;
        Point    = point;
        Normal   = normal;
        Distance = distance;

        _colliderId = colliderId;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(CURRENT_SERIALIZATION_VERSION);

        writer.Write(Point);
        writer.Write(Normal);
        writer.Write(Distance);

        _colliderId.Write(writer);
    }

    public static SerializableRaycastHit Read(BinaryReader reader)
    {
        int version = reader.ReadInt32();

        var point    = reader.ReadVector3();
        var normal   = reader.ReadVector3();
        var distance = reader.ReadSingle();

        var colliderId = SerializableGameObject.SerializedId.Read(reader);
        var collider = colliderId.GetFromContext<Collider>();

        return new SerializableRaycastHit(colliderId, collider, point, normal, distance);
    }
}
