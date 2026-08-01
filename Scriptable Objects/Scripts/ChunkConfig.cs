using UnityEngine;

using Utils;

[CreateAssetMenu(fileName = "Chunk Config", menuName = "Scriptable Objects/Chunk Config")]
public class ChunkConfig : ScriptableObject
{
    [field: SerializeField] public float ChunkSize          { get; private set; }
    [field: SerializeField] public int   CellsPerChunkWidth { get; private set; }

    public float CellSize => ChunkSize / CellsPerChunkWidth;

    public Vector3 GetChunkPosition(Coordinates3D c)
    {
        c.y = 0;
        return c.ToVector3() * ChunkSize;
    }

    public Coordinates3D GetChunkCoordinates(Vector3 worldPosition)
    {
        worldPosition.y = 0;
        return CoordinatesUtils.FromVector3Floor(worldPosition / ChunkSize);
    }

    public static string GetChunkLabel(Coordinates3D chunkCoordinates)
    {
        return $"Chunk_{chunkCoordinates.x}_{chunkCoordinates.z}";
    }
}
