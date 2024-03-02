using System.Diagnostics;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;

namespace Titan.Graphics.D3D12.Adapters;

[DebuggerDisplay("{DebugString,nq}")]
internal unsafe struct AdapterInfo
{
    public const int MaxNameLength = 128;
    public ComPtr<IDXGIAdapter3> Adapter;
    public uint DeviceId;
    public uint VendorId;
    public fixed char Description[MaxNameLength];
    public uint DescriptionLength;
    public bool IsHardware;

    public readonly string DebugString => $"{GetName()} Hardware = {IsHardware} DeviceId = {DeviceId} VendorId = {VendorId}";
    public readonly ReadOnlySpan<char> GetName()
    {
        fixed (char* pDescription = Description)
        {
            return new(pDescription, (int)DescriptionLength);
        }
    }

    public void SetName(ReadOnlySpan<char> name)
    {
        fixed (char* pDescription = Description)
        {
            // cap it at the max length (128)
            var slicedName = name.Length > MaxNameLength ? name[..MaxNameLength] : name;
            MemoryUtils.Copy(pDescription, slicedName);
        }
    }
}
