namespace Utils
{
    public static class NavigationUtils
    {
        public static Graph<Coordinates3D, T> ToGraph<T>(this Grid3D<T> grid, bool edgesAreAdjacent = false, bool cornersAreAdjacent = false)
        {
            var graph = new Graph<Coordinates3D, T>();

            foreach (var c in grid.Bounds.EnumerateFromZero())
            {
                var node = grid[c];
                if (node == null)
                {
                    continue;
                }

                var graphNode = graph.AddNewNodeOrGet(c, node);

                foreach (var direction in Coordinates3D.COMPASS_DIRECTIONS)
                {
                    TryAddEdge(c + direction);
                }

                if (edgesAreAdjacent)
                {
                    foreach (var direction in Coordinates3D.EDGE_DIRECTIONS)
                    {
                        TryAddEdge(c + direction);
                    }
                }

                if (cornersAreAdjacent)
                {
                    foreach (var direction in Coordinates3D.CORNER_DIRECTIONS)
                    {
                        TryAddEdge(c + direction);
                    }
                }

                void TryAddEdge(Coordinates3D e)
                {
                    if (grid.IsWithinBounds(e) == false)
                    {
                        return;
                    }

                    var adjNode = grid[e];
                    if (adjNode == null)
                    {
                        return;
                    }

                    var edgeNode = graph.AddNewNodeOrGet(e, adjNode);
                    graphNode.AddNewEdgeOrGet(edgeNode, 1);
                    edgeNode.AddNewEdgeOrGet(graphNode, 1);
                }
            }

            return graph;
        }

        public static Grid3D<T> ToGrid<T>(this Graph<Coordinates3D, T> graph)
        {
            var bounds = Coordinates3D.ZERO;
            foreach (var node in graph)
            {
                bounds = CoordinatesUtils.Max(bounds, node.Id);
            }

            var grid = new Grid3D<T>(bounds);
            foreach (var node in graph)
            {
                grid[node.Id] = node.Data;
            }

            return grid;
        }
    }
}
