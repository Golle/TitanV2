using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;

namespace Titan.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct CommandList(ID3D12GraphicsCommandList4* commandList)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetRenderTarget(Texture* texture)
    {
        Debug.Assert(texture != null);
        commandList->OMSetRenderTargets(1, &texture->RTV.CPU, 1, null);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetPipelineState(PipelineState* pipelineState)
    {
        Debug.Assert(pipelineState != null);
        commandList->SetPipelineState(pipelineState->Resource);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public void SetRenderTargets(Texture** textures, uint count)
    {
        //TODO(Jens): This might be slow, maybe we can cache this on the caller? Needs to measure the overhead of having a nicer API.
        var handles = stackalloc D3D12_CPU_DESCRIPTOR_HANDLE[(int)count];
        for (var i = 0; i < count; ++i)
        {
            handles[i] = textures[i]->RTV.CPU;
        }

        commandList->OMSetRenderTargets(count, handles, 0, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetRenderTargets(D3D12_CPU_DESCRIPTOR_HANDLE* renderTargetHandles, uint count)
    {
        Debug.Assert(renderTargetHandles != null && count > 0);
        commandList->OMSetRenderTargets(count, renderTargetHandles, 0, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetRenderTargets(D3D12_CPU_DESCRIPTOR_HANDLE* renderTargetHandles, uint count, D3D12_CPU_DESCRIPTOR_HANDLE* depthBuffer)
    {
        commandList->OMSetRenderTargets(count, renderTargetHandles, 1, depthBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetRenderTarget(Texture* texture, Texture* depthBuffer)
    {
        Debug.Assert(depthBuffer != null);
        Debug.Assert(texture != null);

        commandList->OMSetRenderTargets(1, &texture->RTV.CPU, 1, &depthBuffer->DSV.CPU);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearRenderTargetView(Texture* texture, in Color color)
    {
        Debug.Assert(commandList != null);
        commandList->ClearRenderTargetView(texture->RTV.CPU, (float*)MemoryUtils.AsPointer(color), 0, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearRenderTargetView(in Texture texture, in Color color)
    {
        Debug.Assert(commandList != null);
        commandList->ClearRenderTargetView(texture.RTV.CPU, (float*)MemoryUtils.AsPointer(color), 0, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearRenderTargetView(Texture* texture, Color* color)
    {
        Debug.Assert(commandList != null);
        commandList->ClearRenderTargetView(texture->RTV.CPU, (float*)color, 0, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Transition(Texture* texture, D3D12_RESOURCE_STATES before, D3D12_RESOURCE_STATES after)
    {
        Unsafe.SkipInit(out D3D12_RESOURCE_BARRIER barrier);

        barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS.D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barrier.Type = D3D12_RESOURCE_BARRIER_TYPE.D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
        barrier.Transition.StateAfter = after;
        barrier.Transition.StateBefore = before;
        barrier.Transition.Subresource = 0;
        barrier.Transition.pResource = texture->Resource;
        ResourceBarriers(&barrier, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResourceBarriers(ReadOnlySpan<D3D12_RESOURCE_BARRIER> barriers)
    {
        fixed (D3D12_RESOURCE_BARRIER* ptr = barriers)
        {
            ResourceBarriers(ptr, (uint)barriers.Length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResourceBarriers(D3D12_RESOURCE_BARRIER* barriers, uint count)
        => commandList->ResourceBarrier(count, barriers);

    public void Close()
    {
        Debug.Assert(commandList != null);
        var hr = commandList->Close();
        if (Win32Common.FAILED(hr))
        {
            Debugger.Launch();
        }
        //Debug.Assert(Win32Common.SUCCEEDED(hr), "Failed to Close the command list.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetViewport(D3D12_VIEWPORT* viewport)
        => commandList->RSSetViewports(1, viewport);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetScissorRect(Rect* rects, uint count = 1)
        => commandList->RSSetScissorRects(count, (D3D12_RECT*)rects);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetViewport(Viewport* viewports, uint count = 1)
        => commandList->RSSetViewports(count, (D3D12_VIEWPORT*)viewports);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetScissorRect(D3D12_RECT* rect)
        => commandList->RSSetScissorRects(1, rect);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawIndexedInstanced(uint indexCountPerInstance, uint instanceCount, uint startIndexLocation = 0, int baseVertexLocation = 0, uint startInstanceLocation = 0)
        => commandList->DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawInstanced(uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation = 0, uint startInstanceLocation = 0)
        => commandList->DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExecuteIndirect(CommandSignature* commandSignature, uint maxCommandCount, GPUBuffer* argumentsBuffer, ulong argumentOffset = 0)
        => commandList->ExecuteIndirect(commandSignature->Resource, maxCommandCount, argumentsBuffer->Resource.Get(), argumentOffset, null, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetIndexBuffer(GPUBuffer* buffer)
        => SetIndexBuffer(*buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetIndexBuffer(in GPUBuffer buffer)
    {
        Debug.Assert(buffer.Type is BufferType.Index);
        var view = new D3D12_INDEX_BUFFER_VIEW
        {
            Format = buffer.Stride == 2 ? DXGI_FORMAT.DXGI_FORMAT_R16_UINT : DXGI_FORMAT.DXGI_FORMAT_R32_UINT,
            BufferLocation = buffer.Resource.Get()->GetGPUVirtualAddress(),
            SizeInBytes = buffer.Size
        };
        commandList->IASetIndexBuffer(&view);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetIndexBuffer(D3D12_INDEX_BUFFER_VIEW view)
        => commandList->IASetIndexBuffer(&view);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootSignature(ID3D12RootSignature* rootSignature)
        => commandList->SetGraphicsRootSignature(rootSignature);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTopology(D3D_PRIMITIVE_TOPOLOGY type)
        => commandList->IASetPrimitiveTopology(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDescriptorHeap(ID3D12DescriptorHeap* heap)
        => SetDescriptorHeaps(1, &heap);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDescriptorHeaps(uint count, ID3D12DescriptorHeap** heaps)
        => commandList->SetDescriptorHeaps(count, heaps);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootDescriptorTable(uint rootParameterIndex, GPUBuffer* buffer)
        => SetGraphicsRootDescriptorTable(rootParameterIndex, *buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootDescriptorTable(uint rootParameterIndex, in GPUBuffer buffer)
    {
        Debug.Assert(buffer.SRV.IsValid);
        Debug.Assert(buffer.SRV.IsShaderVisible);
        commandList->SetGraphicsRootDescriptorTable(rootParameterIndex, buffer.SRV.GPU);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetComputeRootDescriptorTable(uint rootParameterIndex, D3D12_GPU_DESCRIPTOR_HANDLE baseDescriptor)
        => commandList->SetComputeRootDescriptorTable(rootParameterIndex, baseDescriptor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootDescriptorTable(uint rootParameterIndex, D3D12_GPU_DESCRIPTOR_HANDLE baseDescriptor)
        => commandList->SetGraphicsRootDescriptorTable(rootParameterIndex, baseDescriptor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootConstantBuffer(uint rootParameterIndex, GPUBuffer* buffer)
    {
        Debug.Assert(buffer != null);
        Debug.Assert(buffer->SRV.IsValid);
        commandList->SetGraphicsRootConstantBufferView(rootParameterIndex, buffer->Resource.Get()->GetGPUVirtualAddress());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootConstantBufferView(uint rootParameterIndex, D3D12_GPU_VIRTUAL_ADDRESS bufferLocation)
        => commandList->SetGraphicsRootConstantBufferView(rootParameterIndex, bufferLocation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetComputeRootConstants(uint index, ReadOnlySpan<int> data)
    {
        fixed (int* ptr = data)
        {
            commandList->SetComputeRoot32BitConstants(index, (uint)data.Length, ptr, 0);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootConstants(uint index, ReadOnlySpan<int> data)
    {
        fixed (int* ptr = data)
        {
            commandList->SetGraphicsRoot32BitConstants(index, (uint)data.Length, ptr, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootConstant<T>(uint index, in T value) where T : unmanaged
    {
        Debug.Assert(sizeof(T) % 4 == 0);
        commandList->SetGraphicsRoot32BitConstants(index, (uint)(sizeof(T) / 4u), MemoryUtils.AsPointer(value), 0);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootShaderResourceView(uint rootParameterIndex, D3D12_GPU_VIRTUAL_ADDRESS bufferLocation)
        => commandList->SetGraphicsRootShaderResourceView(rootParameterIndex, bufferLocation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDepthStencilView(D3D12_CPU_DESCRIPTOR_HANDLE depthStencilView, D3D12_CLEAR_FLAGS flags, float depth, byte stencil, uint numberOfRects, D3D12_RECT* rects)
        => commandList->ClearDepthStencilView(depthStencilView, flags, depth, stencil, numberOfRects, rects);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStencilRef(uint stencilRef) 
        => commandList->OMSetStencilRef(stencilRef);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDepthStencilView(Texture* depthBuffer, D3D12_CLEAR_FLAGS flags, float depth, byte stencil, uint numberOfRects, D3D12_RECT* rects)
    {
        Debug.Assert(depthBuffer != null);
        commandList->ClearDepthStencilView(depthBuffer->DSV.CPU, flags, depth, stencil, numberOfRects, rects);
    }
}
