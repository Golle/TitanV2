using System.Text.Json.Serialization;
using Titan.Core.Maths;
using Titan.Tools.AssetProcessor.Utils;

namespace Titan.Tools.AssetProcessor.Metadata.Types;


internal enum ImageType
{
    Texture,
    SpriteSheet
}

internal enum CompressionType
{
    None,
    BC7
}

internal sealed class ImageMetadata : AssetFileMetadata
{
    public ImageType Type { get; set; }
    public CompressionType Compression { get; set; } = CompressionType.None;
    public SpriteMetadata[] Sprites { get; set; } = [];
    //public NinePatchSpriteMetadata[] NinePatch { get; set; } = [];

    [JsonIgnore]
    public bool IsAseprite => FileExtension == ".aseprite";
}

internal class SpriteMetadata
{
    public string? Name { get; set; }
    [JsonConverter(typeof(SizeIntArrayJsonConverter))]
    public required Size BottomLeft { get; set; }
    [JsonConverter(typeof(SizeIntArrayJsonConverter))]
    public required Size TopRight { get; set; }

    public NinePatchInsets? NinePatch { get; set; }
}

internal sealed class NinePatchInsets
{
    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }
}
