namespace Titan.Graphics;

public record struct ResourceConfig(uint MaxTextures, uint MaxMaterials, uint MaxBuffers, uint MaxPipelineStates, uint MaxRootSignatures, uint MaxShaders);

public unsafe struct GPUMemoryConfig
{
    private fixed uint _counts[(int)DescriptorHeapType.Count];
    public readonly uint TempBufferSize;
    public readonly uint TempShaderResourceViewCount;
    /// <summary>
    ///  The TotalCount is the number of Descriptors forSRV, RTV, DSV and UAV. Not the temporary ones.This is used when allocating a single block
    /// </summary>
    public readonly uint TotalCount;

    public GPUMemoryConfig(uint srvCount, uint rtvCount, uint dsvCount, uint uavCount, uint tempBufferSize, uint tempSrvCount)
    {
        _counts[(int)DescriptorHeapType.ShaderResourceView] = srvCount;
        _counts[(int)DescriptorHeapType.RenderTargetView] = rtvCount;
        _counts[(int)DescriptorHeapType.DepthStencilView] = dsvCount;
        _counts[(int)DescriptorHeapType.UnorderedAccessView] = uavCount;
        TempBufferSize = tempBufferSize;
        TempShaderResourceViewCount = tempSrvCount;
        TotalCount = srvCount + rtvCount + dsvCount + uavCount;
    }
    
    public readonly uint GetDescriptorCount(DescriptorHeapType type)
        => GetDescriptorCount((int)type);

    public readonly uint GetDescriptorCount(int index)
        => _counts[index];
}
