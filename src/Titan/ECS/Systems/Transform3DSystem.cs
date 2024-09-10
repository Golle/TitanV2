using System.Runtime.CompilerServices;
using Titan.Core.Maths;
using Titan.ECS.Components;
using Titan.Systems;

namespace Titan.ECS.Systems;

internal partial struct Transform3DSystem
{
    [System]
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void Update(ReadOnlySpan<Transform3D> transforms, Span<Mesh> meshes)
    {
        var count = meshes.Length;
        for (var i = 0; i < count; ++i)
        {
            ref readonly var transform = ref transforms[i];
            MathUtils.CalculateWorldMatrix(ref meshes[i].ModelMatrix, transform.Position, transform.Scale, transform.Rotation);
        }
    }
}
