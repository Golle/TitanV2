using System.Numerics;

namespace Titan.ECS.Components;

[Component]
public partial struct Transform3D
{

    public const ulong Id = 10;
    public Vector3 Position;
    public Quaternion Rotation;
}
