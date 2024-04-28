using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12.Upload;

internal struct UploadFrame
{
    public ComPtr<ID3D12CommandAllocator> Allocator;
    public ComPtr<ID3D12GraphicsCommandList4> CommandList;
    public ulong FenceValue;
    public UploadState State;

    public unsafe void WaitAndReset(HANDLE fenceEvent, ID3D12Fence* fence)
    {
        if (fence->GetCompletedValue() < FenceValue)
        {
            fence->SetEventOnCompletion(FenceValue, fenceEvent);
            Kernel32.WaitForSingleObject(fenceEvent, Win32Common.INFINITE);
        }
        State = UploadState.Available;
    }
}