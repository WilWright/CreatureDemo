using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Utils;

public class GameController : MonoBehaviour
{
    [SerializeField] List<UnitType> _registerUnitTypes;

    [SerializeField] ChunkConfig _chunkConfig;

    public static ChunkManager ChunkManager { get; private set; }

    public static UnityEvent OnMainThreadUpdate = new();

    void Awake()
    {
        UnitType.ClearRegistry();
        foreach (var unitType in _registerUnitTypes)
        {
            UnitType.Register(unitType);
        }

        ChunkManager = new ChunkManager(_chunkConfig);

        var player = GameObject.FindGameObjectWithTag("Player");
        ChunkManager.LoadChunk(player.transform.position);
    }

    private void Update()
    {
        OnMainThreadUpdate.Invoke();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_chunkConfig != null)
        {
            var displayChunkSize = Coordinates3D.ONE * 5;
            var displayCellSize  = Coordinates3D.ONE * _chunkConfig.CellsPerChunkWidth;
            float cellSize = _chunkConfig.CellSize;
            var endOffset = Vector3.up * 100;
            foreach (var chunk in displayChunkSize.EnumerateFromZero())
            {
                if (chunk.y > 0)
                {
                    break;
                }

                var chunkPos = chunk.ToVector3() * _chunkConfig.ChunkSize;

                Gizmos.color = ColorUtils.GREEN;
                foreach (var cell in displayCellSize.EnumerateFromZero())
                {
                    if (cell.y > 0)
                    {
                        break;
                    }

                    var cellPos = chunkPos + cell.ToVector3() * cellSize;
                    Gizmos.DrawLine(cellPos, cellPos + endOffset);
                }

                Gizmos.color = ColorUtils.WHITE;
                Gizmos.DrawLine(chunkPos, chunkPos + endOffset);
            }
        }
    }
#endif
}
