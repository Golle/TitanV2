using System;
using System.Diagnostics;
using System.Reflection;
using Titan.Core;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using static Titan.Platform.Win32.Kernel32;

namespace Titan.RenderingV3;

internal struct D3D12CommandLists
{
    // max 16 allocators and command lists per frame. 
    public Inline16<ComPtr<ID3D12CommandAllocator>> Allocators;
    public Inline16<ComPtr<ID3D12GraphicsCommandList4>> CommandLists;
    public int Next;
}


internal enum CopyCommandListState : byte
{
    Available,
    Busy
}
internal struct D3D12CopyCommandLists
{
    public ComPtr<ID3D12Fence> Fence;
    public ulong FenceValue;
    public HANDLE EventHandle;
    private SpinLock CommandQueueLock;

    public Inline4<ComPtr<ID3D12CommandAllocator>> Allocator;
    public Inline4<ComPtr<ID3D12GraphicsCommandList4>> CommandList;
    public Inline4<CopyCommandListState> State;
    private SpinLock Lock;

    public bool TryGetAvailableCommandList(out int index)
    {
        var lockTaken = false;
        Lock.Enter(ref lockTaken);
        Debug.Assert(lockTaken);
        index = -1;
        for (var i = 0; i < State.Size; ++i)
        {
            if (State[i] == CopyCommandListState.Available)
            {
                State[i] = CopyCommandListState.Busy;
                index = i;
                break;
            }
        }

        Lock.Exit();
        return index != -1;
    }

    public unsafe void ExecuteCommandList(ID3D12CommandQueue* queue, int commandListIndex)
    {
        Debug.Assert(State[commandListIndex] == CopyCommandListState.Busy);
        var gotLock = false;
        CommandQueueLock.Enter(ref gotLock);
        var value = ++FenceValue;

        queue->ExecuteCommandLists(1, (ID3D12CommandList**)CommandList[commandListIndex].Get());
        queue->Signal(Fence, value);
        if (Fence.Get()->GetCompletedValue() < value)
        {
            Fence.Get()->SetEventOnCompletion(FenceValue, EventHandle);
            WaitForSingleObject(EventHandle, Win32Common.INFINITE);
        }
        CommandQueueLock.Exit();
        State[commandListIndex] = CopyCommandListState.Available;
    }

}
