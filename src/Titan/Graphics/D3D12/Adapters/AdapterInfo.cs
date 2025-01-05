using System.Diagnostics;
using Titan.Core;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;

namespace Titan.Graphics.D3D12.Adapters;

public struct AdapterMode
{
    public uint Width;
    public uint Height;
    //TODO: Add refresh rate if we ever need it
    public Inline32<char> Description;
    public int DescriptionLength;

    public readonly ReadOnlySpan<char> GetDescription()
        => Description.AsReadOnlySpan()[..DescriptionLength];
}

public struct AdapterOutput
{
    public Inline64<AdapterMode> Modes;
    public uint ModeCount;
    public readonly ReadOnlySpan<AdapterMode> GetModes()
        => Modes.AsReadOnlySpan()[..(int)ModeCount];

}
[DebuggerDisplay("{DebugString,nq}")]
public struct AdapterInfo
{
    internal ComPtr<IDXGIAdapter3> Adapter;
    public uint DeviceId;
    public uint VendorId;
    public Inline256<char> Description;
    public uint DescriptionLength;
    public bool IsHardware;
    public Inline8<AdapterOutput> Outputs;
    public uint OutputCount;

    public readonly ReadOnlySpan<AdapterOutput> GetOutputs() => Outputs.AsReadOnlySpan()[..(int)OutputCount];
    public readonly string DebugString => $"{GetName()} Hardware = {IsHardware} DeviceId = {DeviceId} VendorId = {VendorId}";
    public readonly ReadOnlySpan<char> GetName()
        => Description.AsReadOnlySpan()[..(int)DescriptionLength];

    public void SetName(ReadOnlySpan<char> name)
    {
        // cap it at the max length
        var slicedName = name.Length > Description.Size ? name[..Description.Size] : name;
        MemoryUtils.Copy(Description, slicedName);
    }
}
