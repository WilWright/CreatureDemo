using System;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class SerializableGameObject : MonoBehaviour
{
    [Serializable]
    public struct SerializedId
    {
        readonly struct Context
        {
            public readonly GameObject GameObject;
            public readonly Component[] Components;

            public Context(GameObject gameObject, Component[] components)
            {
                GameObject = gameObject;
                Components = components;
            }
        }

        [field: SerializeField] public string Id     { get; private set; }
        [field: SerializeField] public string IdType { get; private set; }

        static Dictionary<string, Context> _serializationContext;

        public SerializedId(GameObject obj)
        {
            Id     = null;
            IdType = "GameObject";

            Id = GetOrInitSerializableGameObject(obj).Id;
        }

        public SerializedId(Component component)
        {
            Id     = null;
            IdType = component.GetType().ToString();

            Id = GetOrInitSerializableGameObject(component.gameObject).Id;
        }

        public static void InitContext(Transform fromTransform = null)
        {
            _serializationContext = new();

            SerializableGameObject[] sgos;

            if (fromTransform != null)
            {
                sgos = fromTransform.GetComponentsInChildren<SerializableGameObject>(true);
            }
            else
            {
                sgos = FindObjectsByType<SerializableGameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            }

            if (CollectionUtils.IsNullOrEmpty(sgos))
            {
                return;
            }

            foreach (var s in sgos)
            {
                _serializationContext.Add(s.Id, new Context(s.gameObject, s.GetComponents<Component>()));
            }
        }

        public static void ClearContext()
        {
            _serializationContext = null;
        }

        public readonly GameObject GetFromContext()
        {
            var context = GetContext();
            if (context.Equals(default))
            {
                return null;
            }

            return context.GameObject;
        }

        public readonly T GetFromContext<T>() where T : Component
        {
            var context = GetContext();
            if (context.Equals(default))
            {
                return null;
            }

            foreach (var c in context.Components)
            {
                if (c is T t)
                {
                    return t;
                }
            }

            return null;
        }

        readonly Context GetContext()
        {
            if (_serializationContext == null)
            {
                SystemLog.Warn("Serialization context not initialized");
                return default;
            }

            if (_serializationContext.TryGetValue(Id, out var c) == false)
            {
                SystemLog.Warn($"Serialization context not found for {Id} ({IdType})");
                return default;
            }

            return c;
        }

        readonly SerializableGameObject GetOrInitSerializableGameObject(GameObject obj)
        {
            if (obj.TryGetComponent(out SerializableGameObject s) == false)
            {
                s = obj.AddComponent<SerializableGameObject>();
                s.SetNewId();
            }

            if (string.IsNullOrWhiteSpace(s.Id))
            {
                s.SetNewId();
            }

            return s;
        }
    }

    [field: SerializeField] public string Id { get; private set; }

    public void SetNewId()
    {
        Id = Guid.NewGuid().ToString();
    }
}
