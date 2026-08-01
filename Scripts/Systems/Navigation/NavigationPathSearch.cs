using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

using Utils;

namespace Navigation
{
    public class NavigationPathSearch
    {
        readonly struct SearchRequest
        {
            public bool IsCancelled => CancellationTokenSource.IsCancellationRequested;

            public readonly Action<NavigationPath> OnReady;

            public readonly Graph<Coordinates3D, NavigationNode>.Node From;
            public readonly Graph<Coordinates3D, NavigationNode>.Node To;

            public readonly CancellationTokenSource CancellationTokenSource;

            public SearchRequest
            (
                Action<NavigationPath> onReady,
                Graph<Coordinates3D, NavigationNode>.Node from,
                Graph<Coordinates3D, NavigationNode>.Node to
            )
            {
                OnReady = onReady;

                From = from;
                To   = to;

                CancellationTokenSource = new CancellationTokenSource();
            }
        }

        Graph<Coordinates3D, NavigationNode> _searchGraph;

        readonly BlockingCollection<SearchRequest> _searchRequests = new();
        Thread _initThread;
        Thread _searchThread;

        readonly MinHeap<NavigationNode, float> _openNodes = new();

        readonly Vector3 _graphOrigin;
        readonly float _nodeSize;

        int _currentSearchIndex;

        bool _unloaded;

        public NavigationPathSearch(NavigationGraph graph)
        {
            _graphOrigin = graph.GraphOrigin;
            _nodeSize    = graph.NodeSize;

            _initThread = new Thread(() =>
            {
                _searchGraph = new Graph<Coordinates3D, NavigationNode>();

                foreach (var node in graph.Graph)
                {
                    var n = new NavigationNode(node.Id, node.Data);
                    var searchNode = _searchGraph.AddNewNodeOrGet(node.Id, n);

                    foreach (var edge in node)
                    {
                        var e = new NavigationNode(edge.Node.Id, edge.Node.Data);
                        searchNode.AddNewEdgeOrGet(_searchGraph.AddNewNodeOrGet(e.Id, e), edge.Cost);
                    }
                }

                _searchThread = new Thread(ProcessSearchThread);
                _searchThread.Start();
            });

            _initThread.Start();
        }

        public void Unload()
        {
            _unloaded = true;

            _searchRequests.CompleteAdding();

            foreach (var request in _searchRequests)
            {
                request.CancellationTokenSource.Cancel();
            }

            _initThread  .Join();
            _searchThread.Join();
        }

        public CancellationTokenSource RequestNavigationPath(Vector3 from, Vector3 to, Action<NavigationPath> onReady)
        {
            return RequestNavigationPath(GetClosestNodeId(from), GetClosestNodeId(to), onReady);
        }

        public CancellationTokenSource RequestNavigationPath(Coordinates3D from, Coordinates3D to, Action<NavigationPath> onReady)
        {
            if (_unloaded)
            {
                return null;
            }

            if (_searchGraph.TryGetNode(from, out var fromNode) == false)
            {
                onReady(null);
                return null;
            }

            if (_searchGraph.TryGetNode(to, out var toNode) == false)
            {
                onReady(null);
                return null;
            }

            if (from == to)
            {
                onReady(new NavigationPath(fromNode.Data));
                return null;
            }

            var request = new SearchRequest(onReady, fromNode, toNode);
            _searchRequests.Add(request);

            return request.CancellationTokenSource;
        }

        void ProcessSearchThread()
        {
            foreach (var request in _searchRequests.GetConsumingEnumerable())
            {
                if (request.IsCancelled)
                {
                    continue;
                }

                GetNavigationPath(request);
            }
        }

        Coordinates3D GetNodeId(Vector3 worldPosition) => NavigationMap.GetNodeId(worldPosition, _graphOrigin, _nodeSize);

        Coordinates3D GetClosestNodeId(Vector3 worldPosition, int checkClosestCoordinatesRadius = 10)
        {
            var c = GetNodeId(worldPosition);
            if (_searchGraph.TryGetNode(c, out var closestNode))
            {
                return c;
            }

            for (int i = 1; i <= checkClosestCoordinatesRadius; i++)
            {
                float closestDistance = float.MaxValue;
                foreach (var check in c.EnumerateRadiusAsCubeShell(i))
                {
                    if (_searchGraph.TryGetNode(check, out var node) == false)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(worldPosition, node.Data.Point.Position);
                    if (distance < closestDistance)
                    {
                        closestNode = node;
                        closestDistance = distance;
                    }
                }
                if (closestNode != null)
                {
                    return closestNode.Id;
                }
            }

            return c;
        }

        void GetNavigationPath(SearchRequest searchRequest)
        {
            if (searchRequest.IsCancelled)
            {
                return;
            }

            var from = searchRequest.From;
            var to   = searchRequest.To;

            _openNodes.Clear();

            _currentSearchIndex++;

            from.Data.InitSearch(_currentSearchIndex, true);

            _openNodes.Insert(from.Data);

            while (_openNodes.Count > 0)
            {
                if (searchRequest.IsCancelled)
                {
                    return;
                }

                var next = _openNodes.Pop();

                if (next == to.Data)
                {
                    searchRequest.OnReady(new NavigationPath(next));
                    return;
                }

                next.IsClosed = true;

                if (_searchGraph.TryGetNode(next.Id, out var node) == false)
                {
                    continue;
                }

                foreach (var edge in node)
                {
                    var e = edge.Node.Data;
                    if (e == null)
                    {
                        continue;
                    }

                    if (e.SearchIndex != _currentSearchIndex)
                    {
                        e.InitSearch(_currentSearchIndex);
                    }

                    if (e.IsClosed)
                    {
                        continue;
                    }

                    float g = next.G + edge.Cost;
                    if (g < e.G || e.HeapIndex == -1)
                    {
                        e.G = g;
                        e.H = GetHeuristic(e.Point.Position, to.Data.Point.Position);
                        e.Parent = next;

                        if (e.HeapIndex == -1)
                        {
                            _openNodes.Insert(e);
                        }
                        else
                        {
                            _openNodes.SortUp(e);
                        }
                    }
                }
            }

            searchRequest.OnReady(null);
        }

        float GetHeuristic(Vector3 from, Vector3 to)
        {
            float dx = from.x - to.x;
            float dy = from.y - to.y;
            float dz = from.z - to.z;
            return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
