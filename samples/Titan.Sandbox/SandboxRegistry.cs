// Auto-Generated
#pragma warning disable

namespace Titan.Sandbox;
public readonly struct SandboxRegistry : Titan.Assets.IAssetRegistry
{
    public static Titan.Assets.RegistryId Id { get; } = Titan.Assets.RegistryId.GetNext();
    private static readonly __ASSETS__ _assets;
    private static readonly __ASSETS_DEPS__ _dependencies;
    public static ReadOnlySpan<char> GetFilePath()
    {
        ReadOnlySpan<char> path = "sandbox.tbin";
        return path;
    }
    public static ReadOnlySpan<Titan.Assets.AssetDescriptor> GetAssetDescriptors()
        => _assets;
    public static ReadOnlySpan<System.UInt32> GetDependencies(in Titan.Assets.AssetDescriptor descriptor)
        => ((ReadOnlySpan<System.UInt32>)_dependencies).Slice((int)descriptor.Dependencies.Index, (int)descriptor.Dependencies.Count);
    public readonly struct Textures
    {
        public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset0 => ref _assets[0];
        public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset2 => ref _assets[2];
        public static ref readonly Titan.Assets.AssetDescriptor Albedo1 => ref _assets[3];
    }
    
    public readonly struct Meshes
    {
        public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset1 => ref _assets[1];
        public static ref readonly Titan.Assets.AssetDescriptor TileLowRed => ref _assets[7];
    }
    
    public readonly struct Sprites
    {
        public static class DebugUiStyle
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[4];
            
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
            }
        }
        public static class UiStyleOrange
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[9];
            
            public static class SpriteIndex
            {
                public const byte Button = 0;
                public const byte Checkbox1 = 1;
                public const byte Checkbox1Down = 2;
                public const byte Checkbox1Checked = 3;
                public const byte Input1 = 4;
                public const byte Input1Selected = 5;
                public const byte Slider1 = 6;
                public const byte Slider1Blob = 7;
                public const byte Slider1BlobBlack = 8;
                public const byte Radio1 = 9;
                public const byte Radio1Selected = 10;
                public const byte Radio1Hover = 11;
                public const byte Progress1 = 12;
                public const byte Progress1Bar = 13;
            }
        }
    }
    
    public readonly struct Materials
    {
        public static class MagicBookOBJ
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[5];
            public const byte BookTexture = 0;
        }
        public static class TileLargeForest
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[6];
            public const byte BrownDark = 0;
            public const byte Green = 1;
        }
        public static class TileLowTeamRed
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[8];
            public const byte Metal = 0;
            public const byte Red = 1;
        }
    }
    
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
            AssetPath = Titan.Core.Strings.StringRef.Create(@"redsheet.png"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"redsheet.png.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
                Length = 375644,
                Offset = 524288,
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
        _assets[2] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Texture,
            File = 
            { 
                Length = 67108864,
                Offset = 899932,
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
                Length = 16384,
                Offset = 68008796,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"materials\albedo_1.aseprite"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"materials\albedo_1.aseprite.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Texture2D = new()
            {
                Width = 64,
                Height = 64,
                Stride = 256,
                BitsPerPixel = 32,
                DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM
            }
        };
        _assets[4] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Sprite,
            File = 
            { 
                Length = 262175,
                Offset = 68025180,
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
                NumberOfSprites = 3,
                NumberOfNinePatchSprites = 1,
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
        _assets[5] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Material,
            File = 
            { 
                Length = 17,
                Offset = 68287355,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.mtl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.mtl.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Material = new()
            {
                MaterialCount = 1
            }
        };
        _assets[6] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Material,
            File = 
            { 
                Length = 34,
                Offset = 68287372,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLarge_forest.mtl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLarge_forest.mtl.kbin")
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
        _assets[7] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 37624,
                Offset = 68287406,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLow_teamRed.obj"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLow_teamRed.obj.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(1, 1),
            Mesh = new()
            {
                IndexCount = 1044,
                VertexCount = 1044,
                SubMeshCount = 2,
                MaterialCount = -1,
            }
        };
        _assets[8] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Material,
            File = 
            { 
                Length = 34,
                Offset = 68325030,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLow_teamRed.mtl"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLow_teamRed.mtl.kbin")
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
        _assets[9] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Sprite,
            File = 
            { 
                Length = 65662,
                Offset = 68325064,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"ui\ui_style_orange.aseprite"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"ui\ui_style_orange.aseprite.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
#endif
            },
            Dependencies = new(0, 0),
            Sprite = new()
            {
                NumberOfSprites = 14,
                NumberOfNinePatchSprites = 0,
                Texture = new()
                {
                    Width = 128,
                    Height = 128,
                    Stride = 512,
                    BitsPerPixel = 32,
                    DXGIFormat = Titan.Platform.Win32.DXGI.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM
                }
            }
        };
        _dependencies[0] = 5;
        _dependencies[1] = 8;
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(10)]
    private struct __ASSETS__
    {
        private Titan.Assets.AssetDescriptor _ref;
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(2)]
    private struct __ASSETS_DEPS__
    {
        private System.UInt32 _ref;
    }
}
