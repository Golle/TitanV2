using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Titan.Core.Strings;
using Titan.Platform.Win32.DXGI;

namespace Titan.Tools.AssetProcessor;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct Texture2DDescriptor
{
    public uint Width;
    public uint Height;
    public ushort Stride;
    public ushort BitsPerPixel;
    public DXGI_FORMAT DXGIFormat;
}

/// <summary>
/// All types that we currently support. Maybe this should be extended with user defined types.
/// </summary>
public enum AssetType
{
    Texture2D = 1,
    Model3D = 2,
    ComputeShader = 3,
    VertexShader = 4,
    PixelShader = 5,
    Font = 6,

    //NOTE(Jens): A custom type will not have any descriptors with it, everything will be inside the file and have to be read my the loader.
    CustomType = 100 // Register loaders/types with a ID greater than 100 for custom
}

[StructLayout(LayoutKind.Sequential)]
public struct ShaderDescriptor;

[StructLayout(LayoutKind.Sequential)]
public struct Model3DDescriptor;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct AssetDescriptor
{
    //public AssetId Id;
    //public RegistryId RegistryId;
    public AssetType Type;
    public FileDescriptor File;
    //public AssetDependencies Dependencies;
    private AssetDescriptorUnion _descriptors;
    [UnscopedRef]
    public ref Texture2DDescriptor Texture2D => ref _descriptors.Texture2D;
    [UnscopedRef]
    public ref Model3DDescriptor Model3D => ref _descriptors.Model3D;
    [UnscopedRef]
    public ref ShaderDescriptor Shader => ref _descriptors.Shader;

//#if !RELEASE
//    public AssetDebugInformation DebugInformation;
//#endif


    /// <summary>
    /// All supported built in Asset descriptors
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    private struct AssetDescriptorUnion
    {
        [FieldOffset(0)]
        public Texture2DDescriptor Texture2D;
        [FieldOffset(0)]
        public Model3DDescriptor Model3D;
        [FieldOffset(0)]
        public ShaderDescriptor Shader;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FileDescriptor
{
    public uint Offset;
    public uint Length;

    //#if !RELEASE
    public StringRef AssetPath;
    //#endif
}
