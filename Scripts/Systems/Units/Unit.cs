using UnityEngine;

public class Unit : MonoBehaviour
{
    [field: SerializeField] public UnitType UnitType { get; private set; }

    public Coordinates3D SpatialCoordinates { get; set; } = Chunk.UNREGISTERED_COORDINATES;

    Chunk _currentChunk;

    void Start()
    {
        // TODO: Pool units and spawn from spawner or save data to init
        var c = GameController.ChunkManager.ChunkConfig.GetChunkCoordinates(transform.position);
        if (GameController.ChunkManager.TryGetChunk(c, out var chunk))
        {
            Init(chunk);
        }
    }

    void Update()
    {
        _currentChunk.UpdateUnit(this);
    }

    public void Init(Chunk currentChunk)
    {
        _currentChunk = currentChunk;
        _currentChunk.SetUnit(this);
    }

    private void OnDestroy()
    {
        _currentChunk.RemoveUnit(this);
    }
}
