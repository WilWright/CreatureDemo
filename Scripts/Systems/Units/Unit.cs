using System;
using System.Threading;
using UnityEngine;

using Navigation;

public class Unit : MonoBehaviour
{
    [field: SerializeField] public UnitType UnitType { get; private set; }

    public Coordinates3D SpatialCoordinates { get; private set; } = Chunk.UNREGISTERED_COORDINATES;

    Chunk _currentChunk;

    void Start()
    {
        // TODO: Pool units and spawn from spawner or save data to init
        UpdateChunk();
    }

    void OnDestroy()
    {
        RemoveFromChunk();
    }

    void Update()
    {
        UpdateChunk();
    }

    public CancellationTokenSource RequestNavigationPath(Vector3 to, Action<NavigationPath> onReady)
    {
        if (_currentChunk == null)
        {
            onReady(null);
            return null;
        }

        return _currentChunk.RequestNavigationPath(this, to, onReady);
    }

    public void RemoveFromChunk()
    {
        if (_currentChunk != null)
        {
            _currentChunk.RemoveUnit(this);
            _currentChunk = null;
        }

        SpatialCoordinates = Chunk.UNREGISTERED_COORDINATES;
    }

    void UpdateChunk()
    {
        Coordinates3D spatialCoordinates;

        if (_currentChunk == null)
        {
            if (GameController.ChunkManager.TryGetChunk(transform.position, out var chunk) == false)
            {
                return;
            }

            _currentChunk = chunk;
            _currentChunk.AddUnit(this, out spatialCoordinates);

            SpatialCoordinates = spatialCoordinates;
            return;
        }

        bool retry = _currentChunk.UpdateUnit(this, out spatialCoordinates) == false;

        SpatialCoordinates = spatialCoordinates;

        if (retry)
        {
            _currentChunk = null;
            UpdateChunk();
        }
    }
}
