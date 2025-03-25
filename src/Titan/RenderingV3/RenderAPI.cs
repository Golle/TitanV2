using System.Diagnostics;
using Titan.Core;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering;

namespace Titan.RenderingV3;

public unsafe struct RenderAPI
{
    private D3D12Context* _context;
    private D3D12ResourceManager1* _resourceManager;

    public CommandListHandle BeginCommandList(Handle<PipelineState> pipelineStateHandle)
    {
        ref var commandList = ref _context->GetCurrentCommandList();
        var index = Interlocked.Increment(ref commandList.Next) - 1;
        Debug.Assert(index < commandList.CommandLists.Size);

        var allocator = commandList.Allocators[index].Get();
        var list = commandList.CommandLists[index].Get();

        var pso = _resourceManager->GetPipelineState(pipelineStateHandle);
        list->Reset(allocator, pso);

        return default;
    }



    public void SetViewport(CommandListHandle handle, in Viewport viewport) { }

    public void SetRenderTargets(CommandListHandle handle, ReadOnlySpan<Handle<Texture>> textures)
    {
        ref var commandList = ref _context->GetCurrentCommandList();
        Span<D3D12_CPU_DESCRIPTOR_HANDLE> handles = stackalloc D3D12_CPU_DESCRIPTOR_HANDLE[textures.Length];
        //_resourceManager->GetTextureHandles(textures, handles);
        fixed (D3D12_CPU_DESCRIPTOR_HANDLE* handlesPtr = handles)
        {
            commandList.CommandLists[handle.Index].Get()->OMSetRenderTargets((uint)textures.Length, handlesPtr, 0, null);
        }
    }

    public void EndCommandList(CommandListHandle handle)
    {

    }

    public void SubmitCommandList(CommandListHandle handle)
    {

    }

}
