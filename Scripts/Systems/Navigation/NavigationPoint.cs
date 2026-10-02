using System.IO;
using UnityEngine;

using Utils;

namespace Navigation
{
    public class NavigationPoint
    {
        public NavigationTerrain      Terrain    { get; private set; }
        public SerializableRaycastHit Hit        { get; private set; }
        public Vector3                Position   { get; private set; }
        public bool                   IsRestable { get; private set; }
        public bool                   IsLedge    { get; private set; }

        SerializableGameObject.SerializedId _terrainId;

        const int CURRENT_SERIALIZATION_VERSION = 1;

        public NavigationPoint(NavigationTerrain terrain, RaycastHit hit, Vector3 position, bool isRestable, bool isLedge)
            : this(new SerializableGameObject.SerializedId(terrain), terrain, new SerializableRaycastHit(hit), position, isRestable, isLedge) {}

        public NavigationPoint(NavigationTerrain terrain, SerializableRaycastHit hit, Vector3 position, bool isRestable, bool isLedge)
            : this(new SerializableGameObject.SerializedId(terrain), terrain, hit, position, isRestable, isLedge) { }

        public NavigationPoint(SerializableGameObject.SerializedId terrainId, NavigationTerrain terrain, SerializableRaycastHit hit, Vector3 position, bool isRestable, bool isLedge)
        {
            Terrain    = terrain;
            Hit        = hit;
            Position   = position;
            IsRestable = isRestable;
            IsLedge    = isLedge;

            _terrainId = terrainId;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(CURRENT_SERIALIZATION_VERSION);

            Hit   .Write(writer);
            writer.Write(Position);
            writer.Write(IsRestable);
            writer.Write(IsLedge);

            _terrainId.Write(writer);
        }

        public static NavigationPoint Read(BinaryReader reader)
        {
            int version = reader.ReadInt32();

            var hit = SerializableRaycastHit.Read(reader);
            var position    = reader.ReadVector3();
            bool isRestable = reader.ReadBoolean();
            bool isLedge    = reader.ReadBoolean();

            var terrainId = SerializableGameObject.SerializedId.Read(reader);
            var terrain = terrainId.GetFromContext<NavigationTerrain>();

            return new NavigationPoint(terrainId, terrain, hit, position, isRestable, isLedge);
        }
    }
}
