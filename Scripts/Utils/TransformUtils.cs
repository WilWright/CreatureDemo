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
    }
}
