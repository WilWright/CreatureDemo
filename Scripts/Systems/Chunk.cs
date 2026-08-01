using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using Navigation;
using Utils;

public class Chunk : MonoBehaviour
{
    public enum ChunkDataType
    {
        NavigationScan,
        NavigationGraph
    }

    class SpatialCell
    {
        public List<Unit> Units { get; private set; } = new(16);
    }

    enum MapLoadStatus
    {
        NotLoaded,
        Loading,
        Loaded
    }

    [field: SerializeField] public ChunkConfig Config { get; private set; }

    [field: SerializeField] Transform _navigationDataHolder;

    public Coordinates3D ChunkId => Config == null ? UNREGISTERED_COORDINATES : Config.GetChunkCoordinates(transform.position);

    public NavigationData[] NavigationDatas { get; private set; }

    Grid3D<SpatialCell> _spatialGrid;

    NavigationMap[] _navigationMaps;
    MapLoadStatus[] _mapLoadStatuses;

    public static readonly Coordinates3D UNREGISTERED_COORDINATES = Coordinates3D.ONE * -1;

    public void Init(Coordinates3D c, ChunkConfig config)
    {
        SerializableGameObject.SerializedId.InitContext(transform);

        Config = config;

        transform.position = Config.GetChunkPosition(c);

        var gridBounds = CoordinatesUtils.FromVector3Ceil(Vector3.one * Config.CellSize);
        gridBounds.y = 0;
        _spatialGrid = new Grid3D<SpatialCell>(gridBounds);

        _navigationMaps  = new NavigationMap[UnitType.UnitTypeCount];
        _mapLoadStatuses = new MapLoadStatus[UnitType.UnitTypeCount];

        if (_navigationDataHolder != null)
        {
            NavigationDatas = _navigationDataHolder.GetComponentsInChildren<NavigationData>();
        }
    }

    public void Unload()
    {
        foreach (var map in _navigationMaps)
        {
            map?.Unload();
        }
    }

    public void SetUnit(Unit unit)
    {
        InitNavigationMap(unit.UnitType);

        var c = GetSpatialCoordinates(unit.transform.position);
        unit.SpatialCoordinates = c;
        AddUnit(c, unit);
    }

    public void UpdateUnit(Unit unit)
    {
        RemoveUnit(unit);
        SetUnit(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        RemoveUnit(unit.SpatialCoordinates, unit);
        unit.SpatialCoordinates = UNREGISTERED_COORDINATES;
    }

    public CancellationTokenSource RequestNavigationPath(NavigationUnit unit, Vector3 to, Action<NavigationPath> onReady)
    {
        var map = _navigationMaps[(int)unit.UnitType.Id];
        if (map == null)
        {
            onReady(null);
            SystemLog.Warn($"{unit.gameObject.name}({unit.UnitType}) navigation map not ready");
            return null;
        }

        return map.RequestNavigationPath(unit.transform.position, to, onReady);
    }

    void AddUnit(Coordinates3D c, Unit unit)
    {
        var cell = _spatialGrid[c];
        if (cell == null)
        {
            cell = new SpatialCell();
            _spatialGrid[c] = cell;
        }

        cell.Units.Add(unit);
    }

    void RemoveUnit(Coordinates3D c, Unit unit)
    {
        var cell = _spatialGrid[c];
        if (cell == null)
        {
            return;
        }

        cell.Units.Remove(unit);
    }

    Coordinates3D GetSpatialCoordinates(Vector3 worldPosition)
    {
        worldPosition.y = 0;
        var c = CoordinatesUtils.FromVector3Floor((worldPosition - transform.position) / Config.CellSize);
        if (_spatialGrid.IsWithinBounds(c))
        {
            return c;
        }

        // TODO: Move to another chunk if applicable

        return UNREGISTERED_COORDINATES;
    }

    async void InitNavigationMap(UnitType unitType)
    {
        int index = (int)unitType.Id;

        switch (_mapLoadStatuses[index])
        {
            case MapLoadStatus.NotLoaded:
                _mapLoadStatuses[index] = MapLoadStatus.Loading;

                var graph = await NavigationGraph.Load(GetNavigationGraphPath(unitType));
                var map = new NavigationMap(graph);
                _navigationMaps[index] = map;

                _mapLoadStatuses[index] = MapLoadStatus.Loaded;
                break;

            case MapLoadStatus.Loading:
                return;

            case MapLoadStatus.Loaded:
                return;
        }
    }

    public string GetNavigationScanPath() => GetChunkDataPath("NavigationScan");

    public string GetNavigationGraphPath(UnitType unitType) => GetChunkDataPath("NavigationGraph", unitType.name);

    string GetChunkDataPath(string fileName, string context = null)
    {
        if (context != null)
        {
            fileName += $"_{context}";
        }

        return FileUtils.GetStreamingAssetsPath("Chunk Data", ChunkConfig.GetChunkLabel(ChunkId), $"{fileName}.json");
    }
}
 
