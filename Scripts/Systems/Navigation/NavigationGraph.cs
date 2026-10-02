using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using Utils;

namespace Navigation
{
    public class NavigationGraph
    {
        readonly struct SerializedEdge
        {
            public readonly Coordinates3D Id;
            public readonly float Cost;

            public SerializedEdge(Coordinates3D id, float cost)
            {
                Id   = id;
                Cost = cost;
            }
        }

        public Graph<Coordinates3D, NavigationPoint> Graph { get; private set; }
        public Vector3       GraphOrigin { get; private set; }
        public Coordinates3D GraphBounds { get; private set; }
        public float         NodeSize    { get; private set; }

        const int CURRENT_SERIALIZATION_VERSION = 1;
        const string SERIALIZED_FILE_EXTENSION = ".navgraph";

        public NavigationGraph(Graph<Coordinates3D, NavigationPoint> graph, Vector3 graphOrigin, float nodeSize)
        {
            Graph       = graph;
            GraphOrigin = graphOrigin;
            NodeSize    = nodeSize;

            foreach (var node in graph)
            {
                GraphBounds = CoordinatesUtils.Max(GraphBounds, node.Id);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(CURRENT_SERIALIZATION_VERSION);

            writer.Write(Graph.NodeCount);
            foreach (var node in Graph)
            {
                writer.Write(node.Id);
                node.Data.Write(writer);

                writer.Write(node.EdgeCount);
                foreach (var edge in node)
                {
                    writer.Write(edge.Node.Id);
                    writer.Write(edge.Cost);
                }
            }

            writer.Write(GraphOrigin);
            writer.Write(NodeSize);
        }

        public static NavigationGraph Read(BinaryReader reader)
        {
            int version = reader.ReadInt32();

            var graph = new Graph<Coordinates3D, NavigationPoint>();
            var edges = new Dictionary<Coordinates3D, List<SerializedEdge>>();
            int nodeCount = reader.ReadInt32();
            for (int i = 0; i < nodeCount; i++)
            {
                var nodeId = reader.ReadCoordinates3D();
                var nodeData = NavigationPoint.Read(reader);
                graph.AddNewNodeOrGet(nodeId, nodeData);

                int edgeCount = reader.ReadInt32();
                if (edgeCount == 0)
                {
                    continue;
                }

                var edgeList = new List<SerializedEdge>();
                for (int j = 0; j < edgeCount; j++)
                {
                    var edgeId   = reader.ReadCoordinates3D();
                    int edgeCost = reader.ReadInt32();
                    edgeList.Add(new SerializedEdge(edgeId, edgeCost));
                }
                edges.Add(nodeId, edgeList);
            }

            foreach (var kvp in edges)
            {
                var nodeId   = kvp.Key;
                var edgeList = kvp.Value;

                graph.TryGetNode(nodeId, out var node);
                foreach (var edge in edgeList)
                {
                    graph.TryGetNode(edge.Id, out var edgeNode);
                    node.AddNewEdgeOrGet(edgeNode, edge.Cost);
                }
            }

            var graphOrigin = reader.ReadVector3();
            float nodeSize  = reader.ReadSingle();

            return new NavigationGraph(graph, graphOrigin, nodeSize);
        }

        public async Task<FileUtils.FileResult> Save(string path)
        {
            path += SERIALIZED_FILE_EXTENSION;
            return await FileUtils.WriteBinary(path, Write);
        }

        public static async Task<NavigationGraph> Load(string path)
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
