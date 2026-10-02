using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class GameObjectUtils
    {
        public static string GetHierarchy(this GameObject gameObject)
        {
            var builder = new CollectionStringBuilder(" < ");

            builder.Append(gameObject.name);

            var parent = gameObject.transform.parent;
            while (parent != null)
            {
                builder.Append(parent.name);
                parent = parent.parent;
            }

            return builder.Build();
        }
    }
}
