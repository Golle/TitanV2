using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Upload;
using Titan.Platform.Win32;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.Storage;

/// <summary>
/// Data stored on the GPU that's required for each model. (Maybe root signature?)
/// </summary>

[StructLayout(LayoutKind.Sequential)]
internal struct MeshInstanceData
{
    public uint VertexIndex;
    public uint MaterialIndex;
    public TitanArray<SubmeshData> Submeshes;
}


/// <summary>
/// The mesh information, stored on the CPU side.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MeshInstance
{
    //public TitanArray<MeshInstanceSubmesh> Submeshes;

}

public ref struct CreateMeshArgs
{
    public required ReadOnlySpan<Vertex> Vertices { get; init; }
    public required ReadOnlySpan<SubMesh> SubMeshes { get; init; }
    public ReadOnlySpan<uint> Indices { get; init; }
}

[StructLayout(LayoutKind.Sequential)]
internal struct SubmeshData
{
    public uint IndexOffset;
    public uint IndexCount;
    public uint VertexOffset;
    public uint VertexCount;
}


[UnmanagedResource]
internal unsafe partial struct MeshStorage
{
    public Handle<Buffer> IndexBufferHandle;
    public Handle<Buffer> VertexBufferHandle;
    public Handle<Buffer> MeshInstancesHandle;

    private ulong _indexBufferOffset;
    private ulong _vertexBufferOffset;
    private uint _meshInstanceOffset;

    private MeshInstance* _gpuInstances;
    private TitanArray<MeshInstanceData> _cpuInstances;

    private D3D12ResourceManager* _resourceManager;
    private D3D12UploadQueue* _uploadQueue;

    private GeneralAllocator _allocator;
    private SpinLock _allocatorLock;

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

        if (!memoryManager.TryAllocArray(out storage->_cpuInstances, meshCount))
        {
            Logger.Error<MeshStorage>("Failed to allocate buffer for meshes.");
            return;
        }

        if (!memoryManager.TryCreateGeneralAllocator(out storage->_allocator, MemoryUtils.KiloBytes(4), MemoryUtils.KiloBytes(4)))
        {
            Logger.Error<MeshStorage>("Failed to create the allocator for submeshes.");
            return;
        }

        storage->_resourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
        storage->_uploadQueue = registry.GetResourcePointer<D3D12UploadQueue>();

    }


    public Handle<MeshInstance> CreateMesh(in CreateMeshArgs args)
    {
        var meshIndex = Interlocked.Increment(ref _meshInstanceOffset); // first allocation will be offset 1

        var data = _cpuInstances.GetPointer(meshIndex);
        data->Submeshes = AllocSubmeshData((uint)args.SubMeshes.Length);
        foreach (ref readonly var submesh in args.SubMeshes)
        {
            //args.SubMeshes
        }
        var vertexBuffer = _resourceManager->Access(VertexBufferHandle);
        var indexBuffer = _resourceManager->Access(IndexBufferHandle);

        var numberOfVertices = args.Vertices.Length;
        var numberOfIndices = args.Indices.Length;
        var numberOfSubmeshes = args.SubMeshes.Length;

        fixed (Vertex* pVertices = args.Vertices)
        {
            //TODO(Jens): need support for offsets/regions for uploads
            _uploadQueue->Upload(vertexBuffer->Resource, new(pVertices, (uint)(numberOfVertices * sizeof(Vertex))));
        }

        //TODO(Jens): Add support for 2 bytes indices
        fixed (uint* pIndices = args.Indices)
        {
            //TODO(Jens): need support for offsets/regions for uploads
            _uploadQueue->Upload(indexBuffer->Resource, new(pIndices, (uint)(args.Indices.Length * sizeof(uint))));
        }


        //MemoryUtils.Copy(_gpuInstances + offset, &instance, sizeof(MeshInstance));

        return meshIndex;
    }


    public readonly MeshInstance* Access(in Handle<MeshInstance> handle)
    {

        return null;
    }

    public void DestroyMesh(Handle<MeshInstance> handle)
    {
        Logger.Warning<MeshStorage>("Destroy haas not been implemented.");
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(ref MeshStorage storage, in D3D12ResourceManager resourceManager)
    {




    }

    private TitanArray<SubmeshData> AllocSubmeshData(uint count)
    {
        if (count == 0)
        {
            return TitanArray<SubmeshData>.Empty;
        }

        var lockTaken = false;
        _allocatorLock.Enter(ref lockTaken);
        try
        {
            return _allocator.AllocArray<SubmeshData>(count);
        }
        finally
        {
            _allocatorLock.Exit();
        }
    }
}
