using System;

public struct FieldParameter
{
    [Serializable]
    public struct Int
    {
        public int baseValue;
        public bool useRandomValue;
        public MinMaxValue.Int minMaxValue;

        public readonly int GetValue() => useRandomValue ? minMaxValue.GetRandomValue() : baseValue;
    }

    [Serializable]
    public struct Float
    {
        public float baseValue;
        public bool useRandomValue;
        public MinMaxValue.Float minMaxValue;

        public readonly float GetValue() => useRandomValue ? minMaxValue.GetRandomValue() : baseValue;
    }

    [Serializable]
    public struct Color
    {
        public UnityEngine.Color baseValue;
        public bool useRandomValue;
        public MinMaxValue.Color minMaxValue;

        public readonly UnityEngine.Color GetValue() => useRandomValue ? minMaxValue.GetRandomValue() : baseValue;
    }
}
