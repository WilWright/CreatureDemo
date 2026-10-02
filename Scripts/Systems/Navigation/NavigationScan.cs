using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using Utils;

namespace Navigation
{
    public class NavigationScan
    {
        readonly struct SerializedElement
        {
            public readonly Coordinates3D Coordinates;
            public readonly NavigationPoint Point;

            public SerializedElement(Coordinates3D coordinates, NavigationPoint point)
            {
                Coordinates = coordinates;
                Point       = point;
            }
        }

        public Grid3D<NavigationPoint> Grid       { get; private set; }
        public Vector3                 GridOrigin { get; private set; }
        public float                   NodeSize   { get; private set; }

        const int CURRENT_SERIALIZATION_VERSION = 1;
        const string SERIALIZED_FILE_EXTENSION = ".navscan";

        public NavigationScan(Grid3D<NavigationPoint> grid, Vector3 gridOrigin, float nodeSize)
        {
            Grid       = grid;
            GridOrigin = gridOrigin;
            NodeSize   = nodeSize;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(CURRENT_SERIALIZATION_VERSION);

            writer.Write(GridOrigin);
            writer.Write(NodeSize);

            writer.Write(Grid.Bounds);
            foreach (var c in Grid.EnumerateBounds())
            {
                var point = Grid[c];
                if (point == null)
                {
                    continue;
                }

                writer.Write(c);
                point .Write(writer);
            }
        }

        public static NavigationScan Read(BinaryReader reader)
        {
            int version = reader.ReadInt32();

            var gridOrigin = reader.ReadVector3();
            float nodeSize = reader.ReadSingle();

            var bounds = reader.ReadCoordinates3D();
            var grid = new Grid3D<NavigationPoint>(bounds);
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var c = reader.ReadCoordinates3D();
                var point = NavigationPoint.Read(reader);
                grid[c] = point;
            }

            return new NavigationScan(grid, gridOrigin, nodeSize);
        }

        public async Task<FileUtils.FileResult> Save(string path)
        {
            path += SERIALIZED_FILE_EXTENSION;
            return await FileUtils.WriteBinary(path, Write);
        }

        public static async Task<NavigationScan> Load(string path)
        {
            path += SERIALIZED_FILE_EXTENSION;

            var result = await FileUtils.ReadBinary(path, Read);
            if (result.IsSuccess == false)
            {
                SystemLog.Error(result.FailMessage);
                return null;
            }

            return result.Data;
        }
    }
}
