using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class TransformUtils
    {
        public static void LookAtXY(this Transform transform, Vector3 position)
        {
            position.z = transform.position.z;
            transform.LookAt(position);
        }

        public static void LookAtXZ(this Transform transform, Vector3 position)
        {
            position.y = transform.position.y;
            transform.LookAt(position);
        }

        public static bool GetLookRotationXY(Vector3 forward, out Quaternion rotation)
        {
            forward.z = 0;
            return GetLookRotation(forward, out rotation);
        }

        public static bool GetLookRotationXZ(Vector3 forward, out Quaternion rotation)
        {
            forward.y = 0;
            return GetLookRotation(forward, out rotation);
        }

        public static bool GetLookRotation(Vector3 forward, out Quaternion rotation)
        {
            if (forward == Vector3.zero)
            {
                rotation = Quaternion.identity;
                return false;
            }

            rotation = Quaternion.LookRotation(forward);
            return true;
        }

        public static T[] GetComponentsInDirectChildren<T>(this Transform transform) where T : Component
        {
            var components = new List<T>();
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out T component))
                {
                    components.Add(component);
                }
            }

            return components.ToArray();
        }

        public static void DestroyChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
