using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Rendering;
using Titan.Rendering.RenderPasses;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Meshes;

[UnmanagedResource]
internal unsafe partial struct MeshInstanceSystem
{
    private Inline2<Handle<GPUBuffer>> MeshInstances;

    [System(SystemStage.Init)]
    public static void Init(MeshInstanceSystem* system, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager)
    {
        var maxCount = 1024u;

        for (var i = 0; i < GlobalConfiguration.CommandBufferCount; ++i)
        {
            system->MeshInstances[i] = resourceManager.CreateBuffer(CreateBufferArgs.Create<MeshInstanceData>(maxCount, BufferType.Structured, cpuVisible: true, shaderVisible: true));
        }
    }


    [System]
    public static void Update()
    {

    }


    [System(SystemStage.Shutdown)]
    public static void Shutdown()
    {
        Logger.Warning<MeshInstanceSystem>("Shutdown not implemented");
    }
}
