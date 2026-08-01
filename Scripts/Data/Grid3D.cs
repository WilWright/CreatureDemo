using System;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class Grid3D<T>
{
    [Serializable]
    public struct SerializedGrid
    {
        [Serializable]
        public struct Element
        {
            [field: SerializeField] public Coordinates3D Coordinates { get; private set; }
            [field: SerializeField] public T             Data        { get; private set; }

            public Element(Coordinates3D c, T element)
            {
                Coordinates = c;
                Data        = element;
            }
        }

        [field: SerializeField] public Element[] Elements { get; private set; }

        public SerializedGrid(Grid3D<T> g)
        {
            var elements = new List<Element>();
            foreach (var c in g.Bounds.EnumerateFromZero())
            {
                var e = g[c];
                if (e == null)
                {
                    continue;
                }

                elements.Add(new Element(c, e));
            }
            Elements = elements.Count == 0 ? null : elements.ToArray();
        }
    }

    public T this[Coordinates3D c]
    {
        get => _flatGrid[c.x + c.z * Size.x + c.y * Size.x * Size.z];
        set => _flatGrid[c.x + c.z * Size.x + c.y * Size.x * Size.z] = value;
    }

    public readonly Coordinates3D Bounds;
    public readonly Coordinates3D Size;

    readonly T[] _flatGrid;

    public Grid3D(Coordinates3D bounds) : this(bounds.x + 1, bounds.y + 1, bounds.z + 1) {}
    public Grid3D(int width, int height, int depth)
    {
        Size = new Coordinates3D(width, height, depth);
        Bounds = Size - 1;
        _flatGrid = new T[width * height * depth];
    }

    public Grid3D(SerializedGrid s)
    {
        if (s.Elements == null)
        {
            return;
        }

        foreach (var element in s.Elements)
        {
            Bounds = CoordinatesUtils.Max(Bounds, element.Coordinates);
        }

        Size = Bounds + 1;
        _flatGrid = new T[Size.x * Size.y * Size.z];

        foreach (var element in s.Elements)
        {
            this[element.Coordinates] = element.Data;
        }
    }

    public SerializedGrid GetSerialized()
    {
        return new SerializedGrid(this);
    }

    public T RemoveAt(Coordinates3D c)
    {
        T element = this[c];
        this[c] = default;
        return element;
    }

    public void Move(Coordinates3D from, Coordinates3D to)
    {
        T element = RemoveAt(from);
        if (element.Equals(default))
        {
            throw new Exception($"Element does not exist at {from}");
        }

        this[to] = element;
    }

    public void MoveDirection(Coordinates3D c, Coordinates3D direction)
    {
        Move(c, c + direction);
    }

    public bool IsWithinBounds(Coordinates3D c)
    {
        return c.x >= 0 && c.x <= Bounds.x
            && c.y >= 0 && c.y <= Bounds.y 
            && c.z >= 0 && c.z <= Bounds.z;
    }

    public Enumerator GetEnumerator() => new(_flatGrid);

    public CoordinatesUtils.FromZeroEnumerator EnumerateBounds() => Bounds.EnumerateFromZero();

    public override string ToString()
    {
        var builder = new CollectionStringBuilder();
        foreach (var c in Bounds.EnumerateFromZero())
        {
            builder.Append($"{c}: {this[c]}");
        }
        return builder.Build();
    }

    public struct Enumerator
    {
        public readonly T Current => _flatGrid[_index];

        readonly T[] _flatGrid;

        int _index;

        public Enumerator(T[] flatGrid)
        {
            _flatGrid = flatGrid;

            _index = -1;
        }

        public bool MoveNext()
        {
            if (++_index >= _flatGrid.Length)
            {
                return false;
            }

            return true;
        }
    }
}
