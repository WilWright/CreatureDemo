using System;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class Graph<TId, TData>
{
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

        public int EdgeCount => _edges.Count;

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

        public void RemoveEdge(TId id, bool removeReverseEdge = true)
        {
            for (int i = 0; i < _edges.Count; i++)
            {
                var edge = _edges[i];
                if (edge.Node.Id.Equals(id))
                {
                    if (removeReverseEdge)
                    {
                        edge.Node.RemoveEdge(Id, false);
                    }

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

    public int NodeCount => _nodes.Count;

    readonly Dictionary<TId, Node> _nodes = new();

    public Graph() {}

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
