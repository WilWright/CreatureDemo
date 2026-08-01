using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Navigation;

public class ChunkManager
{
    readonly Dictionary<Coordinates3D, Chunk> _chunks = new();

    public ChunkConfig ChunkConfig { get; private set; }

    public ChunkManager(ChunkConfig chunkConfig)
    {
        ChunkConfig = chunkConfig;
    }

    public bool TryGetChunk(Vector3 worldPosition, out Chunk chunk)
    {
        var c = ChunkConfig.GetChunkCoordinates(worldPosition);
        return TryGetChunk(c, out chunk);
    }
    public bool TryGetChunk(Coordinates3D c, out Chunk chunk)
    {
        return _chunks.TryGetValue(c, out chunk);
    }

    public Chunk LoadChunk(Vector3 worldPosition)
    {
        var c = ChunkConfig.GetChunkCoordinates(worldPosition);
        if (_chunks.TryGetValue(c, out var chunk))
        {
            return chunk;
        }

        // TODO: Load scenes additively and get chunk from loaded scene
        chunk = Object.FindAnyObjectByType<Chunk>();

        _chunks.Add(c, chunk);
        chunk.Init(c, ChunkConfig);
        return chunk;
    }

    public bool TryGetNavigationDatas(Vector3 worldPosition, out NavigationData[] navigationDatas)
    {
        var c = ChunkConfig.GetChunkCoordinates(worldPosition);
        return TryGetNavigationDatas(c, out navigationDatas);
    }
    public bool TryGetNavigationDatas(Coordinates3D c, out NavigationData[] navigationDatas)
    {
        navigationDatas = null;
        if (TryGetChunk(c, out var chunk))
        {
            navigationDatas = chunk.NavigationDatas;
        }
        return navigationDatas != null;
    }
}
