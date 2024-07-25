using System.Numerics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.D3D12.Renderers;

/// <summary>
/// A text renderer that is used for debug text
/// </summary>
[UnmanagedResource]
internal unsafe partial struct D3D12TextRenderer
{
    private struct TextEntry
    {
        public TitanArray<byte> Text;
        public Vector2 Position;
    }

    private static readonly uint TextMaxSize = MemoryUtils.MegaBytes(8);
    private const uint MaxEntries = 1024;
    private AtomicBumpAllocator _allocator;
    private TitanArray<TextEntry> _entries;
    private volatile uint _count;

    [System(SystemStage.Init)]
    public static void Init(D3D12TextRenderer* renderer, AssetsManager assetsManager, IMemoryManager memoryManager)
    {
        if (!memoryManager.TryCreateAtomicBumpAllocator(out renderer->_allocator, TextMaxSize))
        {
            Logger.Error<D3D12TextRenderer>($"Failed to create the bump allocator. Size = {TextMaxSize} bytes");
            return;
        }

        //var vs = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.DebugTextVertexShader);
        //var ps = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.DebugTextPixelShader);

        //Logger.Error<D3D12TextRenderer>($"Yay! {vs.Index} {ps.Index}");
    }

    public void DrawText(in Vector2 position, ReadOnlySpan<byte> text)
    {
        //NOTE(Jens): Build a PSO registry, where you can just add whatever you want and you get a PSO back. 
        ref var entry = ref _entries[Interlocked.Increment(ref _count) - 1];
        entry.Position = position;

        MemoryUtils.AsPointer(_allocator)->AllocateArray<byte>(text.Length);

        var stuff = _allocator.AllocateArray<byte>(text.Length);
        text.CopyTo(stuff.AsSpan());
    }

    [System(SystemStage.PreUpdate, SystemExecutionType.Inline)]
    internal static void ResetAllocator(ref D3D12TextRenderer renderer)
    {
        ref var allocator = ref renderer._allocator;
        allocator.Reset();
        renderer._entries = allocator.AllocateArray<TextEntry>(MaxEntries);
        renderer._count = 0;
    }

    [System(SystemStage.PostUpdate)]
    public static void PostUpdate(D3D12TextRenderer* renderer)
    {

    }
}
