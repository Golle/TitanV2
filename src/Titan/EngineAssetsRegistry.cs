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
    public readonly struct Textures
    {
        public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset0 => ref _assets[0];
        public static ref readonly Titan.Assets.AssetDescriptor BookTexture => ref _assets[3];
        public static ref readonly Titan.Assets.AssetDescriptor Box => ref _assets[4];
        public static ref readonly Titan.Assets.AssetDescriptor TitanLogo => ref _assets[29];
    }
    
    public readonly struct Audios
    {
        public static ref readonly Titan.Assets.AssetDescriptor BackgroundMusic => ref _assets[1];
        public static ref readonly Titan.Assets.AssetDescriptor Click1 => ref _assets[5];
        public static ref readonly Titan.Assets.AssetDescriptor Click2 => ref _assets[6];
        public static ref readonly Titan.Assets.AssetDescriptor Click3 => ref _assets[7];
        public static ref readonly Titan.Assets.AssetDescriptor Click4 => ref _assets[8];
        public static ref readonly Titan.Assets.AssetDescriptor Click5 => ref _assets[9];
    }
    
    public readonly struct Meshes
    {
        public static ref readonly Titan.Assets.AssetDescriptor Book => ref _assets[2];
        public static ref readonly Titan.Assets.AssetDescriptor TheModel => ref _assets[27];
    }
    
    public readonly struct Fonts
    {
        public static ref readonly Titan.Assets.AssetDescriptor CutiveMonoRegular => ref _assets[10];
        public static ref readonly Titan.Assets.AssetDescriptor RobotoMonoRegular => ref _assets[13];
        public static ref readonly Titan.Assets.AssetDescriptor SyneMonoRegular => ref _assets[26];
    }
    
    public readonly struct Sprites
    {
        public static class DebugUiStyle
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[11];
            
            public static class SpriteIndex
            {
                public const byte NoNinePatch1 = 0;
                public const byte FirstNinePatch = 1;
                public const byte FirstNinePatch_0 = 2;
                public const byte FirstNinePatch_1 = 3;
                public const byte FirstNinePatch_2 = 4;
                public const byte FirstNinePatch_3 = 5;
                public const byte FirstNinePatch_4 = 6;
                public const byte FirstNinePatch_5 = 7;
                public const byte FirstNinePatch_6 = 8;
                public const byte FirstNinePatch_7 = 9;
                public const byte FirstNinePatch_8 = 10;
                public const byte NoNinePatch2 = 11;
                public const byte SliderLeft = 12;
                public const byte SliderRight = 13;
                public const byte SliderCenter = 14;
                public const byte SliderEmptyLeft = 15;
                public const byte SliderEmptyRight = 16;
                public const byte SliderEmptyCenter = 17;
                public const byte SliderBlob = 18;
                public const byte SliderBlobSelected = 19;
                public const byte Button01 = 20;
                public const byte Button01_0 = 21;
                public const byte Button01_1 = 22;
                public const byte Button01_2 = 23;
                public const byte Button01_3 = 24;
                public const byte Button01_4 = 25;
                public const byte Button01_5 = 26;
                public const byte Button01_6 = 27;
                public const byte Button01_7 = 28;
                public const byte Button01_8 = 29;
                public const byte Checkbox = 30;
                public const byte CheckboxSelected = 31;
                public const byte CheckboxCheckmark = 32;
                public const byte CheckboxCross = 33;
                public const byte Button02 = 34;
                public const byte Button02_0 = 35;
                public const byte Button02_1 = 36;
                public const byte Button02_2 = 37;
                public const byte Button02_3 = 38;
                public const byte Button02_4 = 39;
                public const byte Button02_5 = 40;
                public const byte Button02_6 = 41;
                public const byte Button02_7 = 42;
                public const byte Button02_8 = 43;
                public const byte Button02Selected = 44;
                public const byte Button02Selected_0 = 45;
                public const byte Button02Selected_1 = 46;
                public const byte Button02Selected_2 = 47;
                public const byte Button02Selected_3 = 48;
                public const byte Button02Selected_4 = 49;
                public const byte Button02Selected_5 = 50;
                public const byte Button02Selected_6 = 51;
                public const byte Button02Selected_7 = 52;
                public const byte Button02Selected_8 = 53;
                public const byte Button02Pressed = 54;
                public const byte Button02Pressed_0 = 55;
                public const byte Button02Pressed_1 = 56;
                public const byte Button02Pressed_2 = 57;
                public const byte Button02Pressed_3 = 58;
                public const byte Button02Pressed_4 = 59;
                public const byte Button02Pressed_5 = 60;
                public const byte Button02Pressed_6 = 61;
                public const byte Button02Pressed_7 = 62;
                public const byte Button02Pressed_8 = 63;
                public const byte TextBox = 64;
                public const byte TextBox_0 = 65;
                public const byte TextBox_1 = 66;
                public const byte TextBox_2 = 67;
                public const byte TextBox_3 = 68;
                public const byte TextBox_4 = 69;
                public const byte TextBox_5 = 70;
                public const byte TextBox_6 = 71;
                public const byte TextBox_7 = 72;
                public const byte TextBox_8 = 73;
                public const byte TextBoxFocus = 74;
                public const byte TextBoxFocus_0 = 75;
                public const byte TextBoxFocus_1 = 76;
                public const byte TextBoxFocus_2 = 77;
                public const byte TextBoxFocus_3 = 78;
                public const byte TextBoxFocus_4 = 79;
                public const byte TextBoxFocus_5 = 80;
                public const byte TextBoxFocus_6 = 81;
                public const byte TextBoxFocus_7 = 82;
                public const byte TextBoxFocus_8 = 83;
                public const byte TextBoxCursor = 84;
                public const byte SelectBox = 85;
                public const byte SelectBox_0 = 86;
                public const byte SelectBox_1 = 87;
                public const byte SelectBox_2 = 88;
                public const byte SelectBox_3 = 89;
                public const byte SelectBox_4 = 90;
                public const byte SelectBox_5 = 91;
                public const byte SelectBox_6 = 92;
                public const byte SelectBox_7 = 93;
                public const byte SelectBox_8 = 94;
                public const byte SelectBoxFocus = 95;
                public const byte SelectBoxFocus_0 = 96;
                public const byte SelectBoxFocus_1 = 97;
                public const byte SelectBoxFocus_2 = 98;
                public const byte SelectBoxFocus_3 = 99;
                public const byte SelectBoxFocus_4 = 100;
                public const byte SelectBoxFocus_5 = 101;
                public const byte SelectBoxFocus_6 = 102;
                public const byte SelectBoxFocus_7 = 103;
                public const byte SelectBoxFocus_8 = 104;
            }
        }
    }
    
    public readonly struct Materials
    {
        public static class MagicBookOBJ
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[12];
            public const byte BookTexture = 0;
            public const byte BookTexture1 = 1;
        }
        public static class TileLargeForest
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[28];
            public const byte BrownDark = 0;
            public const byte Green = 1;
        }
    }
    
    public readonly struct Shaders
    {
        public static ref readonly Titan.Assets.AssetDescriptor ShaderUIPixel => ref _assets[14];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderUIVertex => ref _assets[15];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderDebugPixel => ref _assets[16];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderDebugVertex => ref _assets[17];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderDeferredLightingPixel => ref _assets[18];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderDeferredLightingVertex => ref _assets[19];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderFullscreenPixel => ref _assets[20];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderFullscreenVertex => ref _assets[21];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderGBufferPixel => ref _assets[22];
        public static ref readonly Titan.Assets.AssetDescriptor ShaderGBufferVertex => ref _assets[23];
        public static ref readonly Titan.Assets.AssetDescriptor SimplePixelShader => ref _assets[24];
        public static ref readonly Titan.Assets.AssetDescriptor SimpleVertexShader => ref _assets[25];
    }
    
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
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture1.png"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture1.png.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 42033152,
                Offset = 67108864,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Audio\brave_space_explorersmix2.ogg"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Audio\brave_space_explorersmix2.ogg.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[2] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 375640,
                Offset = 109142016,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.obj"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.obj.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 1),
            Mesh = new()
            {
                IndexCount = 10434,
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
                Length = 22369648,
                Offset = 109517656,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture.png"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture.png.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
        _assets[4] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 4194304,
                Offset = 131887304,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"box.png"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"box.png.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 16552,
                Offset = 136081608,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click1.ogg"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click1.ogg.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
                Length = 9832,
                Offset = 136098160,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click2.ogg"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click2.ogg.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
                Length = 15196,
                Offset = 136107992,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click3.ogg"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click3.ogg.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
                Length = 6444,
                Offset = 136123188,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click4.ogg"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click4.ogg.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
            Type = Titan.Assets.AssetType.Audio,
            File = 
            { 
                Length = 5648,
                Offset = 136129632,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click5.ogg"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\UIAudio\click5.ogg.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Audio = new()
            {
            }
        };
        _assets[10] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Font,
            File = 
            { 
                Length = 128581,
                Offset = 136135280,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\Cutive_Mono\CutiveMono-Regular.ttf"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\Cutive_Mono\CutiveMono-Regular.ttf.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
        _assets[11] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Sprite,
            File = 
            { 
                Length = 262396,
                Offset = 136263861,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"ui\debug-ui.style.aseprite"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"ui\debug-ui.style.aseprite.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Sprite = new()
            {
                NumberOfSprites = 24,
                NumberOfNinePatchSprites = 9,
                Texture = new()
                {
                    Width = 256,
                    Height = 256,
                    Stride = 1024,
                    BitsPerPixel = 32,
                    DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM
                }
            }
        };
        _assets[12] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Material,
            File = 
            { 
                Length = 34,
                Offset = 136526257,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.mtl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.mtl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(1, 2),
            Material = new()
            {
                MaterialCount = 2
            }
        };
        _assets[13] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Font,
            File = 
            { 
                Length = 12827,
                Offset = 136526291,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\RobotoMono-Regular.ttf"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\RobotoMono-Regular.ttf.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Font = new()
            {
                DefaultGlyphIndex = 24,
                NumberOfGlyphs = 77,
                BytesPerPixel = 1,
                Width = 768,
                Height = 16
            }
        };
        _assets[14] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3168,
                Offset = 136539118,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.UI.Pixel.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.UI.Pixel.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[15] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3080,
                Offset = 136542286,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.UI.Vertex.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.UI.Vertex.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[16] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1732,
                Offset = 136545366,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Debug.Pixel.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Debug.Pixel.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[17] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2716,
                Offset = 136547098,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Debug.Vertex.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Debug.Vertex.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[18] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3768,
                Offset = 136549814,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Pixel.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Pixel.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[19] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2060,
                Offset = 136553582,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Vertex.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.DeferredLighting.Vertex.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[20] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2848,
                Offset = 136555642,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Pixel.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Pixel.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[21] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 1784,
                Offset = 136558490,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Vertex.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.Fullscreen.Vertex.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[22] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3368,
                Offset = 136560274,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Pixel.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Pixel.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[23] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 4868,
                Offset = 136563642,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Vertex.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\Shader.GBuffer.Vertex.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[24] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 2796,
                Offset = 136568510,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_ps_01.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_ps_01.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Pixel
            }
        };
        _assets[25] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Shader,
            File = 
            { 
                Length = 3084,
                Offset = 136571306,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_vs_01.hlsl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"shaders\simple_vs_01.hlsl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Shader = new()
            {
                Type = Titan.Rendering.Resources.ShaderType.Vertex
            }
        };
        _assets[26] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Font,
            File = 
            { 
                Length = 90139,
                Offset = 136574390,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\SyneMono-Regular.ttf"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"fonts\SyneMono-Regular.ttf.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
        _assets[27] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 37616,
                Offset = 136664529,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\tileLarge_forest.obj"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\tileLarge_forest.obj.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(3, 1),
            Mesh = new()
            {
                IndexCount = 1044,
                VertexCount = 1044,
                SubMeshCount = 2,
                MaterialCount = -1,
            }
        };
        _assets[28] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Material,
            File = 
            { 
                Length = 34,
                Offset = 136702145,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\tileLarge_forest.mtl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\tileLarge_forest.mtl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Material = new()
            {
                MaterialCount = 2
            }
        };
        _assets[29] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 113920,
                Offset = 136702179,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"titan.png"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"titan.png.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
        _dependencies[0] = 12;
        _dependencies[1] = 3;
        _dependencies[2] = 0;
        _dependencies[3] = 28;
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(30)]
    private struct __ASSETS__
    {
        private Titan.Assets.AssetDescriptor _ref;
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(4)]
    private struct __ASSETS_DEPS__
    {
        private System.UInt32 _ref;
    }
}
