// Auto-Generated
#pragma warning disable

namespace Titan.Assets;
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
    public readonly struct Audios
    {
        public static ref readonly Titan.Assets.AssetDescriptor BackgroundMusic => ref _assets[0];
        public static ref readonly Titan.Assets.AssetDescriptor Click1 => ref _assets[4];
        public static ref readonly Titan.Assets.AssetDescriptor Click2 => ref _assets[5];
        public static ref readonly Titan.Assets.AssetDescriptor Click3 => ref _assets[6];
        public static ref readonly Titan.Assets.AssetDescriptor Click4 => ref _assets[7];
        public static ref readonly Titan.Assets.AssetDescriptor Click5 => ref _assets[8];
    }
    
    public readonly struct Meshes
    {
        public static ref readonly Titan.Assets.AssetDescriptor Book => ref _assets[1];
        public static ref readonly Titan.Assets.AssetDescriptor TheModel => ref _assets[22];
    }
    
    public readonly struct Textures
    {
        public static ref readonly Titan.Assets.AssetDescriptor BookTexture => ref _assets[2];
        public static ref readonly Titan.Assets.AssetDescriptor Box => ref _assets[3];
        public static ref readonly Titan.Assets.AssetDescriptor TitanLogo => ref _assets[23];
    }
    
    public readonly struct Fonts
    {
        public static ref readonly Titan.Assets.AssetDescriptor CutiveMonoRegular => ref _assets[9];
        public static ref readonly Titan.Assets.AssetDescriptor SyneMonoRegular => ref _assets[21];
    }
    
    public readonly struct Sprites
    {
        public static class RedSheet
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[10];
            
            public static class SpriteIndex
            {
                public const byte Button1 = 0;
                public const byte Button1Hover = 1;
                public const byte Button1Clicked = 2;
            }
        }
    }
    
    public readonly struct Shaders
    {
        public static ref readonly Titan.Assets.AssetDescriptor ShaderUIPixel => ref _assets[11];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderUIVertex => ref _assets[12];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderDeferredLightingPixel => ref _assets[13];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderDeferredLightingVertex => ref _assets[14];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderFullscreenPixel => ref _assets[15];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderFullscreenVertex => ref _assets[16];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderGBufferPixel => ref _assets[17];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderGBufferVertex => ref _assets[18];
        public static ref readonly Titan.Assets.AssetDescriptor SimplePixelShader => ref _assets[19];
        public static ref readonly Titan.Assets.AssetDescriptor SimpleVertexShader => ref _assets[20];
    }
    
    static EngineAssetsRegistry()
    {
        _assets[0] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 42033152,
                Offset = 0,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Audio\brave_space_explorersmix2.ogg")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[1] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 375640,
                Offset = 42033152,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.obj")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Mesh = new()
            {
                IndexCount = 10434,
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
                Length = 22369648,
                Offset = 42408792,
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
                BitsPerPixel = 8,
                DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM
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
                Offset = 64778440,
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
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 16552,
                Offset = 68972744,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click1.ogg")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[5] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 9832,
                Offset = 68989296,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click2.ogg")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[6] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 15196,
                Offset = 68999128,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click3.ogg")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[7] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 6444,
                Offset = 69014324,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click4.ogg")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[8] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 5648,
                Offset = 69020768,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click5.ogg")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[9] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Font,
            File = 
            { 
                Length = 128581,
                Offset = 69026416,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\Cutive_Mono\CutiveMono-Regular.ttf")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Font = new()
            {
                DefaultGlyphIndex = 24,
                NumberOfGlyphs = 83,
                BytesPerPixel = 1,
                Width = 2560,
                Height = 50
            }
        };
        _assets[10] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Sprite,
            File = 
            { 
                Length = 524312,
                Offset = 69154997,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"ui\redsheet.png")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Sprite = new()
            {
                NumberOfSprites = 3,
                Texture = new()
                {
                    Width = 512,
                    Height = 256,
                    Stride = 2048,
                    BitsPerPixel = 32,
                    DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM
                }
            }
        };
        _assets[11] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2896,
                Offset = 69679309,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.UI.Pixel.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[12] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3368,
                Offset = 69682205,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.UI.Vertex.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[13] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3548,
                Offset = 69685573,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Pixel.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[14] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1784,
                Offset = 69689121,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Vertex.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[15] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2848,
                Offset = 69690905,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Pixel.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[16] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1784,
                Offset = 69693753,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Vertex.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[17] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3396,
                Offset = 69695537,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Pixel.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[18] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3764,
                Offset = 69698933,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Vertex.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[19] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2796,
                Offset = 69702697,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_ps_01.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[20] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3084,
                Offset = 69705493,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_vs_01.hlsl")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[21] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Font,
            File = 
            { 
                Length = 90139,
                Offset = 69708577,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\SyneMono-Regular.ttf")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Font = new()
            {
                DefaultGlyphIndex = 24,
                NumberOfGlyphs = 77,
                BytesPerPixel = 1,
                Width = 1792,
                Height = 50
            }
        };
        _assets[22] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 37616,
                Offset = 69798716,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\tileLarge_forest.obj")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Mesh = new()
            {
                IndexCount = 1044,
                VertexCount = 1044,
                SubMeshCount = 2,
                MaterialCount = -1,
            }
        };
        _assets[23] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 113920,
                Offset = 69836332,
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
    [System.Runtime.CompilerServices.InlineArrayAttribute(24)]
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
