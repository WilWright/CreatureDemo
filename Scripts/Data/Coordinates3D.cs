using System;
using System.Collections.Generic;

[Serializable]
// Warning: Since this object is mutable, if used as a key (e.g. Dictionary<Coordinates3D, Object>),
// it will no longer reference the same key if values are changed
public struct Coordinates3D : IEquatable<Coordinates3D>
{
    public static readonly Coordinates3D UP      = new( 0,  1,  0);
    public static readonly Coordinates3D DOWN    = new( 0, -1,  0);
    public static readonly Coordinates3D LEFT    = new(-1,  0,  0);
    public static readonly Coordinates3D RIGHT   = new( 1,  0,  0);
    public static readonly Coordinates3D FORWARD = new( 0,  0,  1);
    public static readonly Coordinates3D BACK    = new( 0,  0, -1);

    public static readonly Coordinates3D ONE  = new(1, 1, 1);
    public static readonly Coordinates3D ZERO = new(0, 0, 0);

    public static readonly IReadOnlyList<Coordinates3D> COMPASS_DIRECTIONS = new Coordinates3D[]
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        FORWARD,
        BACK
    };

    public static readonly IReadOnlyList<Coordinates3D> EDGE_DIRECTIONS = new Coordinates3D[]
    {
        UP    + LEFT,
        UP    + RIGHT,
        UP    + FORWARD,
        UP    + BACK,
        DOWN  + LEFT,
        DOWN  + RIGHT,
        DOWN  + FORWARD,
        DOWN  + BACK,
        LEFT  + FORWARD,
        LEFT  + BACK,
        RIGHT + FORWARD,
        RIGHT + BACK
    };

    public static readonly IReadOnlyList<Coordinates3D> CORNER_DIRECTIONS = new Coordinates3D[]
    {
        UP   + LEFT  + FORWARD,
        UP   + RIGHT + FORWARD,
        UP   + LEFT  + BACK,
        UP   + RIGHT + BACK,
        DOWN + LEFT  + FORWARD,
        DOWN + RIGHT + FORWARD,
        DOWN + LEFT  + BACK,
        DOWN + RIGHT + BACK
    };

    public static readonly IReadOnlyList<Coordinates3D> ALL_DIRECTIONS = GetAllDirections();

    public int x;
    public int y;
    public int z;

    public Coordinates3D(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public readonly override string ToString() => "(" + x + ", " + y + ", " + z + ")";

    public readonly bool Equals(Coordinates3D other) => this == other;
    public readonly override bool Equals(object obj)
    {
        if (obj is Coordinates3D other)
        {
            return Equals(other);
        }
        return false;
    }

    public readonly override int GetHashCode() => HashCode.Combine(x, y, z);

    public static Coordinates3D operator +(Coordinates3D a, Coordinates3D b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Coordinates3D operator +(Coordinates3D a, int b)           => new(a.x + b  , a.y + b  , a.z + b);
    public static Coordinates3D operator -(Coordinates3D a, Coordinates3D b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Coordinates3D operator -(Coordinates3D a, int b)           => new(a.x - b  , a.y - b  , a.z - b);
    public static Coordinates3D operator *(Coordinates3D a, int b)           => new(a.x * b  , a.y * b  , a.z * b);
    public static Coordinates3D operator /(Coordinates3D a, int b)           => new(a.x / b  , a.y / b  , a.z / b);
    public static Coordinates3D operator -(Coordinates3D a)                  => new(-a.x, -a.y, -a.z);
     
    public static bool operator ==(Coordinates3D a, Coordinates3D b) => a.x == b.x && a.y == b.y && a.z == b.z;
    public static bool operator !=(Coordinates3D a, Coordinates3D b) => a.x != b.x || a.y != b.y || a.z != b.z;

    static List<Coordinates3D> GetAllDirections()
    {
        var allDirections = new List<Coordinates3D>();
        allDirections.AddRange(COMPASS_DIRECTIONS);
        allDirections.AddRange(EDGE_DIRECTIONS);
        allDirections.AddRange(CORNER_DIRECTIONS);
        return allDirections;
    }
}
