namespace Titan.Platform.Win32.D3D12;

public unsafe struct D3D12_MESSAGE
{
    public D3D12_MESSAGE_CATEGORY Category;
    public D3D12_MESSAGE_SEVERITY Severity;
    public D3D12_MESSAGE_ID ID;
    public char* pDescription;
    public nuint DescriptionByteLength;
}
