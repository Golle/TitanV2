using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Maths;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct CommandList(ID3D12GraphicsCommandList4* commandList)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetRenderTarget(Texture* texture)
    {
        Debug.Assert(texture != null);
        var d3d12Texture = (D3D12Texture*)texture;
        commandList->OMSetRenderTargets(1, &d3d12Texture->RTV.CPU, 1, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public void SetRenderTargets(Texture** textures, uint count)
    {
        //TODO(Jens): This might be slow, maybe we can cache this on the caller? Needs to measure the overhead of having a nicer API.
        var handles = stackalloc D3D12_CPU_DESCRIPTOR_HANDLE[(int)count];
        for (var i = 0; i < count; ++i)
        {
            handles[i] = ((D3D12Texture*)textures[i])->RTV.CPU;
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
        Debug.Assert(renderTargetHandles != null && count > 0);
        commandList->OMSetRenderTargets(count, renderTargetHandles, 1, depthBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetRenderTarget(Texture* texture, Texture* depthBuffer)
    {
        Debug.Assert(depthBuffer != null);
        Debug.Assert(texture != null);
        var d3d12Texture = (D3D12Texture*)texture;
        var d3d12DepthBuffer = (D3D12Texture*)depthBuffer;

        commandList->OMSetRenderTargets(1, &d3d12Texture->RTV.CPU, 1, &d3d12DepthBuffer->DSV.CPU);
    }

    public void ClearRenderTargetView(Texture* texture, Color* color)
    {
        Debug.Assert(commandList != null);
        var d3d12Texture = (D3D12Texture*)texture;
        commandList->ClearRenderTargetView(d3d12Texture->RTV.CPU, (float*)color, 0, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Transition(Texture* texture, D3D12_RESOURCE_STATES before, D3D12_RESOURCE_STATES after)
    {
        var d3d12Texture = (D3D12Texture*)texture;
        //NOTE(Jens): remove this method and use the one with Handle<Texture> when we have proper render target implementation
        Unsafe.SkipInit(out D3D12_RESOURCE_BARRIER barrier);

        barrier.Flags = D3D12_RESOURCE_BARRIER_FLAGS.D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barrier.Type = D3D12_RESOURCE_BARRIER_TYPE.D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
        barrier.Transition.StateAfter = after;
        barrier.Transition.StateBefore = before;
        barrier.Transition.Subresource = 0;
        barrier.Transition.pResource = d3d12Texture->Resource;
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
        Debug.Assert(Win32Common.SUCCEEDED(hr), "Failed to Close the command list.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetViewport(D3D12_VIEWPORT* viewport)
        => commandList->RSSetViewports(1, viewport);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetScissorRect(D3D12_RECT* rect)
        => commandList->RSSetScissorRects(1, rect);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawIndexedInstanced(uint indexCountPerInstance, uint instanceCount, uint startIndexLocation = 0, int baseVertexLocation = 0, uint startInstanceLocation = 0)
        => commandList->DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawInstanced(uint vertexCountPerInstance, uint instanceCount, uint startIndexLocation = 0, uint startInstanceLocation = 0)
        => commandList->DrawInstanced(vertexCountPerInstance, instanceCount, startIndexLocation, startInstanceLocation);

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
    public void SetGraphicsRootDescriptorTable(uint index, D3D12_GPU_DESCRIPTOR_HANDLE baseDescriptor)
        => commandList->SetGraphicsRootDescriptorTable(index, baseDescriptor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootConstantBufferView(uint rootParameterIndex, D3D12_GPU_VIRTUAL_ADDRESS bufferLocation)
        => commandList->SetGraphicsRootConstantBufferView(rootParameterIndex, bufferLocation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGraphicsRootShaderResourceView(uint rootParameterIndex, D3D12_GPU_VIRTUAL_ADDRESS bufferLocation)
        => commandList->SetGraphicsRootShaderResourceView(rootParameterIndex, bufferLocation);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IASetIndexBuffer(D3D12_INDEX_BUFFER_VIEW indexBufferView)
        => commandList->IASetIndexBuffer(&indexBufferView);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDepthStencilView(D3D12_CPU_DESCRIPTOR_HANDLE depthStencilView, D3D12_CLEAR_FLAGS flags, float depth, byte stencil, uint numberOfRects, D3D12_RECT* rects)
        => commandList->ClearDepthStencilView(depthStencilView, flags, depth, stencil, numberOfRects, rects);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDepthStencilView(Texture* depthBuffer, D3D12_CLEAR_FLAGS flags, float depth, byte stencil, uint numberOfRects, D3D12_RECT* rects)
    {
        Debug.Assert(depthBuffer != null);
        var d3d12DepthBuffer = (D3D12Texture*)depthBuffer;

        commandList->ClearDepthStencilView(d3d12DepthBuffer->DSV.CPU, flags, depth, stencil, numberOfRects, rects);
    }
}
