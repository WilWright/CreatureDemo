#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

using Utils;

namespace Navigation
{
    public class TerrainScanner : MonoBehaviour
    {
        [SerializeField] float _scanSize;
        [SerializeField] float _scanCeiling;
        [SerializeField] float _scanNodeSize;
        [SerializeField] float _minimumCollisionClearance;
        [SerializeField] Chunk _chunk;

        [MethodButton(nameof(CopyChunkConfig))]
        [SerializeField] MethodButton m_0;

        [Tooltip("Saves to and reads from test data so chunk data isn't overwritten")]
        [SerializeField] bool _testMode;

        [MethodButton(nameof(BakeNavigationScan))]
        [SerializeField, Space(20)] MethodButton m_1;
        [SerializeField] string _lastChunkScanBake;
        [HideInInspector, SerializeField] Debugging.SceneDebug _scanDebug;

        [MethodButton(nameof(LoadScanDebug))]
        [SerializeField] MethodButton m_2;
        [SerializeField] HideFlags _scanDebugHideFlags;
        [SerializeField] bool _scanDebugDisplayNodeSize;
        [SerializeField] bool _scanDebugDisplayNormals;

        [SerializeField, Space(20)] NavigationUnitConfig _graphUnitConfig;

        [MethodButton(nameof(BakeNavigationGraph))]
        [SerializeField] MethodButton m_3;
        [SerializeField] string _lastChunkGraphBake;
        [HideInInspector, SerializeField] Debugging.SceneDebug _graphDebug;

        [MethodButton(nameof(LoadGraphDebug))]
        [SerializeField] MethodButton m_4;
        [SerializeField] HideFlags _graphDebugHideFlags;
        [SerializeField] bool _graphDebugDisplayEdges;
        [SerializeField] bool _graphDebugDisplayNormals;

        void CopyChunkConfig()
        {
            var config = _chunk?.Config;
            if (config == null)
            {
                SystemLog.Error("Missing chunk config");
                return;
            }

            _scanSize = config.ChunkSize;
            transform.position = Vector3.zero;
        }

