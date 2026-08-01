using System;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class Graph<TId, TData>
{
    [Serializable]
    public struct SerializedGraph
    {
        [Serializable]
        public struct Node
        {
            [Serializable]
            public struct Edge
            {
                [field: SerializeField] public TId   Id   { get; private set; }
                [field: SerializeField] public float Cost { get; private set; }

                public Edge(TId id, float cost)
                {
                    Id   = id;
                    Cost = cost;
                }
            }

            [field: SerializeField] public TId    Id    { get; private set; }
            [field: SerializeField] public TData  Data  { get; private set; }
            [field: SerializeField] public Edge[] Edges { get; private set; }

            public Node(Graph<TId, TData>.Node node)
            {
                Id   = node.Id;
                Data = node.Data;

                var edges = new List<Edge>();
                foreach (var edge in node)
                {
                    edges.Add(new Edge(edge.Node.Id, edge.Cost));
                }
                Edges = edges.Count == 0 ? null : edges.ToArray();
            }
        }

        [field: SerializeField] public Node[] Nodes { get; private set; }

        public SerializedGraph(Graph<TId, TData> g)
        {
            var nodes = new List<Node>();
            foreach (var node in g)
            {
                nodes.Add(new Node(node));
            }
            Nodes = nodes.Count == 0 ? null : nodes.ToArray();
        }
    }

    [Serializable]
    public class Node
    {
        [Serializable]
        public class Edge
        {
            [field: SerializeField] public float Cost { get; set; }
            [field: SerializeField] public Node  Node { get; private set; }

            public Edge(Node node, float cost)
            {
                Node = node;
                Cost = cost;
            }
        }

        public TId Id { get; private set; }

        public TData Data { get; private set; }

        readonly List<Edge> _edges = new();

        public Node(TId id, TData data)
        {
            Id = id;
            Data = data;
        }

        public bool TryGetEdge(TId id, out Edge edge)
        {
            foreach (var e in _edges)
            {
                if (e.Node.Id.Equals(id))
                {
                    edge = e;
                    return true;
                }
            }

            edge = null;
            return false;
        }

        public Edge AddNewEdgeOrGet(Node node, float cost)
        {
            if (TryGetEdge(node.Id, out var edge))
            {
                return edge;
            }

            edge = new Edge(node, cost);
            _edges.Add(edge);
            return edge;
        }

        public void RemoveEdge(TId id)
        {
            for (int i = 0; i < _edges.Count; i++)
            {
                if (_edges[i].Node.Id.Equals(id))
                {
                    _edges.RemoveAt(i);
                    return;
                }
            }
        }

        public bool TryGetReverseEdge(Edge edge, out Edge reverseEdge)
        {
            return edge.Node.TryGetEdge(Id, out reverseEdge);
        }

        public List<Edge>.Enumerator GetEnumerator() => _edges.GetEnumerator();

        public override string ToString()
        {
            var builder = new CollectionStringBuilder();
            foreach (var edge in this)
            {
                builder.Append($"{edge.Node.Id}{{{edge.Cost}}}");
            }
            return $"{Id} -> {builder.Build()}";
        }
    }

    readonly Dictionary<TId, Node> _nodes = new();

    public Graph() {}

    public Graph(SerializedGraph s)
    {
        if (s.Nodes == null)
        {
            return;
        }

        foreach (var node in s.Nodes)
        {
            AddNewNodeOrGet(node.Id, node.Data);
        }
        foreach (var node in s.Nodes)
        {
            if (node.Edges == null)
            {
                continue;
            }

            TryGetNode(node.Id, out var n);
            foreach (var edge in node.Edges)
            {
                TryGetNode(edge.Id, out var e);
                n.AddNewEdgeOrGet(e, edge.Cost);
            }
        }
    }

    public SerializedGraph GetSerialized()
    {
        return new SerializedGraph(this);
    }

    public bool TryGetNode(TId id, out Node node)
    {
        return _nodes.TryGetValue(id, out node);
    }

    public Node AddNewNodeOrGet(TId id, TData data)
    {
        if (TryGetNode(id, out var node))
        {
            return node;
        }

        node = new Node(id, data);
        _nodes.Add(id, node);
        return node;
    }

    public void RemoveNode(TId id)
    {
        if (_nodes.Remove(id) == false)
        {
            return;
        }

        foreach (var node in _nodes.Values)
        {
            node.RemoveEdge(id);
        }
    }

    public Dictionary<TId, Node>.ValueCollection.Enumerator GetEnumerator() => _nodes.Values.GetEnumerator();

    public override string ToString()
    {
        var builder = new CollectionStringBuilder("\n");
        foreach (var node in this)
        {
            builder.Append(node);
        }
        return builder.Build();
    }
}
