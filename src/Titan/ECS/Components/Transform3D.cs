using System.Numerics;
using Titan.Assets;
using Titan.Core;
using Titan.Rendering.Resources;
using Titan.Rendering.Storage;

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
    public AssetHandle<MeshAsset> Asset;
    public AssetHandle<TextureAsset> TextureAsset;
    public Handle<MeshInstance> InstanceIndex;
    public Matrix4x4 ModelMatrix;

    internal unsafe MeshData* MeshData;
    
}
