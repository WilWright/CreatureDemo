using System;
using System.Collections.Generic;

public class MinHeap<T, TValue> where T : class, IHeapElement<T, TValue>
{
    public int Count => _heap.Count;

    readonly List<T> _heap = new();

    public T Peek()
    {
        if (_heap.Count == 0)
        {
            throw new InvalidOperationException("Heap empty.");
        }

        return _heap[0];
    }

    public T Pop()
    {
        var element = Peek();
        element.HeapIndex = -1;

        int last = Count - 1;
        if (last == 0)
        {
            _heap.RemoveAt(0);
            return element;
        }

        var l = _heap[last];
        l.HeapIndex = 0;
        _heap[0] = l;

        _heap.RemoveAt(last);

        SortDown(l);

        return element;
    }

    public void Insert(T element)
    {
        element.HeapIndex = _heap.Count;
        _heap.Add(element);
        SortUp(element);
    }

    public void SortUp(T element)
    {
        while (element.HeapIndex > 0)
        {
            int parent = (element.HeapIndex - 1) / 2;
            var p = _heap[parent];
            if (element.CompareTo(p) >= 0)
            {
                return;
            }

            Swap(element, p);
        }

        if (element.HeapIndex == -1)
        {
            throw new InvalidOperationException("Element not in heap.");
        }
    }

    public void SortDown(T element)
    {
        while (element.HeapIndex >= 0)
        {
            int children = element.HeapIndex * 2;
            int left = children + 1;

            if (left >= Count)
            {
                return;
            }

            var swap = _heap[left];
            int right = children + 2;
            if (right < Count)
            {
                var r = _heap[right];
                if (r.CompareTo(swap) < 0)
                {
                    swap = r;
                }
            }

            if (element.CompareTo(swap) <= 0)
            {
                return;
            }

            Swap(element, swap);
        }

        throw new InvalidOperationException("Element not in heap.");
    }

    public void Clear()
    {
        foreach (var element in _heap)
        {
            element.HeapIndex = -1;
        }
        _heap.Clear();
    }

    public override string ToString()
    {
        var stringBuilder = new Utils.CollectionStringBuilder();
        foreach (var element in _heap)
        {
            stringBuilder.Append(element.GetHeapValue());
        }
        return stringBuilder.Build();
    }

    void Swap(T a, T b)
    {
        _heap[a.HeapIndex] = b;
        _heap[b.HeapIndex] = a;

        (b.HeapIndex, a.HeapIndex) = (a.HeapIndex, b.HeapIndex);
    }
}

public interface IHeapElement<T, TValue> : IComparable<T>
{
    int HeapIndex { get; set; }
    TValue GetHeapValue();
}
