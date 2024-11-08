using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

public static unsafe class IID
{

    public static Guid* IID_ID3D12InfoQueue => AsPointer([0x0b, 0xa9, 0x42, 0x07, 0x87, 0xc3, 0x3f, 0x48, 0xb9, 0x46, 0x30, 0xa7, 0xe4, 0xe6, 0x14, 0x58]);
    public static Guid* IID_ID3D12InfoQueue1 => AsPointer([0x88, 0xdd, 0x52, 0x28, 0x84, 0xb4, 0x0c, 0x4c, 0xb6, 0xb1, 0x67, 0x16, 0x85, 0x00, 0xe6, 0x00]);
    public static Guid* IID_ID3D12QueryHeap => AsPointer([0xae, 0x58, 0x96, 0x0d, 0x45, 0xed, 0x9e, 0x46, 0xa6, 0x1d, 0x97, 0x0e, 0xc5, 0x83, 0xca, 0xb4]);
    public static Guid* IID_ID3D12CommandAllocator => AsPointer([0xe4, 0xde, 0x02, 0x61, 0x59, 0xaf, 0x09, 0x4b, 0xb9, 0x99, 0xb4, 0x4d, 0x73, 0xf0, 0x9b, 0x24]);
    public static Guid* IID_ID3D12CommandQueue => AsPointer([0xa6, 0x70, 0xc8, 0x0e, 0x7e, 0x5d, 0x22, 0x4c, 0x8c, 0xfc, 0x5b, 0xaa, 0xe0, 0x76, 0x16, 0xed]);
    public static Guid* IID_ID3D12Debug => AsPointer([0xb7, 0x88, 0x44, 0x34, 0x46, 0x68, 0x4b, 0x47, 0xb9, 0x89, 0xf0, 0x27, 0x44, 0x82, 0x45, 0xe0]);
    public static Guid* IID_ID3D12Debug1 => AsPointer([0xca, 0xa4, 0xfa, 0xaf, 0xfe, 0x63, 0x8e, 0x4d, 0xb8, 0xad, 0x15, 0x90, 0x00, 0xaf, 0x43, 0x04]);
    public static Guid* IID_ID3D12DescriptorHeap => AsPointer([0x1d, 0x47, 0xfb, 0x8e, 0x6c, 0x61, 0x49, 0x4f, 0x90, 0xf7, 0x12, 0x7b, 0xb7, 0x63, 0xfa, 0x51]);
    public static Guid* IID_ID3D12Device4 => AsPointer([0x17, 0xdf, 0x65, 0xe8, 0xee, 0xa9, 0xf9, 0x46, 0xa4, 0x63, 0x30, 0x98, 0x31, 0x5a, 0xa2, 0xe5]);
    public static Guid* IID_ID3D12Device7 => AsPointer([0x53, 0x4b, 0x01, 0x5c, 0xa1, 0x68, 0x9b, 0x4b, 0x8b, 0xd1, 0xdd, 0x60, 0x46, 0xb9, 0x35, 0x8b]);
    public static Guid* IID_ID3D12Fence => AsPointer([0xcf, 0x3d, 0x75, 0x0a, 0xd8, 0xc4, 0x91, 0x4b, 0xad, 0xf6, 0xbe, 0x5a, 0x60, 0xd9, 0x5a, 0x76]);
    public static Guid* IID_ID3D12GraphicsCommandList => AsPointer([0x0f, 0x0d, 0x16, 0x5b, 0x1b, 0xac, 0x85, 0x41, 0x8b, 0xa8, 0xb3, 0xae, 0x42, 0xa5, 0xa4, 0x55]);
    public static Guid* IID_ID3D12GraphicsCommandList4 => AsPointer([0x8e, 0x31, 0x54, 0x87, 0xa9, 0xd3, 0x41, 0x45, 0x98, 0xcf, 0x64, 0x5b, 0x50, 0xdc, 0x48, 0x74]);
    public static Guid* IID_ID3D12StateObject => AsPointer([0x43, 0x69, 0x01, 0x47, 0xa8, 0xfc, 0x94, 0x45, 0x93, 0xea, 0xaf, 0x25, 0x8b, 0x55, 0x34, 0x6d]);
    public static Guid* IID_ID3D12Heap => AsPointer([0x02, 0x25, 0x3b, 0x6b, 0x51, 0x6e, 0xb3, 0x45, 0x90, 0xee, 0x98, 0x84, 0x26, 0x5e, 0x8d, 0xf3]);
    public static Guid* IID_ID3D12PipelineState => AsPointer([0xf3, 0x30, 0x5a, 0x76, 0x24, 0xf6, 0x6f, 0x4c, 0xa8, 0x28, 0xac, 0xe9, 0x48, 0x62, 0x24, 0x45]);
    public static Guid* IID_ID3D12Resource => AsPointer([0xbe, 0x42, 0x64, 0x69, 0x2e, 0xa7, 0x59, 0x40, 0xbc, 0x79, 0x5b, 0x5c, 0x98, 0x04, 0x0f, 0xad]);
    public static Guid* IID_ID3D12RootSignature => AsPointer([0x66, 0x6b, 0x4a, 0xc5, 0xdf, 0x72, 0xe8, 0x4e, 0x8b, 0xe5, 0xa9, 0x46, 0xa1, 0x42, 0x92, 0x14]);
    public static Guid* IID_ID3DBlob => AsPointer([0x08, 0xfb, 0xa5, 0x8b, 0x95, 0x51, 0xe2, 0x40, 0xac, 0x58, 0x0d, 0x98, 0x9c, 0x3a, 0x01, 0x02]);
    public static Guid* IID_IDXCBlob => AsPointer([0x08, 0xfb, 0xa5, 0x8b, 0x95, 0x51, 0xe2, 0x40, 0xac, 0x58, 0x0d, 0x98, 0x9c, 0x3a, 0x01, 0x02]);
    public static Guid* IID_IDXCBlobUtf8 => AsPointer([0xc9, 0x36, 0xa6, 0x3d, 0x71, 0xba, 0x24, 0x40, 0xa3, 0x01, 0x30, 0xcb, 0xf1, 0x25, 0x30, 0x5b]);
    public static Guid* IID_IDXCBlobEncoding => AsPointer([0x24, 0xd4, 0x41, 0x72, 0x46, 0x26, 0x91, 0x41, 0x97, 0xc0, 0x98, 0xe9, 0x6e, 0x42, 0xfc, 0x68]);
    public static Guid* IID_IDXCBlobWide => AsPointer([0xab, 0x4e, 0xf8, 0xa3, 0xaa, 0x0f, 0x7e, 0x49, 0xa3, 0x9c, 0xee, 0x6e, 0xd6, 0x0b, 0x2d, 0x84]);
    public static Guid* IID_IDXCCompiler3 => AsPointer([0x87, 0x46, 0x8b, 0x22, 0x6a, 0x5a, 0x30, 0x47, 0x90, 0x0c, 0x97, 0x02, 0xb2, 0x20, 0x3f, 0x54]);
    public static Guid* IID_IDXCIncludeHandler => AsPointer([0x7d, 0xfc, 0x61, 0x7f, 0x0d, 0x95, 0x7f, 0x46, 0xb3, 0xe3, 0x3c, 0x02, 0xfb, 0x49, 0x18, 0x7c]);
    public static Guid* IID_IDXCResult => AsPointer([0xda, 0x6c, 0x34, 0x58, 0xe7, 0xdd, 0x97, 0x44, 0x94, 0x61, 0x6f, 0x87, 0xaf, 0x5e, 0x06, 0x59]);
    public static Guid* IID_IDXCUtils => AsPointer([0xcb, 0xc4, 0x05, 0x46, 0x19, 0x20, 0x2a, 0x49, 0xad, 0xa4, 0x65, 0xf2, 0x0b, 0xb7, 0xd6, 0x7f]);
    public static Guid* IID_IDXGIAdapter3 => AsPointer([0xa4, 0x67, 0x59, 0x64, 0x92, 0x13, 0x10, 0x43, 0xa7, 0x98, 0x80, 0x53, 0xce, 0x3e, 0x93, 0xfd]);
    public static Guid* IID_IDXGIFactory7 => AsPointer([0xed, 0x6e, 0x96, 0xa4, 0xdb, 0x76, 0xda, 0x44, 0x84, 0xc1, 0xee, 0x9a, 0x7a, 0xfb, 0x20, 0xa8]);
    public static Guid* IID_IDXGISwapChain3 => AsPointer([0xdb, 0x9b, 0xd9, 0x94, 0xf8, 0xf1, 0xb0, 0x4a, 0xb2, 0x36, 0x7d, 0xa0, 0x17, 0x0e, 0xda, 0xb1]);
    public static Guid* IID_IDXGIDebug => AsPointer([0x52, 0x74, 0x9e, 0x11, 0x9e, 0xde, 0xfe, 0x40, 0x88, 0x06, 0x88, 0xf9, 0x0c, 0x12, 0xb4, 0x41]);
    public static Guid* IID_IWICBitmapDecoder => AsPointer([0xe7, 0xe9, 0xdd, 0x9e, 0xee, 0x8d, 0xea, 0x47, 0x99, 0xdf, 0xe6, 0xfa, 0xf2, 0xed, 0x44, 0xbf]);
    public static Guid* IID_IWICBitmapFrameDecode => AsPointer([0x1b, 0x81, 0x16, 0x3b, 0x43, 0x6a, 0xc9, 0x4e, 0xa8, 0x13, 0x3d, 0x93, 0x0c, 0x13, 0xb9, 0x40]);
    public static Guid* IID_IWICComponentInfo => AsPointer([0x0a, 0x3f, 0xbc, 0x23, 0x8b, 0x69, 0x57, 0x43, 0x88, 0x6b, 0xf2, 0x4d, 0x50, 0x67, 0x13, 0x34]);
    public static Guid* IID_IWICImagingFactory => AsPointer([0xa9, 0xc8, 0x5e, 0xec, 0x95, 0xc3, 0x14, 0x43, 0x9c, 0x77, 0x54, 0xd7, 0xa9, 0x35, 0xff, 0x70]);
    public static Guid* IID_IWICPixelFormatInfo => AsPointer([0x01, 0xa6, 0xed, 0xe8, 0x48, 0x3d, 0x1a, 0x43, 0xab, 0x44, 0x69, 0x05, 0x9b, 0xe8, 0x8b, 0xbe]);
    public static Guid* IID_IXAudio2 => AsPointer([0xcf, 0xe3, 0x02, 0x2b, 0x0b, 0x2e, 0xc3, 0x4e, 0xbe, 0x45, 0x1b, 0x2a, 0x3f, 0xe7, 0x21, 0x0d]);
    public static Guid* DXGI_DEBUG_ALL => AsPointer([0x83, 0xE2, 0x8A, 0xE4, 0x80, 0xDA, 0x0B, 0x49, 0x87, 0xE6, 0x43, 0xE9, 0xA9, 0xCF, 0xDA, 0x08]);
    public static Guid* IID_IMMDeviceEnumerator => AsPointer([0xd2, 0x64, 0x56, 0xa9, 0x14, 0x96, 0x35, 0x4f, 0xa7, 0x46, 0xde, 0x8d, 0xb6, 0x36, 0x17, 0xe6]);
    public static Guid* IID_IMMDeviceCollection => AsPointer([0xd2, 0x64, 0x56, 0xa9, 0x14, 0x96, 0x35, 0x4f, 0xa7, 0x46, 0xde, 0x8d, 0xb6, 0x36, 0x17, 0xe6]);
    public static Guid* IID_IMMDevice => AsPointer([0x3f, 0x06, 0x66, 0xd6, 0x87, 0x15, 0x43, 0x4e, 0x81, 0xf1, 0xb9, 0x48, 0xe8, 0x07, 0x36, 0x3f]);
    public static Guid* IID_IPropertyStore => AsPointer([0xbc, 0x8a, 0x0b, 0x88, 0xcf, 0x6a, 0x48, 0x0c, 0x8c, 0x3a, 0xc8, 0xf3, 0x0c, 0x8a, 0xf0, 0x81]);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid* AsPointer(in ReadOnlySpan<byte> data)
    {
        Debug.Assert(data.Length == sizeof(Guid));
        return (Guid*)Unsafe.AsPointer(ref Unsafe.As<byte, Guid>(ref MemoryMarshal.GetReference(data)));
    }
}
