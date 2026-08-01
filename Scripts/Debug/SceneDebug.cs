using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Debugging
{
    public class SceneDebug : MonoBehaviour
    {
        public Transform DebugHolder { get; private set; }

        KeyCode? _toggleKey;

        static readonly Dictionary<Color, Material> _materials = new();

        static Material _baseMaterial;
        static Material BaseMaterial
        {
            get
            {
                if (_baseMaterial == null)
                {
                    _baseMaterial = Resources.Load<Material>("Debug/BaseMaterial");
                }
                return _baseMaterial;
            }
        }

        static Material _baseTransparentMaterial;
        static Material BaseTransparentMaterial
        {
            get
            {
                if (_baseTransparentMaterial == null)
                {
                    _baseTransparentMaterial = Resources.Load<Material>("Debug/BaseTransparentMaterial");
                }
                return _baseTransparentMaterial;
            }
        }

        static readonly Vector3[] CUBE_VERTICES = new Vector3[]
        {
            Vector3.zero,
            Vector3.right,
            Vector3.forward,
            Vector3.right + Vector3.forward,
            Vector3.up,
            Vector3.up + Vector3.right,
            Vector3.up + Vector3.forward,
            Vector3.up + Vector3.right + Vector3.forward
        };

        static readonly (int a, int b)[] CUBE_VERTEX_PAIRS = new (int a, int b)[]
        {
            (0, 1), (0, 2), (1, 3), (2, 3),
            (4, 5), (4, 6), (5, 7), (6, 7),
            (0, 4), (1, 5), (2, 6), (3, 7)
        };

        static readonly Vector3[] WIRE_SPHERE_DIRECTIONS = new Vector3[]
        {
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };

        const int CIRCLE_SEGMENTS = 25;

        public static SceneDebug CreateDebug(string name, Transform parent = null, bool startActive = true, KeyCode? toggleKey = KeyCode.Alpha2)
        {
            var debug = new GameObject(name).AddComponent<SceneDebug>();
            debug.transform.SetParent(parent);
            debug.DebugHolder = new GameObject(name).transform;
            debug.DebugHolder.SetParent(debug.transform);
            debug.DebugHolder.gameObject.SetActive(startActive);
            debug._toggleKey = toggleKey;
            return debug;
        }

        public void Update()
        {
            if (_toggleKey.HasValue && Input.GetKey(KeyCode.Tab) && Input.GetKeyDown(_toggleKey.Value))
            {
                DebugHolder.gameObject.SetActive(!DebugHolder.gameObject.activeSelf);
            }
        }

        public static void HighlightObject(GameObject obj, bool highlightTransform = false, Bounds? bounds = null, Color? color = null)
        {
            var debugColor = color ?? ColorUtils.WHITE;

            var t = obj.transform;

            float radius = 0.1f;
            var vertexStart = t.position - Vector3.one * radius / 2;
            foreach (var (a, b) in CUBE_VERTEX_PAIRS)
            {
                Debug.DrawLine(
                    vertexStart + CUBE_VERTICES[a] * radius,
                    vertexStart + CUBE_VERTICES[b] * radius,
                    debugColor
                );
            }

            if (highlightTransform)
            {
                float length = 0.5f;
                DrawAxis(t.right, ColorUtils.RED);
                DrawAxis(t.up, ColorUtils.GREEN);
                DrawAxis(t.forward, ColorUtils.BLUE);

                void DrawAxis(Vector3 direction, Color color)
                {
                    Debug.DrawLine(bounds.Value.center, bounds.Value.center + direction * length, color);
                }
            }

            if (bounds != null)
            {
                var size = bounds.Value.size;
                foreach (var (a, b) in CUBE_VERTEX_PAIRS)
                {
                    Debug.DrawLine(
                        GetVertexPosition(CUBE_VERTICES[a]),
                        GetVertexPosition(CUBE_VERTICES[b]),
                        debugColor
                    );

                    Vector3 GetVertexPosition(Vector3 vertex)
                    {
                        return bounds.Value.min + new Vector3(vertex.x * size.x, vertex.y * size.y, vertex.z * size.z);
                    }
                }
            }
        }

        public void SetChild(Transform transform, bool worldPositionStays = true)
        {
            transform.SetParent(DebugHolder, worldPositionStays);
        }

        public GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Color? color = null)
        {
            return CreatePrimitive(name, primitiveType, position, Quaternion.identity, scale, color);
        }
        public GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Vector3 position, Quaternion rotation, Vector3 scale, Color? color = null)
        {
            var obj = GameObject.CreatePrimitive(primitiveType);
            obj.TryGetComponent<Collider>(out var collider);
            DestroyImmediate(collider);
            InitObject(name, obj, position, rotation, scale);
            SetMaterialColor(obj, color ?? ColorUtils.WHITE);
            return obj;
        }

        public GameObject CreateWireCube(string name, Vector3 position, Vector3 scale, Color? color = null)
        {
            return CreateWireCube(name, position, Quaternion.identity, scale, color);
        }
        public GameObject CreateWireCube(string name, Vector3 position, Quaternion rotation, Vector3 scale, Color? color = null)
        {
            var cube = InitObject(name, null, position, rotation, scale);

            var offset = Vector3.one * -0.5f;
            foreach (var (a, b) in CUBE_VERTEX_PAIRS)
            {
                var line = CreateLine("Edge", CUBE_VERTICES[a] + offset, CUBE_VERTICES[b] + offset, color);
                line.transform.SetParent(cube.transform, false);
            }

            return cube;
        }

        public GameObject CreateWireSphere(string name, Vector3 position, Vector3 scale, Color? color = null)
        {
            return CreateWireSphere(name, position, Quaternion.identity, scale, color);
        }
        public GameObject CreateWireSphere(string name, Vector3 position, Quaternion rotation, Vector3 scale, Color? color = null)
        {
            var sphere = InitObject(name, null, position, rotation, scale);

            foreach (var direction in WIRE_SPHERE_DIRECTIONS)
            {
                var circle = CreateWireCircle("Circle", Vector3.zero, Quaternion.LookRotation(direction), Vector3.one, color);
                circle.transform.SetParent(sphere.transform, false);
            }

            return sphere;
        }

        public GameObject CreateWireCircle(string name, Vector3 position, Vector3 scale, Color? color = null)
        {
            return CreateWireCircle(name, position, Quaternion.identity, scale, color);
        }
        public GameObject CreateWireCircle(string name, Vector3 position, Quaternion rotation, Vector2 scale, Color? color = null)
        {
            var circle = InitObject(name, null, position, Quaternion.identity, scale);

            float angleStep = 360.0f / CIRCLE_SEGMENTS;
            float radius = 0.5f;
            for (int i = 1; i <= CIRCLE_SEGMENTS; i++)
            {
                var line = CreateLine("Line", GetCirclePoint(i - 1), GetCirclePoint(i), color);
                line.transform.SetParent(circle.transform);
            }
            circle.transform.rotation = rotation;

            return circle;

            Vector3 GetCirclePoint(int step)
            {
                float angle = step * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                return new Vector3(x, y, 0);
            }
        }

        public GameObject CreateArrow(string name, Vector3 start, Vector3 end, Vector3 headScale, Color? color = null)
        {
            var arrow = InitObject(name, null, start, Quaternion.identity, Vector3.one);
            var line = CreateLine("Line", start, end, color);
            var head = CreateWireCube("Head", end, Quaternion.LookRotation(end - start), headScale, color);

            arrow.transform.rotation = line.transform.rotation;

            head.transform.SetParent(line.transform);
            line.transform.SetParent(arrow.transform);

            return arrow;
        }

        public GameObject CreateLine(string name, Vector3 start, Vector3 end, Color? color = null)
        {
            var line = DebugLine.Create(name, start, end, color);
            line.transform.SetParent(DebugHolder);
            return line.gameObject;
        }

        public void Destroy()
        {
            DestroyImmediate(gameObject);
        }

        GameObject InitObject(string name, GameObject obj, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (obj == null)
            {
                obj = new GameObject();
            }

            obj.name = name;
            obj.transform.SetParent(DebugHolder);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = scale;

            return obj;
        }

        void SetMaterialColor(GameObject gameObject, Color color)
        {
            var renderer = gameObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = GetMaterialColor(color);
        }

        Material GetMaterialColor(Color color)
        {
            if (_materials.TryGetValue(color, out var material) == false)
            {
                material = new(color.a == 1 ? BaseMaterial : BaseTransparentMaterial)
                {
                    color = color
                };
                _materials.Add(color, material);
            }

            return material;
        }
    }
}
