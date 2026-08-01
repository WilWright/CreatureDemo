using UnityEngine;

using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Debugging
{
    public class DebugLine : MonoBehaviour
    {
        public Transform End { get; private set; }

        Color _color;

        public static DebugLine Create(string name, Vector3 start, Vector3 end, Color? color = null)
        {
            color ??= ColorUtils.WHITE;

            var vector = end - start;
            var obj = new GameObject(name).AddComponent<DebugLine>();
            obj.transform.LookAt(vector);
            obj._color = color.Value;

            obj.End = new GameObject("End").transform;
            obj.End.SetParent(obj.transform);
            obj.End.SetLocalPositionAndRotation(Vector3.forward * vector.magnitude, Quaternion.identity);

            obj.transform.position = start;

            return obj;
        }

        void Update()
        {
            Debug.DrawLine(transform.position, End.position, _color);
        }

        public void SetStartParent(Transform parent)
        {
            transform.SetParent(parent);
        }
        public void SetEndParent(Transform parent)
        {
            End.SetParent(parent);
        }
        public void SetStartAndEndParent(Transform startParent, Transform endParent)
        {
            SetStartParent(startParent);
            SetEndParent  (endParent  );
        }

    #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            Gizmos.color = _color;
            Gizmos.DrawLine(transform.position, End.position);
        }
#   endif
    }
}
