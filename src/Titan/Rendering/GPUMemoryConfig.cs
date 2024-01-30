using Titan.Rendering.D3D12.Memory;

namespace Titan.Rendering;

public unsafe struct GPUMemoryConfig
{
    private fixed uint _counts[(int)DescriptorHeapType.Count];
    public readonly uint TempBufferSize;
    public readonly uint TempShaderResourceViewCount;

    public GPUMemoryConfig(uint srvCount, uint rtvCount, uint dsvCount, uint uavCount, uint tempBufferSize, uint tempSrvCount)
    {
        _counts[(int)DescriptorHeapType.ShaderResourceView] = srvCount;
        _counts[(int)DescriptorHeapType.RenderTargetView] = rtvCount;
        _counts[(int)DescriptorHeapType.DepthStencilView] = dsvCount;
        _counts[(int)DescriptorHeapType.UnorderedAccessView] = uavCount;
        TempBufferSize = tempBufferSize;
        TempShaderResourceViewCount = tempSrvCount;
    }

    public readonly uint GetDescriptorCount(DescriptorHeapType type)
        => _counts[(int)type];
}
