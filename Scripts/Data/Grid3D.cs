using System;

using Utils;

public class Grid3D<T>
{
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

    public void Clear()
    {
        foreach (var c in EnumerateBounds())
        {
            this[c] = default;
        }
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
