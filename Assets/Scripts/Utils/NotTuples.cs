using UnityEngine;
using System;

// These are definitely not C# 7.2+ tuples in a C# 6.0-compatible format.
namespace Utils.Tuples
{
    [Serializable]
    public struct Orientation
    {
        public Vector3 pos, rot;

        public Orientation(Vector3 pos, Vector3 rot)
        {
            this.pos = pos;
            this.rot = rot;
        }

        public bool IsOrigin()
            => pos == Vector3.zero && rot == Vector3.zero;

        public static Orientation Origin()
            => new Orientation(Vector3.zero, Vector3.zero);
    }

    [Serializable]
    public struct Path
    {
        public string directory, name;

        public Path(string directory, string name)
        {
            this.directory = directory;
            this.name = name;
        }
    }

    [Serializable]
    public struct Rational
    {
        public int num, denom;

        public Rational(int num, int denom)
        {
            this.num = num;
            this.denom = denom;
        }
    }

    [Serializable]
    public struct VBounds
    {
        
        public Vector3 min, max;

        public VBounds(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public struct NestedData
    {
        public CustomInput.KeyData.AbstractData parent;
        public CustomInput.KeyData.SimpleData simple;

        public NestedData(CustomInput.KeyData.AbstractData parent, CustomInput.KeyData.SimpleData simple)
        {
            this.parent = parent;
            this.simple = simple;
        }
    }
}