using System.Numerics;
using Titan.Core.Maths;

namespace Titan.ECS.Components;

[Component]
public partial struct Transform3D
{
    public Vector3 Position;
    public Quaternion Rotation;
}


[Component]
public partial struct TransformRect
{
    public Point Position;
    public int ZIndex;
}
