using System;
using System.Threading.Tasks;
using UnityEngine;

using Utils;

namespace Navigation
{
    public class NavigationScan
    {
        [Serializable]
        public struct SerializedScan
        {
            [field: SerializeField] public Grid3D<NavigationPoint.SerializedNavigationPoint>.SerializedGrid Grid { get; private set; }
            [field: SerializeField] public Vector3 GridOrigin { get; private set; }
            [field: SerializeField] public float   NodeSize   { get; private set; }

            public SerializedScan(NavigationScan n)
            {
                var grid = n.Grid;
                var serializedGrid = new Grid3D<NavigationPoint.SerializedNavigationPoint>(grid.Bounds);
                foreach (var c in grid.Bounds.EnumerateFromZero())
                {
                    var point = grid[c];
                    if (point != null)
                    {
                        serializedGrid[c] = new NavigationPoint.SerializedNavigationPoint(grid[c]);
                    }
                }

                Grid       = serializedGrid.GetSerialized();
                GridOrigin = n.GridOrigin;
                NodeSize   = n.NodeSize;
            }
        }

        public Grid3D<NavigationPoint> Grid       { get; private set; }
        public Vector3                 GridOrigin { get; private set; }
        public float                   NodeSize   { get; private set; }

        public NavigationScan(Grid3D<NavigationPoint> grid, Vector3 gridOrigin, float nodeSize)
        {
            Grid       = grid;
            GridOrigin = gridOrigin;
            NodeSize   = nodeSize;
        }

        NavigationScan(SerializedScan s)
        {
            var grid = new Grid3D<NavigationPoint.SerializedNavigationPoint>(s.Grid);
            Grid = new Grid3D<NavigationPoint>(grid.Bounds);
            foreach (var c in grid.Bounds.EnumerateFromZero())
            {
                var point = grid[c];
                if (point != null)
                {
                    Grid[c] = new NavigationPoint(point);
                }
            }

            GridOrigin = s.GridOrigin;
            NodeSize   = s.NodeSize;
        }

        public SerializedScan GetSerialized()
        {
            return new SerializedScan(this);
        }

        public static async Task<NavigationScan> Load(string path)
        {
            var result = await FileUtils.LoadJson<SerializedScan>(path);
            if (result.IsSuccess == false)
            {
                SystemLog.Error("Scan does not exist: " + path);
                return null;
            }

            var scan = await Task.Run(() => new NavigationScan(result.Data));
            return scan;
        }

        public async Task Save(string path)
        {
            await FileUtils.SaveJson(path, GetSerialized());
        }
    }
}
