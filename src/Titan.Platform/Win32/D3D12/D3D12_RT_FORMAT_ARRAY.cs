using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Platform.Win32.DXGI;

namespace Titan.Platform.Win32.D3D12;

public struct D3D12_RT_FORMAT_ARRAY
{
    public RenderTargetArray RTFormats;
    public uint NumRenderTargets;
    
    public D3D12_RT_FORMAT_ARRAY(params DXGI_FORMAT[] formats)
        : this(formats.AsSpan()) { }
    public D3D12_RT_FORMAT_ARRAY(ReadOnlySpan<DXGI_FORMAT> formats)
    {
        Debug.Assert(formats.Length <= 8);
        formats.CopyTo(RTFormats);
        NumRenderTargets = (uint)formats.Length;
    }


    [InlineArray(8)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RenderTargetArray
    {
        private DXGI_FORMAT _;
    }
}
