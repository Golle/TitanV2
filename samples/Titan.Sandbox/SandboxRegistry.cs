// Auto-Generated
#pragma warning disable

namespace Titan.Sandbox;
public readonly struct SandboxRegistry : Titan.Assets.IAssetRegistry
{
    public static Titan.Assets.RegistryId Id { get; } = Titan.Assets.RegistryId.GetNext();
    private static __ASSETS__ _assets;
    private static __ASSETS_DEPS__ _dependencies;
    public static ReadOnlySpan<char> GetFilePath()
    {
        ReadOnlySpan<char> path = "sandbox.tbin";
        return path;
    }
    public static ReadOnlySpan<Titan.Assets.AssetDescriptor> GetAssetDescriptors()
        => _assets;
    public static ReadOnlySpan<System.UInt32> GetDependencies(in Titan.Assets.AssetDescriptor descriptor)
        => ((ReadOnlySpan<System.UInt32>)_dependencies).Slice((int)descriptor.Dependencies.Index, (int)descriptor.Dependencies.Count);
    public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset0 => ref _assets[0];
    public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset1 => ref _assets[1];
    public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset2 => ref _assets[2];
    public static ref readonly Titan.Assets.AssetDescriptor TileLowRed => ref _assets[3];
    static SandboxRegistry()
    {
        _assets[0] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 524288,
                Offset = 0,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"redsheet.png")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Texture2D = new()
            {
                Width = 512,
                Height = 256,
                Stride = 2048,
                BitsPerPixel = 32,
                DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM
            }
        };
        _assets[1] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 208688,
                Offset = 524288,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.obj")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Mesh = new()
            {
                IndexCount = -1,
                VertexCount = 10434,
                SubMeshCount = 1,
                MaterialCount = -1,
            }
        };
        _assets[2] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 67108864,
                Offset = 732976,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture.png")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Texture2D = new()
            {
                Width = 4096,
                Height = 4096,
                Stride = 16384,
                BitsPerPixel = 32,
                DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM
            }
        };
        _assets[3] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 20896,
                Offset = 67841840,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLow_teamRed.obj")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Mesh = new()
            {
                IndexCount = -1,
                VertexCount = 1044,
                SubMeshCount = 2,
                MaterialCount = -1,
            }
        };
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(4)]
    private struct __ASSETS__
    {
        private Titan.Assets.AssetDescriptor _ref;
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(1)]
    private struct __ASSETS_DEPS__
    {
        private System.UInt32 _ref;
    }
}
