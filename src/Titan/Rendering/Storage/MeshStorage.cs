using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.ECS.Components;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Upload;
using Titan.Platform.Win32;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.Storage;

/// <summary>
/// Represents a Mesh
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MeshData
{
    public uint VertexBufferIndex;

    //NOTE(Jens): Maybe we can use Inline8,16,32 here? Not sure how many unique meshes we'll support in a game. Less complex for a bit more memory
    public TitanArray<SubmeshData> Submeshes;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SubmeshData
{
    public uint StartIndexLocation;
    public uint IndexCount;
}


/// <summary>
/// This is stored on the GPU, have to be 16 byte aligned.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MeshInstance
{
    public Matrix4x4 ModelMatrix;
    public int AlbedoIndex;
    private unsafe fixed float _padding[3];
}

public ref struct CreateMeshArgs
{
    public required ReadOnlySpan<Vertex> Vertices { get; init; }
    public required ReadOnlySpan<SubMesh> SubMeshes { get; init; }
    public ReadOnlySpan<uint> Indices { get; init; }
}

[UnmanagedResource]
internal unsafe partial struct MeshStorage
{
    public Handle<Buffer> IndexBufferHandle;
    public Handle<Buffer> VertexBufferHandle;
    public Handle<Buffer> MeshInstancesHandle;

    private ulong _indexBufferOffset;
    private ulong _vertexBufferOffset;
    private uint _meshDataIndex;
    private uint _meshInstanceIndex;

    private MeshInstance* _gpuInstances;

    private ResourcePool<MeshData> _meshData;

    private D3D12ResourceManager* _resourceManager;
    private D3D12UploadQueue* _uploadQueue;

    private GeneralAllocator _allocator;
    private SpinLock _allocatorLock;

    [System(SystemStage.Init)]
    public static void Init(MeshStorage* storage, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager, IConfigurationManager configurationManager, UnmanagedResourceRegistry registry)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        var indicesCount = config.MemoryConfig.InitialIndexBufferSize / sizeof(uint);
        var verticesCount = config.MemoryConfig.InitialVertexBufferSize / (uint)sizeof(Vertex);
        var meshCount = config.Resources.MaxMeshes;

        //TODO(Jens): Make these buffers not CPU Visible, we want to use async upload queues, but for now we keep it simple.
        storage->IndexBufferHandle = resourceManager.CreateBuffer(CreateBufferArgs.Create<uint>(indicesCount, BufferType.Index, cpuVisible: true, shaderVisible: false));
        storage->VertexBufferHandle = resourceManager.CreateBuffer(CreateBufferArgs.Create<Vertex>(verticesCount, BufferType.Vertex, cpuVisible: true, shaderVisible: true));
        storage->MeshInstancesHandle = resourceManager.CreateBuffer(CreateBufferArgs.Create<MeshInstance>(meshCount, BufferType.Structured, cpuVisible: true, shaderVisible: true));

        if (storage->IndexBufferHandle.IsInvalid)
        {
            Logger.Error<MeshStorage>($"Failed to init the Indexbuffer. Size = {indicesCount} bytes.");
            return;
        }

        if (storage->VertexBufferHandle.IsInvalid)
        {
            Logger.Error<MeshStorage>($"Failed to create the VertexBuffer. Size = {verticesCount} bytes.");
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

        if (!memoryManager.TryCreateResourcePool(out storage->_meshData, meshCount))
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


    [System(SystemStage.PreUpdate)]
    public static void PostUpdate(ref MeshStorage storage, Span<Mesh> meshes, in AssetsManager assetsManager)
    {
        foreach (ref var mesh in meshes)
        {
            if (mesh.InstanceIndex.IsInvalid)
            {
                //NOTE(Jens): Start at 1, but we should replace this with a free list
                mesh.InstanceIndex = ++storage._meshInstanceIndex;
            }

            //NOTE(Jens): I don't like this. We need a better approach.
            if (mesh.MeshData == null && assetsManager.IsLoaded(mesh.Asset))
            {
                ref readonly var asset = ref assetsManager.Get(mesh.Asset);
                mesh.MeshData = storage._meshData.AsPtr(asset.MeshDataHandle);
            }
        }
    }

    /// <summary>
    /// Creates and uploads a mesh to the GPU. 
    /// </summary>
    /// <param name="args">Args containing submeshes, vertices, indices</param>
    /// <returns>The handle to the instance</returns>
    public Handle<MeshData> CreateMesh(in CreateMeshArgs args)
    {
        var handle = _meshData.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<MeshStorage>("Failed to allocate a slot for the mesh.");
            return Handle<MeshData>.Invalid;
        }
        var data = _meshData.AsPtr(handle);
        data->Submeshes = AllocSubmeshData((uint)args.SubMeshes.Length);
        data->VertexBufferIndex = 0; //TODO(Jens): this should be calculated when we allocate the buffers.
        var indexBufferStart = 0; //TODO(Jens): this should be calculated when we allocate the buffers.

        for (var i = 0; i < args.SubMeshes.Length; ++i)
        {
            ref readonly var submesh = ref args.SubMeshes[i];
            data->Submeshes[i].IndexCount = (uint)submesh.IndexCount;
            // we calculate the absolute offset here
            data->Submeshes[i].StartIndexLocation = (uint)(indexBufferStart + submesh.IndexOffset);
        }

        var vertexBuffer = _resourceManager->Access(VertexBufferHandle);
        var indexBuffer = _resourceManager->Access(IndexBufferHandle);

        var numberOfVertices = args.Vertices.Length;
        var numberOfIndices = args.Indices.Length;

        fixed (Vertex* pVertices = args.Vertices)
        {
            //TODO(Jens): need support for offsets/regions for uploads
            _uploadQueue->Upload(vertexBuffer->Resource, new(pVertices, (uint)(numberOfVertices * sizeof(Vertex))));
        }

        //TODO(Jens): Add support for 2 bytes indices
        fixed (uint* pIndices = args.Indices)
        {
            //TODO(Jens): need support for offsets/regions for uploads
            _uploadQueue->Upload(indexBuffer->Resource, new(pIndices, (uint)(numberOfIndices * sizeof(uint))));
        }

        return handle;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly MeshData* Access(in Handle<MeshData> handle)
        => _meshData.AsPtr(handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void UpdateMeshInstance(in Handle<MeshInstance> handle, MeshInstance data)
    {
        //NOTE(Jens): We can skip this check, if the handle is invalid we'd just write to an empty slot.
        Debug.Assert(handle.IsValid);
        byte* data1;
        var id3D12Resource = _resourceManager->Access(MeshInstancesHandle)->Resource.Get();
        id3D12Resource->Map(0, null, (void**)&data1);
        MemoryUtils.Copy(data1 + (sizeof(MeshInstance) * handle.Value), &data, sizeof(MeshInstance));
        id3D12Resource->Unmap(0, null);

        //MemoryUtils.Copy(_gpuInstances + handle.Value, &data, sizeof(MeshInstance));
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