        async void BakeNavigationScan()
        {
            SystemLog.Info("Baking Navigation Scan...");

            NavigationScan scan;

            try
            {
                scan = GetNavigationScan();
            }
            catch (Exception ex)
            {
                SystemLog.PopUp("Terrain Scanner", $"Error: {ex.Message}");
                throw ex;
            }

            if (GetNavigationScanFilePath(out string scanPath) == false)
            {
                return;
            }

            if (_testMode == false && File.Exists(scanPath))
            {
                if (SystemLog.PopUp("Terrain Scanner", $"File already exists:\n\n{scanPath}", "Overwrite", "Cancel") == false)
                {
                    return;
                }
            }

            await scan.Save(scanPath);
            AssetDatabase.Refresh();

            SystemLog.PopUp("Terrain Scanner", $"Saved: {scanPath}");

            if (_testMode)
            {
                return;
            }

            _lastChunkScanBake = DateTime.Now.ToString();

            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        async void LoadScanDebug()
        {
            if (_scanDebug != null)
            {
                _scanDebug.Destroy();
            }

            if (GetNavigationScanFilePath(out string scanPath) == false)
            {
                return;
            }

            SystemLog.Info("Loading Navigation Scan Debug...");

            SerializableGameObject.SerializedId.InitContext();

            var scan = await NavigationScan.Load(scanPath);

            SerializableGameObject.SerializedId.ClearContext();

            if (scan == null)
            {
                SystemLog.Error("Use Bake Navigation Scan first");
                return;
            }

            _scanDebug = Debugging.SceneDebug.CreateDebug("Scan Debug", transform);
            _scanDebug.gameObject.hideFlags = _scanDebugHideFlags;
            var nodesPerDebug = new Coordinates3D(25, 25, 25); // Limit amount of nodes in active object to help with lag, essentially creates debug chunks
            var debugSize = nodesPerDebug.ToVector3() * scan.NodeSize;
            var halfDebugSize = debugSize / 2;

            var b = scan.Grid.Bounds;
            var debugBounds = CoordinatesUtils.FromVector3Ceil(new Vector3(b.x / nodesPerDebug.x, b.y / nodesPerDebug.y, b.z / nodesPerDebug.z));
            foreach (var d in debugBounds.EnumerateFromZero())
            {
                var dStart = new Coordinates3D(d.x * nodesPerDebug.x, d.y * nodesPerDebug.y, d.z * nodesPerDebug.z);
                var dEnd = dStart + nodesPerDebug - 1;

                var nodes = new List<Transform>();
                for (int y = dStart.y; y <= dEnd.y; y++)
                {
                    for (int x = dStart.x; x <= dEnd.x; x++)
                    {
                        for (int z = dStart.z; z <= dEnd.z; z++)
                        {
                            var c = new Coordinates3D(x, y, z);
                            if (scan.Grid.IsWithinBounds(c) == false)
                            {
                                continue;
                            }

                            var point = scan.Grid[c];
                            if (point == null)
                            {
                                continue;
                            }

                            var hitPoint = point.Hit.Point;

                            var nodeDebug = _scanDebug.CreatePrimitive("Node " + c.ToString(), PrimitiveType.Sphere, hitPoint, Vector3.one * 0.1f, ColorUtils.GREEN);
                            nodeDebug.transform.LookAt(hitPoint + point.Hit.Normal);
                            nodes.Add(nodeDebug.transform);

                            if (_scanDebugDisplayNodeSize)
                            {
                                var pointDebug = _scanDebug.CreateWireCube("Size", point.Position, Vector3.one * (scan.NodeSize * 0.9f), ColorUtils.CYAN.SetAlpha(0.2f));
                                pointDebug.transform.SetParent(nodeDebug.transform, true);
                            }

                            if (_scanDebugDisplayNormals)
                            {
                                var normalDebug = _scanDebug.CreateArrow("Normal", hitPoint, hitPoint + point.Hit.Normal, Vector3.one * 0.1f);
                                normalDebug.transform.SetParent(nodeDebug.transform, true);
                            }
                        }
                    }
                }

                if (nodes.Count == 0)
                {
                    continue;
                }

                var position = new Vector3(d.x * debugSize.x, d.y * debugSize.y, d.z * debugSize.z);
                position += halfDebugSize;

                var debugParent = new GameObject("Debug Chunk " + d.ToString()).transform;
                _scanDebug.SetChild(debugParent);

                debugParent.position = scan.GridOrigin + position;
                debugParent.localScale = debugSize;
                debugParent.gameObject.SetActive(false);

                foreach (var node in nodes)
                {
                    node.transform.SetParent(debugParent, true);
                }
            }

            SystemLog.PopUp("Terrain Scanner", "Loaded Navigation Scan Debug");
            EditorGUIUtility.PingObject(_scanDebug);
        }

        async void BakeNavigationGraph()
        {
            if (GetNavigationScanFilePath(out string scanPath) == false)
            {
                return;
            }

            SystemLog.Info("Baking Navigation Graph...");

            SerializableGameObject.SerializedId.InitContext();

            var scan = await NavigationScan.Load(scanPath);

            SerializableGameObject.SerializedId.ClearContext();

            if (scan == null)
            {
                SystemLog.PopUp("Terrain Scanner", "Error: Use Bake Navigation Scan first");
                return;
            }

            NavigationGraph graph;

            try
            {
                graph = GetNavigationGraph(scan, _graphUnitConfig);
            }
            catch (Exception ex)
            {
                SystemLog.PopUp("Terrain Scanner", $"Error: {ex.Message}");
                throw ex;
            }

            if (GetNavigationGraphFilePath(_graphUnitConfig.UnitType, out string graphPath) == false)
            {
                return;
            }

            if (_testMode == false && File.Exists(graphPath))
            {
                if (SystemLog.PopUp("Terrain Scanner", $"File already exists:\n\n{graphPath}", "Overwrite", "Cancel") == false)
                {
                    return;
                }
            }

            await graph.Save(graphPath);
            AssetDatabase.Refresh();

            SystemLog.PopUp("Terrain Scanner", $"Saved: {graphPath}");

            if (_testMode)
            {
                return;
            }

            _lastChunkGraphBake = DateTime.Now.ToString();

            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        async void LoadGraphDebug()
        {
            if (_graphDebug != null)
            {
                _graphDebug.Destroy();
            }

            if (GetNavigationGraphFilePath(_graphUnitConfig.UnitType, out string graphPath) == false)
            {
                return;
            }

            SystemLog.Info("Loading Navigation Graph Debug...");

            SerializableGameObject.SerializedId.InitContext();

            var graph = await NavigationGraph.Load(graphPath);

            SerializableGameObject.SerializedId.ClearContext();

            if (graph == null)
            {
                SystemLog.Error("Use Bake Navigation Graph first");
                return;
            }

            _graphDebug = Debugging.SceneDebug.CreateDebug("Graph Debug", transform);
            _graphDebug.gameObject.hideFlags = _graphDebugHideFlags;
            var nodesPerDebug = new Coordinates3D(25, 25, 25); // Limit amount of nodes in active object to help with lag, essentially creates debug chunks
            var debugSize = nodesPerDebug.ToVector3() * graph.NodeSize;
            var halfDebugSize = debugSize / 2;

            var costRange = new MinMaxValue.Float();
            var costRangeColor = new MinMaxValue.Color(ColorUtils.CYAN, ColorUtils.RED);
            if (_graphDebugDisplayEdges)
            {
                foreach (var node in graph.Graph)
                {
                    foreach (var edge in node)
                    {
                        costRange.min = costRange.min == 0 ? edge.Cost : Mathf.Min(costRange.min, edge.Cost);
                        costRange.max = costRange.max == 0 ? edge.Cost : Mathf.Max(costRange.max, edge.Cost);
                    }
                }
            }

            var b = graph.GraphBounds;
            var debugBounds = CoordinatesUtils.FromVector3Ceil(new Vector3(b.x / nodesPerDebug.x, b.y / nodesPerDebug.y, b.z / nodesPerDebug.z));
            var debuggedEdges = new Dictionary<Coordinates3D, List<Coordinates3D>>();
            foreach (var d in debugBounds.EnumerateFromZero())
            {
                var dStart = new Coordinates3D(d.x * nodesPerDebug.x, d.y * nodesPerDebug.y, d.z * nodesPerDebug.z);
                var dEnd = dStart + nodesPerDebug - 1;

                var nodes = new List<Transform>();
                for (int y = dStart.y; y <= dEnd.y; y++)
                {
                    for (int x = dStart.x; x <= dEnd.x; x++)
                    {
                        for (int z = dStart.z; z <= dEnd.z; z++)
                        {
                            var c = new Coordinates3D(x, y, z);
                            if (graph.Graph.TryGetNode(c, out var node) == false)
                            {
                                continue;
                            }

                            var hitColor = node.Data.IsRestable ? ColorUtils.BLUE : ColorUtils.PINK;
                            if (node.Data.IsLedge)
                            {
                                hitColor = ColorUtils.GREEN;
                            }

                            var hitPoint = node.Data.Hit.Point;

                            var nodeDebug = _graphDebug.CreatePrimitive("Node " + node.Id.ToString(), PrimitiveType.Sphere, hitPoint, Vector3.one * 0.1f, hitColor);
                            nodeDebug.transform.LookAt(hitPoint + node.Data.Hit.Normal);
                            nodes.Add(nodeDebug.transform);

                            if (_graphDebugDisplayNormals)
                            {
                                var normalDebug = _graphDebug.CreateArrow("Normal", hitPoint, hitPoint + node.Data.Hit.Normal, Vector3.one * 0.1f);
                                normalDebug.transform.SetParent(nodeDebug.transform, true);
                            }

                            if (_graphDebugDisplayEdges == false)
                            {
                                continue;
                            }

                            debuggedEdges.Add(node.Id, new List<Coordinates3D>());
                            foreach (var edge in node)
                            {
                                // Only draw one line for bidrectional edges
                                if (debuggedEdges.TryGetValue(edge.Node.Id, out var edgeIds))
                                {
                                    if (edgeIds.Contains(node.Id))
                                    {
                                        continue;
                                    }
                                }

                                if (debuggedEdges.TryGetValue(node.Id, out var nodeEdgeIds) == false)
                                {
                                    nodeEdgeIds = new List<Coordinates3D>();
                                    debuggedEdges.Add(node.Id, nodeEdgeIds);
                                }

                                nodeEdgeIds.Add(edge.Node.Id);
                                var edgeColor = costRangeColor.Lerp(costRange.InverseLerp(edge.Cost));

                                var offset = Vector3.up * 0.1f;
                                var edgeDebug = _graphDebug.CreateLine(edge.Node.Id.ToString(), node.Data.Hit.Point + offset, edge.Node.Data.Hit.Point + offset, edgeColor);
                                edgeDebug.transform.SetParent(nodeDebug.transform, true);
                            }
                        }
                    }
                }

                if (nodes.Count == 0)
                {
                    continue;
                }

                var position = new Vector3(d.x * debugSize.x, d.y * debugSize.y, d.z * debugSize.z);
                position += halfDebugSize;

                var debugParent = new GameObject("Debug Chunk " + d.ToString()).transform;
                _graphDebug.SetChild(debugParent);

                debugParent.position = graph.GraphOrigin + position;
                debugParent.localScale = debugSize;
                debugParent.gameObject.SetActive(false);

                foreach (var node in nodes)
                {
                    node.transform.SetParent(debugParent, true);
                }
            }

            SystemLog.PopUp("Terrain Scanner", "Loaded Navigation Graph Debug");
            EditorGUIUtility.PingObject(_graphDebug);
        }

        public NavigationScan GetNavigationScan()
        {
            var bounds = Vector3.one * _scanSize / _scanNodeSize;
            var gridBounds = CoordinatesUtils.FromVector3Ceil(bounds);
            var grid = new Grid3D<NavigationPoint>(gridBounds);

            var origin = transform.position;

            float halfNodeSize = _scanNodeSize / 2;
            float cornerDistance = halfNodeSize * 0.9f;

            var hitResults = new RaycastHit[100];
            var clearanceColliderResults = new Collider[50];
            var clearanceOffset = Vector3.up * (_minimumCollisionClearance + 0.05f);

            for (int x = 0; x <= gridBounds.x; x++)
            {
                for (int z = 0; z <= gridBounds.z; z++)
                {
                    var search = origin + new Vector3(x * _scanNodeSize, _scanCeiling, z * _scanNodeSize);

                    int hits = Physics.RaycastNonAlloc(search, Vector3.down, hitResults, _scanCeiling);
                    if (hits == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < hits; i++)
                    {
                        var hit = hitResults[i];

                        if (hit.collider.TryGetComponent(out NavigationTerrain terrain) == false || terrain.Config.IsNavigable == false)
                        {
                            continue;
                        }

                        bool hasCollisionClearance = true;
                        if (_minimumCollisionClearance > 0)
                        {
                            var clearancePos = hit.point + clearanceOffset;
                            int clearanceColliders = Physics.OverlapBoxNonAlloc(clearancePos, Vector3.one * _minimumCollisionClearance, clearanceColliderResults);
                            for (int j = 0; j < clearanceColliders; j++)
                            {
                                var collider = clearanceColliderResults[j];
                                if (IsNavigableTerrainCollision(hit.collider, collider) == false)
                                {
                                    hasCollisionClearance = false;
                                    break;
                                }
                            }
                        }

                        if (hasCollisionClearance == false)
                        {
                            continue;
                        }

                        var c = NavigationMap.GetNodeId(hit.point, origin, _scanNodeSize);

                        var nodeWorldPos = origin + c.ToVector3() * _scanNodeSize;

                        // Check if corners raycasted towards surface hit something to detect if on a ledge or not
                        var cornerCenter = hit.point + Vector3.up * halfNodeSize;
                        cornerCenter = hit.point + hit.normal * Vector3.Distance(cornerCenter, hit.point);
                        var cornerLeft = Vector3.Cross(Vector3.forward, hit.normal).normalized;
                        var cornerFoward = Vector3.Cross(-cornerLeft, hit.normal).normalized;

                        var corners = new Vector3[]
                        {
                            cornerCenter + (cornerLeft + cornerFoward) * cornerDistance,
                            cornerCenter - (cornerLeft + cornerFoward) * cornerDistance,
                            cornerCenter + (cornerLeft - cornerFoward) * cornerDistance,
                            cornerCenter - (cornerLeft - cornerFoward) * cornerDistance
                        };

                        bool allCornersHit = true;
                        foreach (var corner in corners)
                        {
                            if (Physics.Raycast(corner, -hit.normal, out var cornerHit, _scanNodeSize) == false)
                            {
                                allCornersHit = false;
                                break;
                            }

                            var cornerTerrain = cornerHit.collider.GetComponent<NavigationTerrain>();
                            if (cornerTerrain == null || cornerTerrain.Config.IsNavigable == false)
                            {
                                allCornersHit = false;
                                break;
                            }
                        }

                        bool isRestable = allCornersHit;
                        bool isLedge = !allCornersHit;
                        var existingNode = grid[c];
                        if (existingNode == null || hit.point.y > existingNode.Hit.Point.y)
                        {
                            grid[c] = new NavigationPoint(terrain, hit, nodeWorldPos, isRestable, isLedge);
                        }
                    }
                }
            }

            return new NavigationScan(grid, origin, _scanNodeSize);
        }

        public NavigationGraph GetNavigationGraph(NavigationScan navigationScan, NavigationUnitConfig unitConfig)
        {
            var grid = navigationScan.Grid;

            float unitRadius = Mathf.Max(unitConfig.UnitSize.x, unitConfig.UnitSize.z) / 2;
            var collisionHalfExtents = new Vector3(unitRadius, unitConfig.UnitSize.y / 2, unitRadius);

            var clearanceColliderResults = new Collider[50];
            var clearanceOffset = Vector3.up * (collisionHalfExtents.y + 0.05f);
            var stepIncrement = 0.1f;

            foreach (var c in grid.Bounds.EnumerateFromZero())
            {
                var point = grid[c];
                if (point == null)
                {
                    continue;
                }

                if (Vector3.Angle(point.Hit.Normal, Vector3.up) - unitConfig.MaxSlopeDegrees > 0.1f)
                {
                    grid[c] = null;
                    continue;
                }

                bool hasClearance = point.IsRestable || point.IsLedge;
                var clearancePos = point.Hit.Point + clearanceOffset;
                if (hasClearance)
                {
                    int clearanceColliders = Physics.OverlapBoxNonAlloc(clearancePos, collisionHalfExtents, clearanceColliderResults);
                    for (int j = 0; j < clearanceColliders; j++)
                    {
                        var collider = clearanceColliderResults[j];
                        if (IsNavigableTerrainCollision(point.Hit.Collider, collider) == false)
                        {
                            hasClearance = false;
                            break;
                        }
                    }
                }

                bool hasStepClearance = false;
                if (hasClearance == false)
                {
                    // If unit is doesn't have clearance on the ground, check if they can step over the collision
                    for (float step = stepIncrement; step <= unitConfig.UnitStepHeight + 0.1f; step += stepIncrement)
                    {
                        hasStepClearance = true;
                        var stepClearancePos = clearancePos + Vector3.up * step;
                        int stepClearanceColliders = Physics.OverlapBoxNonAlloc(stepClearancePos, collisionHalfExtents, clearanceColliderResults);
                        for (int j = 0; j < stepClearanceColliders; j++)
                        {
                            var collider = clearanceColliderResults[j];
                            if (IsNavigableTerrainCollision(point.Hit.Collider, collider) == false)
                            {
                                hasStepClearance = false;
                                break;
                            }
                        }
                        if (hasStepClearance)
                        {
                            break;
                        }
                    }
                }

                if (hasClearance == false && hasStepClearance == false)
                {
                    grid[c] = null;
                    continue;
                }

                bool isRestable = point.IsRestable && hasClearance;
                point = new NavigationPoint(point.Terrain, point.Hit, point.Position, isRestable, point.IsLedge);

                grid[c] = point;
            }

            var graph = new Graph<Coordinates3D, NavigationPoint>();

            foreach (var c in grid.Bounds.EnumerateFromZero())
            {
                var point = grid[c];
                if (point == null)
                {
                    continue;
                }

                var node = graph.AddNewNodeOrGet(c, point);

                if (point.IsLedge)
                {
                    continue;
                }

                // For now, create edges with any adjacent nodes
                // TODO: Use step logic and other distance thresholds to determine where unit can feasibly travel to
                foreach (var e in c.EnumerateRadiusAsCubeShell(1))
                {
                    if (grid.IsWithinBounds(e) == false)
                    {
                        continue;
                    }

                    var edgePoint = grid[e];
                    if (edgePoint == null)
                    {
                        continue;
                    }

                    float cost = Vector3.Distance(point.Hit.Point, edgePoint.Hit.Point);

                    var edgeNode = graph.AddNewNodeOrGet(e, edgePoint);
                    node    .AddNewEdgeOrGet(edgeNode, cost);
                    edgeNode.AddNewEdgeOrGet(node    , cost);
                }
            }

            return new NavigationGraph(graph, navigationScan.GridOrigin, navigationScan.NodeSize);
        }

        bool IsNavigableTerrainCollision(Collider a, Collider b)
        {
            // For now, consider it navigable if unit is colliding with the same terrain that it's walking on, or if the collision is not another terrain object
            // Helps with slopes
            // TODO: Come up with better solution
            return a == b || b.TryGetComponent<NavigationTerrain>(out _) == false;
        }

        bool GetStandaloneFilePath(string fileName, out string path)
        {
            path = null;

            string id = new SerializableGameObject.SerializedId(gameObject).Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                SystemLog.PopUp("Terrain Scanner", $"Error: Missing ID on {gameObject.name}, remove Serializable Game Object component so it can reinitialize");
                return false;
            }

            path = FileUtils.GetStreamingAssetsPath("TerrainScanner", SceneManager.GetActiveScene().name, id, fileName);
            return true;
        }

        bool GetNavigationScanFilePath(out string path)
        {
            if (_chunk == null || _testMode)
            {
                return GetStandaloneFilePath("NavigationScan", out path);
            }

            path = _chunk.GetNavigationScanPath();
            return true;
        }

        bool GetNavigationGraphFilePath(UnitType unitType, out string path)
        {
            if (_chunk == null || _testMode)
            {
                return GetStandaloneFilePath("NavigationGraph", out path);
            }

            path = _chunk.GetNavigationGraphPath(unitType);
            return true;
        }

        void OnDrawGizmos()
        {
            if (SelectionIncludesGameObject(gameObject, true) == false)
            {
                return;
            }

            var mainColor = _testMode ? ColorUtils.RED : ColorUtils.WHITE;

            var size = new Vector3(_scanSize, _scanCeiling, _scanSize);
            Gizmos.color = mainColor;
            Gizmos.DrawWireCube(transform.position + size / 2, size);

            // Since scan and graph debug is split into chunks, highlight selected chunk bounds so it's easy to see what will display
            // before activating tons of debug objects

            if (_scanDebug != null && SelectionIncludesGameObject(_scanDebug.DebugHolder.gameObject, true))
            {
                Gizmos.color = mainColor;

                for (int i = 0; i < _scanDebug.DebugHolder.childCount; i++)
                {
                    var scanChild = _scanDebug.DebugHolder.GetChild(i);
                    if (scanChild.gameObject.activeSelf || SelectionIncludesGameObject(scanChild.gameObject, true) == false)
                    {
                        continue;
                    }

                    Gizmos.DrawWireCube(scanChild.position, scanChild.lossyScale);
                }
            }

            if (_graphDebug != null && SelectionIncludesGameObject(_graphDebug.DebugHolder.gameObject, true))
            {
                Gizmos.color = mainColor;

                for (int i = 0; i < _graphDebug.DebugHolder.childCount; i++)
                {
                    var graphChild = _graphDebug.DebugHolder.GetChild(i);
                    if (graphChild.gameObject.activeSelf || SelectionIncludesGameObject(graphChild.gameObject, true) == false)
                    {
                        continue;
                    }

                    Gizmos.DrawWireCube(graphChild.position, graphChild.lossyScale);
                }
            }

            static bool SelectionIncludesGameObject(GameObject obj, bool asParent = false)
            {
                foreach (var selection in Selection.gameObjects)
                {
                    if (selection == obj)
                    {
                        return true;
                    }

                    if (asParent)
                    {
                        var parent = selection.transform.parent;
                        while (parent != null)
                        {
                            if (parent.gameObject == obj)
                            {
                                return true;
                            }
                            parent = parent.parent;
                        }
                    }
                }
                return false;
            }
        }
    }
}
#endif
