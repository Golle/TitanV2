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
    }
    
    public readonly struct Meshes
    {
        public static ref readonly Titan.Assets.AssetDescriptor UnnamedAsset1 => ref _assets[1];
        public static ref readonly Titan.Assets.AssetDescriptor TileLowRed => ref _assets[4];
    }
    
    public readonly struct Sprites
    {
        public static class DebugUiStyle
        {
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[3];
            
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
            public static ref readonly Titan.Assets.AssetDescriptor Asset => ref _assets[5];
            
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
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"redsheet.kbin")
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
                Length = 375640,
                Offset = 524288,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.obj"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\magic_book_OBJ.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
                Length = 67108864,
                Offset = 899928,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture.png"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"testmodels\Book\book_texture.kbin")
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
            Type = Titan.Assets.AssetType.Sprite,
            File = 
            { 
                Length = 262175,
                Offset = 68008792,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"ui\debug-ui.style.aseprite"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"ui\debug-ui.style.kbin")
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
        _assets[4] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Mesh,
            File = 
            { 
                Length = 37616,
                Offset = 68270967,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLow_teamRed.obj"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"models\tileLow_teamRed.kbin")
#else
            AssetPath = Titan.Core.Strings.StringRef.Empty,
            BinaryAssetPath = Titan.Core.Strings.StringRef.Empty
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
        _assets[5] = new Titan.Assets.AssetDescriptor
        {
            Id = Titan.Assets.AssetId.GetNext(),
            RegistryId = Id,
            Type = Titan.Assets.AssetType.Sprite,
            File = 
            { 
                Length = 65662,
                Offset = 68308583,
#if !RELEASE
            AssetPath = Titan.Core.Strings.StringRef.Create(@"ui\ui_style_orange.aseprite"),
            BinaryAssetPath = Titan.Core.Strings.StringRef.Create(@"ui\ui_style_orange.kbin")
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
    }
    [System.Runtime.CompilerServices.InlineArrayAttribute(6)]
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
