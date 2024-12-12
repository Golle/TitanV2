namespace Titan.Platform.Win32.D3D12;

public unsafe struct D3D12_INFO_QUEUE_FILTER_DESC
{
    public uint NumCategories;
    public D3D12_MESSAGE_CATEGORY* pCategoryList;
    public uint NumSeverities;
    public D3D12_MESSAGE_SEVERITY* pSeverityList;
    public uint NumIDs;
    public D3D12_MESSAGE_ID* pIDList;
}
