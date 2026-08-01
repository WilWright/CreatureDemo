using System;
using UnityEngine;

using Random = UnityEngine.Random;

public struct MinMaxValue
{
    [Serializable]
    public struct Int
    {
        public int min;
        public int max;

        public Int(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public readonly int GetRandomValue() => Random.Range(min, max + 1);

        public readonly int GetClampedValue(int value) => Mathf.Clamp(value, min, max);

        public readonly int Lerp(float percent) => Mathf.RoundToInt(Mathf.Lerp(min, max, percent));

        public readonly float InverseLerp(float value) => Mathf.InverseLerp(min, max, value);

        public readonly override string ToString() => "(" + min + ", " + max + ")";
    }

    [Serializable]
    public struct Float
    {
        public float min;
        public float max;

        public Float(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public readonly float GetRandomValue() => Random.Range(min, max);

        public readonly float GetClampedValue(float value) => Mathf.Clamp(value, min, max);

        public readonly float Lerp(float percent) => Mathf.Lerp(min, max, percent);

        public readonly float InverseLerp(float value) => Mathf.InverseLerp(min, max, value);

        public readonly override string ToString() => "(" + min + ", " + max + ")";
    }

    [Serializable]
    public struct Color
    {
        public UnityEngine.Color min;
        public UnityEngine.Color max;

        public Color(UnityEngine.Color min, UnityEngine.Color max)
        {
            this.min = min;
            this.max = max;
        }

        public readonly UnityEngine.Color GetRandomValue() => Lerp(Random.value);

        public readonly UnityEngine.Color Lerp(float percent) => UnityEngine.Color.Lerp(min, max, percent);

        public readonly override string ToString() => "(" + min + ", " + max + ")";
    }
}
