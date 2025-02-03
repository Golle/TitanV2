using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Application;
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
    private const int InvalidIndex = 0;
    private const uint MaxMaterials = 256;

    private Inline2<Handle<GPUBuffer>> MaterialBuffers;
    private Inline2<MappedGPUResource<MaterialData>> GPUMaterialData;

    private TitanArray<MaterialData> Materials;
    private TitanArray<bool> FreeList;

    private SpinLock SpinLock;
    //private int MaterialCount;

    private uint DirtyCounter;

    private D3D12ResourceManager* ResourceManager;
    public readonly Handle<GPUBuffer> GetMaterialsGPUHandle(uint frameIndex)
    {
        Debug.Assert(frameIndex < GlobalConfiguration.MaxRenderFrames);
        return MaterialBuffers[frameIndex];
    }

    [System(SystemStage.Init)]
    public static void Init(MaterialsSystem* system, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager, UnmanagedResourceRegistry registry)
    {
        Debug.Assert(GlobalConfiguration.MaxRenderFrames == system->MaterialBuffers.Size);

        for (var i = 0; i < GlobalConfiguration.MaxRenderFrames; ++i)
        {
            system->MaterialBuffers[i] = resourceManager.CreateBuffer(CreateBufferArgs.Create<MaterialData>(MaxMaterials, BufferType.Structured, TitanBuffer.Empty, cpuVisible: true, true));
            if (system->MaterialBuffers[i].IsInvalid)
            {
                Logger.Error<MaterialsSystem>($"Failed to create the Material buffer. Index = {i}");
                return;
            }
            if (!resourceManager.TryMapBuffer(system->MaterialBuffers[i], out system->GPUMaterialData[i]))
            {
                Logger.Error<MaterialsSystem>($"Failed to map resource. Index = {i}");
                return;
            }
        }

        if (!memoryManager.TryAllocArray(out system->Materials, MaxMaterials))
        {
            Logger.Error<MaterialsSystem>($"Failed to allocate memory for the Materials. Count = {MaxMaterials}");
            return;
        }

        if (!memoryManager.TryAllocArray(out system->FreeList, MaxMaterials))
        {
            Logger.Error<MaterialsSystem>($"Failed to allocate memory for the freelist. Count = {MaxMaterials} Size = {sizeof(ushort) * MaxMaterials} bytes.");
            return;
        }

        // index 0 will contain an invalid/empty material
        system->Materials[0] = new()
        {
            DiffuseColor = Color.Magenta,
            TextureId = 0
        };
        system->ResourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
    }


    public Handle<MaterialData> CreateMaterial(Handle<Texture> albedoTexture, in Color diffuseColor)
    {
        var albedo = albedoTexture.IsValid ? ResourceManager->Access(albedoTexture) : null;
        return CreateMaterial(albedo, in diffuseColor);
    }

    public Handle<MaterialData> CreateMaterial(Texture* albedoTexture, in Color diffuseColor)
    {
        var index = GetNextFreeIndex();
        if (index == InvalidIndex)
        {
            // maybe we want an assert here.
            return Handle<MaterialData>.Invalid;
        }
        Materials[index] = new()
        {
            DiffuseColor = diffuseColor,
            TextureId = albedoTexture != null ? albedoTexture->GetIndex() : -1
        };
        SetDirty();
        return index;
    }

    public void DestroyMaterial(Handle<MaterialData> handle)
    {
        if (handle.IsValid)
        {
            Free(handle.Value);
        }
    }

    public readonly ref readonly MaterialData GetMaterialData(Handle<MaterialData> handle)
    {
        Debug.Assert(FreeList[handle.Value], "Accessing a material that has been freed.");
        return ref Materials[handle];
    }


    public void UpdateMaterial(Handle<MaterialData> handle, Handle<Texture> albedoTexture, in Color diffuseColor)
    {
        Debug.Assert(handle.IsValid);
        Debug.Assert(FreeList[handle.Value], "trying to update an item that has not been allocated.");
        ref var materialData = ref Materials[handle];
        materialData.TextureId = albedoTexture.IsValid
            ? ResourceManager->Access(albedoTexture)->GetIndex()
            : 0;
        materialData.DiffuseColor = diffuseColor;
        SetDirty();
    }

    public void UpdateDiffuseColor(Handle<MaterialData> handle, in Color diffuseColor)
    {
        //TODO(Jens): This is not a very good solution. We need a better way to do this.
        Debug.Assert(handle.IsValid);
        Debug.Assert(FreeList[handle.Value], "trying to update an item that has not been allocated.");
        ref var materialData = ref Materials[handle];
        materialData.DiffuseColor = diffuseColor;
        SetDirty();
    }

    [System]
    public static void Update(MaterialsSystem* system, in D3D12ResourceManager resourceManager, in RenderGraph graph)
    {
        if (system->DirtyCounter > 0)
        {
            Logger.Error<MaterialsSystem>($"Dirty Materials: Counter = {system->DirtyCounter}");
            //TODO(Jens): Check for dirty materials. Right now we keep it simple. We upload everything if the counter is greater than 0.
            system->GPUMaterialData[EngineState.FrameIndex].Write(system->Materials.AsReadOnlySpan());

            Interlocked.Decrement(ref system->DirtyCounter);
        }
    }


    [System(SystemStage.Shutdown)]
    public static void Shutdown(MaterialsSystem* system, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager)
    {
        for (var i = 0; i < GlobalConfiguration.MaxRenderFrames; ++i)
        {
            resourceManager.Unmap(system->GPUMaterialData[i]);
            resourceManager.DestroyBuffer(system->MaterialBuffers[i]);
        }
        memoryManager.FreeArray(ref system->Materials);
        memoryManager.FreeArray(ref system->FreeList);

        *system = default;
    }


    private void Free(int index)
    {
        Debug.Assert(FreeList[index] == true, "Trying to free a slot that's not in use");
        FreeList[index] = false;
    }

    private int GetNextFreeIndex()
    {
        // could use a CompareAndExchange here to avoid the spinlock. 
        var token = false;
        SpinLock.Enter(ref token);
        var index = InvalidIndex;
        for (var i = 1; i < MaxMaterials; ++i)
        {
            if (!FreeList[i])
            {
                FreeList[i] = true;
                index = i;
                break;
            }
        }
        SpinLock.Exit();
        return index;
    }

    private void SetDirty()
    {
        //NOTE(Jens): This might not be needed.
        Volatile.Write(ref DirtyCounter, GlobalConfiguration.MaxRenderFrames);
    }
}
