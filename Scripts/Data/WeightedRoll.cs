using System;
using System.Collections;
using System.Collections.Generic;

public class WeightedRoll<T>
{
    readonly struct RollWeight
    {
        public readonly T Data;
        public readonly double Weight;

        public RollWeight(T data, double weight)
        {
            Data   = data;
            Weight = weight;
        }
    }

    public int Count => _rollWeights.Count;

    public double TotalWeight { get; private set; }

    readonly List<RollWeight> _rollWeights = new();

    readonly Random _random;

    public WeightedRoll()
    {
        _random = new Random();
    }
    public WeightedRoll(int seed)
    {
        _random = new Random(seed);
    }

    public bool Roll(out T data)
    {
        double roll = _random.NextDouble() * TotalWeight;
        double currentWeight = 0;
        for (int i = 0; i < _rollWeights.Count; i++)
        {
            var rw = _rollWeights[i];
            currentWeight += rw.Weight;
            if (roll < currentWeight)
            {
                data = rw.Data;
                return true;
            }
        }

        data = default;
        return false;
    }

    public void AddWeight(T data, double weight)
    {
        _rollWeights.Add(new RollWeight(data, weight));
        TotalWeight += weight;
    }

    public void RemoveWeight(T data)
    {
        for (int i = 0; i < _rollWeights.Count; i++)
        {
            var rw = _rollWeights[i];
            if (rw.Data.Equals(data))
            {
                TotalWeight -= rw.Weight;
                _rollWeights.RemoveAt(i);
                return;
            }
        }
    }

    public void Clear()
    {
        _rollWeights.Clear();
        TotalWeight = 0;
    }
}
