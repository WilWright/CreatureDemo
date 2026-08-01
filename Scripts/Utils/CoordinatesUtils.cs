using System;
using UnityEngine;

namespace Utils
{
    public static class CoordinatesUtils
    {
        public static Vector3 ToVector3(this Coordinates3D c) => new(c.x, c.y, c.z);

        public static Coordinates3D FromVector3Floor(Vector3 vector3) => new(
            Mathf.FloorToInt(vector3.x),
            Mathf.FloorToInt(vector3.y),
            Mathf.FloorToInt(vector3.z)
        );

        public static Coordinates3D FromVector3Round(Vector3 vector3) => new(
            (int)Math.Round(vector3.x, MidpointRounding.AwayFromZero),
            (int)Math.Round(vector3.y, MidpointRounding.AwayFromZero),
            (int)Math.Round(vector3.z, MidpointRounding.AwayFromZero)
        );

        public static Coordinates3D FromVector3Ceil(Vector3 vector3) => new(
            Mathf.CeilToInt(vector3.x),
            Mathf.CeilToInt(vector3.y),
            Mathf.CeilToInt(vector3.z)
        );

        public static Coordinates3D Min(Coordinates3D a, Coordinates3D b) => new(
            Mathf.Min(a.x, b.x),
            Mathf.Min(a.y, b.y),
            Mathf.Min(a.z, b.z)
        );

        public static Coordinates3D Max(Coordinates3D a, Coordinates3D b) => new(
            Mathf.Max(a.x, b.x),
            Mathf.Max(a.y, b.y),
            Mathf.Max(a.z, b.z)
        );

        public static FromZeroEnumerator EnumerateFromZero(this Coordinates3D c) => new(c);

        public static RadiusAsCubeVolumeEnumerator   EnumerateRadiusAsCubeVolume  (this Coordinates3D c, int radius) => new(c, radius);
        public static RadiusAsCubeShellEnumerator    EnumerateRadiusAsCubeShell   (this Coordinates3D c, int radius) => new(c, radius);
        public static RadiusAsSphereVolumeEnumerator EnumerateRadiusAsSphereVolume(this Coordinates3D c, int radius) => new(c, radius);
        public static RadiusAsSphereShellEnumerator  EnumerateRadiusAsSphereShell (this Coordinates3D c, int radius) => new(c, radius);

        #region Enumerators

        public struct FromZeroEnumerator
        {
            public Coordinates3D Current { get; private set; }

            readonly Coordinates3D _c;

            int _index;
            readonly int _maxIndex;

            public FromZeroEnumerator(Coordinates3D c)
            {
                Current = Coordinates3D.ZERO;

                _c = c + 1;

                _index = -1;
                _maxIndex = _c.x * _c.y * _c.z - 1;
            }

            public bool MoveNext()
            {
                if (++_index > _maxIndex)
                {
                    return false;
                }

                int i = _index / _c.x;
                int x = _index % _c.x;
                int y = i / _c.z;
                int z = i % _c.z;

                Current = new Coordinates3D(x, y, z);

                return true;
            }

            public readonly FromZeroEnumerator GetEnumerator() => this;
        }

        public struct RadiusAsCubeVolumeEnumerator
        {
            public Coordinates3D Current { get; private set; }

            int _x;
            int _y;
            int _z;

            readonly int _minX;
            readonly int _maxX;
            readonly int _minY;
            readonly int _maxY;
            readonly int _minZ;
            readonly int _maxZ;

            public RadiusAsCubeVolumeEnumerator(Coordinates3D c, int radius)
            {
                Current = c;

                _minX = c.x - radius;
                _maxX = c.x + radius;
                _minY = c.y - radius;
                _maxY = c.y + radius;
                _minZ = c.z - radius;
                _maxZ = c.z + radius;

                _x = _minX - 1;
                _y = _minY;
                _z = _minZ;
            }

            public bool MoveNext()
            {
                if (++_x > _maxX)
                {
                    _x = _minX;

                    if (++_z > _maxZ)
                    {
                        _z = _minZ;

                        if (++_y > _maxY)
                        {
                            return false;
                        }
                    }
                }

                Current = new Coordinates3D(_x, _y, _z);

                return true;
            }

            public readonly RadiusAsCubeVolumeEnumerator GetEnumerator() => this;
        }

        public struct RadiusAsCubeShellEnumerator
        {
            public Coordinates3D Current { get; private set; }

            int _x;
            int _y;
            int _z;

