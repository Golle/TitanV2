// Auto-Generated
#pragma warning disable

namespace Titan;
public readonly struct EngineAssetsRegistry : Titan.Assets.IAssetRegistry
{
    public static Titan.Assets.RegistryId Id { get; } = Titan.Assets.RegistryId.GetNext();
    private static readonly __ASSETS__ _assets;
    private static readonly __ASSETS_DEPS__ _dependencies;
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
    public static ref readonly Titan.Assets.AssetDescriptor Book => ref _assets[1];
    public static ref readonly Titan.Assets.AssetDescriptor BookTexture => ref _assets[2];
    public static ref readonly Titan.Assets.AssetDescriptor Box => ref _assets[3];
    public static ref readonly Titan.Assets.AssetDescriptor DebugTextPixelShader => ref _assets[4];
    public static ref readonly Titan.Assets.AssetDescriptor DebugTextVertexShader => ref _assets[5];
    public static ref readonly Titan.Assets.AssetDescriptor RedSheet => ref _assets[6];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderDeferredLighting => ref _assets[7];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderDeferredLightingPixel => ref _assets[8];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderDeferredLightingVertex => ref _assets[9];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderFullscreen => ref _assets[10];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderFullscreenPixel => ref _assets[11];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderFullscreenVertex => ref _assets[12];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderGBuffer => ref _assets[13];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderGBufferPixel => ref _assets[14];
    public static ref readonly Titan.Assets.AssetDescriptor ShaderGBufferVertex => ref _assets[15];
    public static ref readonly Titan.Assets.AssetDescriptor SimplePixelShader => ref _assets[16];
    public static ref readonly Titan.Assets.AssetDescriptor SimpleVertexShader => ref _assets[17];
    public static ref readonly Titan.Assets.AssetDescriptor TheModel => ref _assets[18];
    public static ref readonly Titan.Assets.AssetDescriptor TitanLogo => ref _assets[19];
    static EngineAssetsRegistry()
    {
        _assets[0] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 67108864,
                Offset = 0,
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
        _assets[1] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 208688,
                Offset = 67108864,
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
                Offset = 67317552,
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
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 4194304,
                Offset = 134426416,
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
        _assets[4] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1756,
                Offset = 138620720,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\debug_text_ps.hlsl")
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
        _assets[5] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1816,
                Offset = 138622476,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\debug_text_vs.hlsl")
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
                Offset = 138624292,
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
            Type = Titan.Assets.AssetType.ShaderInfo,
            File = 
            { 
                Length = 2,
                Offset = 139148580,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.shaderconf")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 2),
            ShaderInfo = new()
            {
                NumberOfDescriptorRanges = 0,
                NumberOfConstantBuffers = 1,
                NumberOfSamplers = 1
            }
        };
        _assets[8] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1528,
                Offset = 139148582,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Pixel.hlsl")
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
        _assets[9] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1416,
                Offset = 139150110,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Vertex.hlsl")
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
        _assets[10] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.ShaderInfo,
            File = 
            { 
                Length = 2,
                Offset = 139151526,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Backbuffer.shaderconf")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(2, 2),
            ShaderInfo = new()
            {
                NumberOfDescriptorRanges = 0,
                NumberOfConstantBuffers = 0,
                NumberOfSamplers = 1
            }
        };
        _assets[11] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1648,
                Offset = 139151528,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Pixel.hlsl")
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
        _assets[12] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1784,
                Offset = 139153176,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Vertex.hlsl")
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
        _assets[13] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.ShaderInfo,
            File = 
            { 
                Length = 6,
                Offset = 139154960,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.shaderconf")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(4, 2),
            ShaderInfo = new()
            {
                NumberOfDescriptorRanges = 1,
                NumberOfConstantBuffers = 1,
                NumberOfSamplers = 2
            }
        };
        _assets[14] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2064,
                Offset = 139154966,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Pixel.hlsl")
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
        _assets[15] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1892,
                Offset = 139157030,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Vertex.hlsl")
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
        _assets[16] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2796,
                Offset = 139158922,
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
        _assets[17] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3084,
                Offset = 139161718,
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
        _assets[18] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 20896,
                Offset = 139164802,
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
        _assets[19] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 113920,
                Offset = 139185698,
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
        _dependencies[0] = 8;
        _dependencies[1] = 9;
        _dependencies[2] = 11;
        _dependencies[3] = 12;
        _dependencies[4] = 14;
        _dependencies[5] = 15;
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(20)]
    private struct __ASSETS__
    {
        private Titan.Assets.AssetDescriptor _ref;
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(6)]
    private struct __ASSETS_DEPS__
    {
        private System.UInt32 _ref;
    }
}
