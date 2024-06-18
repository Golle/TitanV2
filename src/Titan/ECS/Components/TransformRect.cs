using Titan.Core.Maths;

namespace Titan.ECS.Components;

[Component]
public partial struct TransformRect
{
    public Point Position;
    public int ZIndex;
}
