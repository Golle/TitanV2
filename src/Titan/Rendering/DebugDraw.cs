using System.Diagnostics;
using System.Numerics;
using Titan.Core.Maths;
using Titan.ECS.Components;
using Titan.Rendering.RenderPasses;

namespace Titan.Rendering;

public static unsafe class DebugDraw
{
    internal static DebugRenderPass* DebugAPI;

    [Conditional("DEBUG")]
    public static void DrawAABB(in AABB aabb)
        => DrawAABB(aabb, Color.White);

    [Conditional("DEBUG")]
    public static void DrawAABB(in AABB aabb, in Color color)
    {
        Debug.Assert(DebugAPI != null);
        DebugAPI->DrawAABB(aabb, color);
    }

    [Conditional("DEBUG")]
    public static void DrawLine(Vector3 start, Vector3 end)
        => DrawLine(start, end, Color.White);

    [Conditional("DEBUG")]
    public static void DrawLine(Vector3 start, Vector3 end, in Color color)
        => DrawLine(start, end, color, color);

    [Conditional("DEBUG")]
    public static void DrawLine(Vector3 start, Vector3 end, in Color colorStart, in Color colorEnd)
    {
        Debug.Assert(DebugAPI != null);
        DebugAPI->DrawLine(start, end, colorStart, colorEnd);
    }
    
    [Conditional("DEBUG")]
    public static void DrawBox(Vector3 center, int size, in Color color)
    {
        var multiple = new Vector3(size);
        var aabb = new AABB
        {
            Max = center + multiple,
            Min = center - multiple
        };

        DrawAABB(aabb, color);
    }
}
