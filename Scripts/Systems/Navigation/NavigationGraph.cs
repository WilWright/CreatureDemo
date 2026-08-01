using System;
using System.Threading.Tasks;
using UnityEngine;

using Utils;

namespace Navigation
{
    public class NavigationGraph
    {
        [Serializable]
        public struct SerializedGraph
        {
            [field: SerializeField] public Graph<Coordinates3D, NavigationPoint.SerializedNavigationPoint>.SerializedGraph Graph { get; private set; }
            [field: SerializeField] public Vector3 GraphOrigin { get; private set; }
            [field: SerializeField] public float   NodeSize    { get; private set; }

            public SerializedGraph(NavigationGraph n)
            {
                var graph = n.Graph;
                var serializedGraph = new Graph<Coordinates3D, NavigationPoint.SerializedNavigationPoint>();
                foreach (var node in graph)
                {
                    serializedGraph.AddNewNodeOrGet(node.Id, new NavigationPoint.SerializedNavigationPoint(node.Data));
                }
                foreach (var node in graph)
                {
                    serializedGraph.TryGetNode(node.Id, out var sNode);
                    foreach (var edge in node)
                    {
                        serializedGraph.TryGetNode(edge.Node.Id, out var edgeNode);
                        sNode.AddNewEdgeOrGet(edgeNode, edge.Cost);
                    }
                }

                Graph       = serializedGraph.GetSerialized();
                GraphOrigin = n.GraphOrigin;
                NodeSize    = n.NodeSize;
            }
        }

        public Graph<Coordinates3D, NavigationPoint> Graph { get; private set; }
        public Vector3       GraphOrigin { get; private set; }
        public Coordinates3D GraphBounds { get; private set; }
        public float         NodeSize    { get; private set; }

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

        NavigationGraph(SerializedGraph s)
        {
            var graph = new Graph<Coordinates3D, NavigationPoint.SerializedNavigationPoint>(s.Graph);
            Graph = new Graph<Coordinates3D, NavigationPoint>();
            foreach (var node in graph)
            {
                Graph.AddNewNodeOrGet(node.Id, new NavigationPoint(node.Data));
                GraphBounds = CoordinatesUtils.Max(GraphBounds, node.Id);
            }
            foreach (var sNode in graph)
            {
                Graph.TryGetNode(sNode.Id, out var node);
                foreach (var edge in sNode)
                {
                    Graph.TryGetNode(edge.Node.Id, out var edgeNode);
                    node.AddNewEdgeOrGet(edgeNode, edge.Cost);
                }
            }

            GraphOrigin = s.GraphOrigin;
            NodeSize    = s.NodeSize;
        }

        public SerializedGraph GetSerialized()
        {
            return new SerializedGraph(this);
        }

        public static async Task<NavigationGraph> Load(string path)
        {
            var result = await FileUtils.LoadJson<SerializedGraph>(path);
            if (result.IsSuccess == false)
            {
                SystemLog.Error("Graph does not exist: " + path);
                return null;
            }

            var scan = await Task.Run(() => new NavigationGraph(result.Data));
            return scan;
        }

        public async Task Save(string path)
        {
            await FileUtils.SaveJson(path, GetSerialized());
        }
    }
}
