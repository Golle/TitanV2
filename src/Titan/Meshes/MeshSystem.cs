using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Materials;
using Titan.Rendering;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Meshes;


[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct MeshData
{
    public uint VertexStartLocation;
    public byte SubMeshCount;
    public Inline8<SubMeshData> SubMeshes;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<SubMeshData> GetSubmeshes()
        => SubMeshes.AsReadOnlySpan()[..SubMeshCount];
}

public struct SubMeshData
{
    public uint IndexStartLocation;
    public uint IndexCount;
    public uint MaterialIndex;
}


public ref struct MeshArgs
{
    public required ReadOnlySpan<Vertex> Vertices;
    public required ReadOnlySpan<uint> Indicies;
    public ReadOnlySpan<SubMesh> SubMeshes;
    public ReadOnlySpan<Handle<MaterialData>> Materials;
}

[UnmanagedResource]
internal unsafe partial struct MeshSystem
{
    private D3D12ResourceManager* ResourceManager;

    private Handle<GPUBuffer> StaticVertexBuffer;
    private Handle<GPUBuffer> StaticIndexBuffer;

    private uint NextVertex;
    private uint NextIndex;

    private ResourcePool<MeshData> MeshData;
    private SpinLock Lock;
    public readonly Handle<GPUBuffer> GetVertexBufferHandle() => StaticVertexBuffer;
    public readonly Handle<GPUBuffer> GetIndexBufferHandle() => StaticIndexBuffer;

    [System(SystemStage.Init)]
    public static void Init(MeshSystem* system, in D3D12ResourceManager resourceManager, IConfigurationManager configurationManager, IMemoryManager memoryManager, UnmanagedResourceRegistry registry)
    {
        //TODO(Jens): Use configurable variables for this. We'll use a lot more than 256 :D
        //TODO(Jens): Use Virtual GPU memory(if it works similar to how virtual memory does)
        var maxMeshCount = 256u;
        var vertexMemorySize = MemoryUtils.MegaBytes(256);
        var indexMemorySize = MemoryUtils.MegaBytes(64);
        var vertexCount = (uint)(vertexMemorySize / sizeof(Vertex));
        var indexCount = (uint)(indexMemorySize / sizeof(uint));

        system->StaticVertexBuffer = resourceManager.CreateBuffer(CreateBufferArgs.Create<Vertex>(vertexCount, BufferType.Vertex, cpuVisible: false, shaderVisible: true));
        system->StaticIndexBuffer = resourceManager.CreateBuffer(CreateBufferArgs.Create<uint>(indexCount, BufferType.Vertex, cpuVisible: false, shaderVisible: true));

        if (system->StaticVertexBuffer.IsInvalid)
        {
            Logger.Error<MeshSystem>($"Failed to create the {nameof(StaticVertexBuffer)}.");
            return;
        }

        if (system->StaticIndexBuffer.IsInvalid)
        {
            Logger.Error<MeshSystem>($"Failed to create the {nameof(StaticIndexBuffer)}.");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out system->MeshData, maxMeshCount))
        {
            Logger.Error<MeshSystem>($"Failed to craete the {nameof(ResourcePool<MeshData>)}.");
            return;
        }

        system->ResourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
    }

    public Handle<MeshData> CreateMesh(in MeshArgs args)
    {
        var indexCount = (uint)args.Indicies.Length;
        var vertexCount = (uint)args.Vertices.Length;

        Debug.Assert(indexCount > 0);
        Debug.Assert(args.Vertices.Length > 0);
        var handle = MeshData.SafeAlloc();
        if (handle.IsInvalid)
        {
            return handle;
        }

        var data = MeshData.AsPtr(handle);
        Debug.Assert(args.SubMeshes.Length <= data->SubMeshes.Size);
        data->VertexStartLocation = GetNextVertexStartLocation(vertexCount);
        var indexStartLocation = GetNextIndicesStartLocation(indexCount);
        // We only support submeshes, so when no submeshes are available we just add a single submesh with all the indices.
        if (args.SubMeshes.IsEmpty)
        {
            data->SubMeshCount = 1;
            data->SubMeshes[0].IndexStartLocation = indexStartLocation;
            data->SubMeshes[0].IndexCount = indexCount;
            Debug.Fail("This has not been implemented, not sure what data we have here. Fix when this occurs.");
            //data->SubMeshes[0].MaterialIndex = ?
        }
        else
        {
            foreach (var submesh in args.SubMeshes)
            {
                data->SubMeshes[data->SubMeshCount++] = new()
                {
                    IndexCount = (uint)submesh.IndexCount,
                    IndexStartLocation = (uint)(indexStartLocation + submesh.IndexOffset),
                    MaterialIndex = args.Materials[submesh.MaterialIndex] //NOTE(Jens): This will always be set in the current implementation.
                };
            }
        }

        fixed (Vertex* vertices = args.Vertices)
        fixed (uint* indices = args.Indicies)
        {
            if (!ResourceManager->Upload(StaticVertexBuffer, new TitanBuffer(vertices, (uint)(sizeof(Vertex) * vertexCount)), data->VertexStartLocation))
            {
                Logger.Error<MeshSystem>("Failed to upload Vertices.");
                MeshData.SafeFree(handle); // TODO: slot is lost, neeeeed to fix.
                return Handle<MeshData>.Invalid;
            }

            if (!ResourceManager->Upload(StaticIndexBuffer, new TitanBuffer(indices, sizeof(uint) * indexCount), data->SubMeshes[0].IndexStartLocation))
            {
                Logger.Error<MeshSystem>("Failed to upload Indices.");
                MeshData.SafeFree(handle); // TODO: slot is lost, neeeeed to fix.
                return Handle<MeshData>.Invalid;
            }
        }

        return handle;
    }

    public void DestroyMesh(Handle<MeshData> handle)
    {
        Debug.Assert(handle.IsValid);
        var gotToken = false;
        Lock.Enter(ref gotToken);

        // need to implement something proper that can merge blocks.
        Logger.Warning<MeshSystem>("Destroy mesh has not been implemented");
        Lock.Exit();
    }

    private uint GetNextIndicesStartLocation(uint count)
    {
        var size = (uint)sizeof(uint) * count;
        return Interlocked.Add(ref NextIndex, size) - size;
        //var alignedIncrement = MemoryUtils.AlignToUpper((uint)(sizeof(uint) * count), 256);
        //return Interlocked.Add(ref NextIndex, alignedIncrement) - alignedIncrement;
    }

    private uint GetNextVertexStartLocation(uint count)
    {
        var size = (uint)sizeof(Vertex) * count;
        return Interlocked.Add(ref NextVertex, size) - size;
        //var alignedIncrement = MemoryUtils.AlignToUpper((uint)(sizeof(Vertex) * count), 256);
        //return Interlocked.Add(ref NextVertex, alignedIncrement) - alignedIncrement;
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(MeshSystem* system, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager)
    {
        Logger.Warning<MeshSystem>("DEferred destruction not implemented yet. :<");
        //resourceManager.DestroyBuffer(system->StaticIndexBuffer);
        //resourceManager.DestroyBuffer(system->StaticVertexBuffer);
        memoryManager.FreeResourcePool(ref system->MeshData);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly MeshData* Access(Handle<MeshData> handle)
    {
        Debug.Assert(handle.IsValid);
        return MeshData.AsPtr(handle);
    }
}
