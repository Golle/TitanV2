// Auto-Generated
#pragma warning disable

namespace Titan;
public readonly struct EngineAssetsRegistry : Titan.Assets.IAssetRegistry
{
    public static Titan.Assets.RegistryId Id { get; } = Titan.Assets.RegistryId.GetNext();
    private static __ASSETS__ _assets;
    private static __ASSETS_DEPS__ _dependencies;
    public static ReadOnlySpan<char> GetFilePath()
    {
        ReadOnlySpan<char> path = "titan.tbin";
        return path;
    }
    public static ReadOnlySpan<Titan.Assets.AssetDescriptor> GetAssetDescriptors()
        => _assets;
    public static ReadOnlySpan<System.UInt32> GetDependencies(in Titan.Assets.AssetDescriptor descriptor)
        => ((ReadOnlySpan<System.UInt32>)_dependencies).Slice((int)descriptor.Dependencies.Index, (int)descriptor.Dependencies.Count);
    public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset0 => ref _assets[0];
    public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset1 => ref _assets[1];
    public static ref readonly Titan.Assets.AssetDescriptor Book => ref _assets[2];
    public static ref readonly Titan.Assets.AssetDescriptor BookTexture => ref _assets[3];
    public static ref readonly Titan.Assets.AssetDescriptor Box => ref _assets[4];
    public static ref readonly Titan.Assets.AssetDescriptor FullScreenVertexShader => ref _assets[5];
    public static ref readonly Titan.Assets.AssetDescriptor RedSheet => ref _assets[6];
    public static ref readonly Titan.Assets.AssetDescriptor SimplePixelShader => ref _assets[7];
    public static ref readonly Titan.Assets.AssetDescriptor SimpleVertexShader => ref _assets[8];
    public static ref readonly Titan.Assets.AssetDescriptor TheModel => ref _assets[9];
    public static ref readonly Titan.Assets.AssetDescriptor TitanLogo => ref _assets[10];
    static EngineAssetsRegistry()
    {
        _assets[0] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1820,
                Offset = 0,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\fullscreen_ps.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Graphics.Resources.ShaderType.Pixel
            }
        };
        _assets[1] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 67108864,
                Offset = 1820,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture1.png")
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
        _assets[2] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 208688,
                Offset = 67110684,
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
        _assets[3] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 67108864,
                Offset = 67319372,
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
        _assets[4] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 4194304,
                Offset = 134428236,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"box.png")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Texture2D = new()
            {
                Width = 1024,
                Height = 1024,
                Stride = 4096,
                BitsPerPixel = 32,
                DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM
            }
        };
        _assets[5] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1656,
                Offset = 138622540,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\fullscreen_vs.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Graphics.Resources.ShaderType.Vertex
            }
        };
        _assets[6] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 524288,
                Offset = 138624196,
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
        _assets[7] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2796,
                Offset = 139148484,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_ps_01.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Graphics.Resources.ShaderType.Pixel
            }
        };
        _assets[8] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3084,
                Offset = 139151280,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_vs_01.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Graphics.Resources.ShaderType.Vertex
            }
        };
        _assets[9] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 20896,
                Offset = 139154364,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\tileLarge_forest.obj")
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
        _assets[10] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 113920,
                Offset = 139175260,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"titan.png")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Texture2D = new()
            {
                Width = 314,
                Height = 89,
                Stride = 1280,
                BitsPerPixel = 32,
                DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM
            }
        };
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(11)]
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
