namespace Titan.Platform.Win32.DXGI;

public unsafe struct DXGI_INFO_QUEUE_MESSAGE
{
    public Guid Producer;
    public DXGI_INFO_QUEUE_MESSAGE_CATEGORY Category;
    public DXGI_INFO_QUEUE_MESSAGE_SEVERITY Severity;
    public int ID;
    public byte* pDescription;
    public nuint DescriptionByteLength;
}
