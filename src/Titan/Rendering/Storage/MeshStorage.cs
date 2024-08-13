using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.Storage;


[StructLayout(LayoutKind.Sequential)]
internal struct MeshInstance
{
    public Matrix4x4 ModelMatrix;
    public Color TestColor;
}

public ref struct CreateMeshArgs<TIndexType> where TIndexType : unmanaged
{
    public required ReadOnlySpan<Vertex> Vertices { get; init; }
    public required ReadOnlySpan<SubMesh> SubMeshes { get; init; }
    public ReadOnlySpan<TIndexType> Indices { get; init; }

}
[UnmanagedResource]
internal unsafe partial struct MeshStorage
{
    public Handle<Buffer> IndexBufferHandle;
    public Handle<Buffer> VertexBufferHandle;
    public Handle<Buffer> MeshInstancesHandle;
    //private TitanArray<int> FreeList;
    private D3D12ResourceManager* _resourceManager;

    private uint _next;
    private MeshInstance* _gpuInstances;

    [System(SystemStage.Init)]
    public static void Init(MeshStorage* storage, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager, IConfigurationManager configurationManager, UnmanagedResourceRegistry registry)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        var indexBufferSize = config.MemoryConfig.InitialIndexBufferSize / sizeof(uint);
        var vertexBufferSize = config.MemoryConfig.InitialVertexBufferSize / (uint)sizeof(Vertex);
        var meshCount = config.Resources.MaxMeshes;

        //TODO(Jens): Make these buffers not CPU Visible, we want to use async upload queues, but for now we keep it simple.
        storage->IndexBufferHandle = resourceManager.CreateBuffer(new CreateBufferArgs(indexBufferSize, sizeof(uint), BufferType.Index) { CpuVisible = true, ShaderVisible = false });
        storage->VertexBufferHandle = resourceManager.CreateBuffer(new CreateBufferArgs(vertexBufferSize, sizeof(Vertex), BufferType.Vertex) { CpuVisible = true, ShaderVisible = true });
        storage->MeshInstancesHandle = resourceManager.CreateBuffer(new CreateBufferArgs(meshCount, sizeof(MeshInstance), BufferType.Constant) { CpuVisible = true, ShaderVisible = true });

        if (storage->IndexBufferHandle.IsInvalid)
        {
            Logger.Error<MeshStorage>($"Failed to init the Indexbuffer. Size = {indexBufferSize} bytes.");
            return;
        }

        if (storage->VertexBufferHandle.IsInvalid)
        {
            Logger.Error<MeshStorage>($"Failed to create the VertexBuffer. Size = {vertexBufferSize} bytes.");
            return;
        }

        if (storage->MeshInstancesHandle.IsInvalid)
        {
            Logger.Error<MeshStorage>($"Failed to create the ConstantBuffer. Size = {sizeof(MeshInstance) * meshCount}");
            return;
        }

        if (Win32Common.FAILED(resourceManager.Access(storage->MeshInstancesHandle)->Resource.Get()->Map(0, null, (void**)&storage->_gpuInstances)))
        {
            Logger.Error<MeshStorage>("Failed to map the MeshInstances.");
            return;
        }

        storage->_resourceManager = registry.GetResourcePointer<D3D12ResourceManager>();

    }


    public Handle<MeshInstance> CreateMesh<T>(in CreateMeshArgs<T> args) where T : unmanaged, INumber<T>
    {
        var offset = Interlocked.Increment(ref _next); // first allocation will be offset 1
        // allcate a handle
        // allocate a GPU buffer

        var instance = new MeshInstance
        {
            ModelMatrix = Matrix4x4.Identity,
            TestColor = offset == 1 ? Color.Yellow : Color.Green
        };

        MemoryUtils.Copy(_gpuInstances + offset, &instance, sizeof(MeshInstance));

        return offset;
    }


    public void DestroyMesh(Handle<MeshInstance> handle)
    {
        Logger.Warning<MeshStorage>("Destroy haas not been implemented.");
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(ref MeshStorage storage, in D3D12ResourceManager resourceManager)
    {




    }
}
