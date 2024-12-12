using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Rendering;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Materials;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MaterialData
{
    public Color DiffuseColor;
    public int TextureId;
    private fixed float Padding[3];
}


[UnmanagedResource]
internal unsafe partial struct MaterialsSystem
{
    private Inline2<Handle<GPUBuffer>> MaterialBuffers;
    private Inline2<MappedGPUResource<MaterialData>> GPUMaterialData;
    private TitanArray<MaterialData> Materials;
    private int MaterialCount;

    public readonly Handle<GPUBuffer> GetMaterialsGPUHandle(uint frameIndex)
    {
        Debug.Assert(frameIndex < GlobalConfiguration.MaxRenderFrames);
        return MaterialBuffers[frameIndex];
    }

    [System(SystemStage.Init)]
    public static void Init(MaterialsSystem* storage, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager)
    {
        Debug.Assert(GlobalConfiguration.MaxRenderFrames == storage->MaterialBuffers.Size);

        const uint MaxMaterials = 256;

        for (var i = 0; i < GlobalConfiguration.MaxRenderFrames; ++i)
        {
            storage->MaterialBuffers[i] = resourceManager.CreateBuffer(CreateBufferArgs.Create<MaterialData>(MaxMaterials, BufferType.Structured, TitanBuffer.Empty, cpuVisible: true, true));
            if (storage->MaterialBuffers[i].IsInvalid)
            {
                Logger.Error<MaterialsSystem>($"Failed to create the Material buffer. Index = {i}");
                return;
            }
            if (!resourceManager.TryMapBuffer(storage->MaterialBuffers[i], out storage->GPUMaterialData[i]))
            {
                Logger.Error<MaterialsSystem>($"Failed to map resource. Index = {i}");
                return;
            }
        }

        if (!memoryManager.TryAllocArray(out storage->Materials, MaxMaterials))
        {
            Logger.Error<MaterialsSystem>($"Failed to allocate memory for the Materials. Count = {MaxMaterials}");
            return;
        }
        // index 0 will contain an invalid/empty material
        storage->MaterialCount = 1;
    }

    public Handle<MaterialData> CreateMaterial(Texture* albedoTexture, in Color color)
    {
        var index = Interlocked.Increment(ref MaterialCount) - 1;
        Materials[index] = new()
        {
            DiffuseColor = color,
            TextureId = albedoTexture != null ? albedoTexture->GetIndex() : -1
        };
        return index;
    }


    [System]
    public static void Update(MaterialsSystem* storage, in D3D12ResourceManager resourceManager, in RenderGraph graph)
    {
        if (storage->MaterialCount > 0)
        {
            //TODO(Jens): Check for dirty materials. Right now we keep it simple.
            //NOTE(Jens): Implement dirty flag as a byte, start with value 2 and decrease for each update. This will ensure that all buffers have the same value.
            var updatedMaterials = storage->Materials.AsReadOnlySpan()[..storage->MaterialCount];
            storage->GPUMaterialData[graph.FrameIndex].Write(updatedMaterials);
        }
    }


    [System(SystemStage.Shutdown)]
    public static void Shutdown(MaterialsSystem* storage, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager)
    {
        for (var i = 0; i < GlobalConfiguration.MaxRenderFrames; ++i)
        {
            resourceManager.Unmap(storage->GPUMaterialData[i]);
            resourceManager.DestroyBuffer(storage->MaterialBuffers[i]);
        }
        memoryManager.FreeArray(ref storage->Materials);
    }
}
