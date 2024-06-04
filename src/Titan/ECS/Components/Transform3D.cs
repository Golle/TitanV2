using System.Numerics;

namespace Titan.ECS.Components;

[Component]
public partial struct Transform3D
{
    public Vector3 Position;
    public Quaternion Rotation;
}
