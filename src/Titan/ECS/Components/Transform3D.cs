using System.Numerics;
using Titan.Assets;
using Titan.Graphics.Resources;

namespace Titan.ECS.Components;

[Component]
public partial struct Transform3D
{
    public Vector3 Position;
    public Quaternion Rotation;
}

[Component]
internal partial struct Mesh3D
{
    public uint Offset;
    public uint Count;
    public AssetHandle<MeshAsset> Asset;
}
