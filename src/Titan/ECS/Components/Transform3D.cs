using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Maths;
using Titan.Materials;
using Titan.Meshes;

namespace Titan.ECS.Components;

/// <summary>
/// A basic Transform3D component that supports Scale, Rotation and Position in a 3D world.
/// <remarks>Use the Create functions to get proper default values.</remarks>
/// </summary>
[Component]
public partial struct Transform3D
{
    public Vector3 Position;
    public Vector3 Scale;
    public Quaternion Rotation;
    public static Transform3D Create() => Create(Vector3.Zero, Vector3.One, Quaternion.Identity);
    public static Transform3D Create(in Vector3 position) => Create(position, Vector3.One, Quaternion.Identity);
    public static Transform3D Create(in Vector3 position, in Vector3 scale) => Create(position, scale, Quaternion.Identity);
    public static Transform3D Create(in Vector3 position, in Quaternion rotation) => Create(position, Vector3.One, rotation);
    public static Transform3D Create(in Vector3 position, in Vector3 scale, in Quaternion rotation) => new() { Position = position, Scale = scale, Rotation = rotation };
}

[Component]
public partial struct Mesh
{
    //public Handle<MaterialData> MaterialIndex;
    public Handle<MeshData> MeshIndex;

    //TODO(Jens): For static meshes this can be calculated in the asset pipeline
    public AABB BoundingBox;
}

public struct AABB
{
    public Vector3 Min;
    public Vector3 Max;
}

[Component]
[StructLayout(LayoutKind.Explicit)]
public partial struct Light
{
    [FieldOffset(0)]
    public LightType LightType;
    [FieldOffset(1)]
    internal short LightIndex;
    [FieldOffset(3)]
    public bool Active;
    [FieldOffset(4)]
    public ColorRGB Color;
    [FieldOffset(16)]
    public float Radius;
    [FieldOffset(16)]
    public float Intensity;
    [FieldOffset(20)]
    public Vector3 Direction;
}

public enum LightType : byte
{
    Point,
    Directional
}