            readonly int _minX;
            readonly int _maxX;
            readonly int _minY;
            readonly int _maxY;
            readonly int _minZ;
            readonly int _maxZ;

            public RadiusAsCubeShellEnumerator(Coordinates3D c, int radius)
            {
                Current = c;

                _minX = c.x - radius;
                _maxX = c.x + radius;
                _minY = c.y - radius;
                _maxY = c.y + radius;
                _minZ = c.z - radius;
                _maxZ = c.z + radius;

                _x = _minX - 1;
                _y = _minY;
                _z = _minZ;
            }

            public bool MoveNext()
            {
                if (++_x > _maxX)
                {
                    _x = _minX;

                    if (++_z > _maxZ)
                    {
                        _z = _minZ;

                        if (++_y > _maxY)
                        {
                            return false;
                        }
                    }
                }

                if (_x > _minX && _x < _maxX
                 && _y > _minY && _y < _maxY
                 && _z > _minZ && _z < _maxZ)
                {
                    return MoveNext();
                }

                Current = new Coordinates3D(_x, _y, _z);

                return true;
            }

            public readonly RadiusAsCubeShellEnumerator GetEnumerator() => this;
        }

        public struct RadiusAsSphereVolumeEnumerator
        {
            public Coordinates3D Current { get; private set; }

            readonly Coordinates3D _c;

            int _x;
            int _y;
            int _z;

            readonly int _minX;
            readonly int _maxX;
            readonly int _minY;
            readonly int _maxY;
            readonly int _minZ;
            readonly int _maxZ;

            int _dyy;
            int _dzz;
            readonly int _rr;

            public RadiusAsSphereVolumeEnumerator(Coordinates3D c, int radius)
            {
                Current = _c = c;

                _rr = _dyy = _dzz = radius * radius;

                _minX = c.x - radius;
                _maxX = c.x + radius;
                _minY = c.y - radius;
                _maxY = c.y + radius;
                _minZ = c.z - radius;
                _maxZ = c.z + radius;

                _x = _minX - 1;
                _y = _minY;
                _z = _minZ;
            }

            public bool MoveNext()
            {
                if (++_x > _maxX)
                {
                    _x = _minX;

                    if (++_z > _maxZ)
                    {
                        _z = _minZ;

                        if (++_y > _maxY)
                        {
                            return false;
                        }

                        int dy = _y - _c.y;
                        _dyy = dy * dy;
                    }

                    int dz = _z - _c.z;
                    _dzz = dz * dz;
                }

                int dx = _x - _c.x;
                int d = dx * dx + _dyy + _dzz;

                if (d > _rr)
                {
                    return MoveNext();
                }

                Current = new Coordinates3D(_x, _y, _z);

                return true;
            }

            public readonly RadiusAsSphereVolumeEnumerator GetEnumerator() => this;
        }

        public struct RadiusAsSphereShellEnumerator
        {
            public Coordinates3D Current { get; private set; }

            readonly Coordinates3D _c;

            int _x;
            int _y;
            int _z;

            readonly int _minX;
            readonly int _maxX;
            readonly int _minY;
            readonly int _maxY;
            readonly int _minZ;
            readonly int _maxZ;

            int _dyy;
            int _dzz;
            readonly int _rr;
            readonly int _vv;

            public RadiusAsSphereShellEnumerator(Coordinates3D c, int radius)
            {
                Current = _c = c;

                _rr = _dyy = _dzz = radius * radius;
                _vv = (radius - 1) * (radius - 1);

                _minX = c.x - radius;
                _maxX = c.x + radius;
                _minY = c.y - radius;
                _maxY = c.y + radius;
                _minZ = c.z - radius;
                _maxZ = c.z + radius;

                _x = _minX - 1;
                _y = _minY;
                _z = _minZ;
            }

            public bool MoveNext()
            {
                if (++_x > _maxX)
                {
                    _x = _minX;

                    if (++_z > _maxZ)
                    {
                        _z = _minZ;

                        if (++_y > _maxY)
                        {
                            return false;
                        }

                        int dy = _y - _c.y;
                        _dyy = dy * dy;
                    }

                    int dz = _z - _c.z;
                    _dzz = dz * dz;
                }

                int dx = _x - _c.x;
                int d = dx * dx + _dyy + _dzz;

                if (d > _rr || d <= _vv)
                {
                    return MoveNext();
                }

                Current = new Coordinates3D(_x, _y, _z);

                return true;
            }

            public readonly RadiusAsSphereShellEnumerator GetEnumerator() => this;
        }

        #endregion
    }
}
