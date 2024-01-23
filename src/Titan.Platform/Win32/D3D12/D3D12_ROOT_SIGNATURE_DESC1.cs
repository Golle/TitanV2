namespace Titan.Platform.Win32.D3D12;

public unsafe struct D3D12_ROOT_SIGNATURE_DESC1
{
    public uint NumParameters;
    public D3D12_ROOT_PARAMETER1* pParameters;
    public uint NumStaticSamplers;
    public D3D12_STATIC_SAMPLER_DESC* pStaticSamplers;
    public D3D12_ROOT_SIGNATURE_FLAGS Flags;
}
