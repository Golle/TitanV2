using Titan.Core.Maths;

namespace Titan.ECS.Components;

[Component]
public partial struct TransformRect
{
    public const ulong Id = 20;
    public Point Position;
    public int ZIndex;
}
