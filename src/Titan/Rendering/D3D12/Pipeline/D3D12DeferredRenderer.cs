using Titan.ECS.Components;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.D3D12.Pipeline;

[UnmanagedResource]
internal partial struct D3D12DeferredRenderer
{
    public static void Init(ref D3D12DeferredRenderer renderer)
    {



    }


    [System]
    public static void Render(in D3D12DeferredRenderer renderer, ReadOnlySpan<Transform3D> transforms, ReadOnlySpan<Mesh3D> meshes)
    {
        if (transforms.IsEmpty)
        {
            return;
        }
    }
}
