using System.Text.Json.Serialization;
using Titan.Core.Maths;
using Titan.Tools.AssetProcessor.Utils;

namespace Titan.Tools.AssetProcessor.Metadata.Types;


internal enum ImageType
{
    Texture,
    SpriteSheet
}

internal sealed class ImageMetadata : AssetFileMetadata
{
    public ImageType Type { get; set; }
    public SpriteMetadata[] Sprites { get; set; } = [];
    public NinePatchSpriteMetadata[] NinePatch { get; set; } = [];
}

internal class SpriteMetadata
{
    public string? Name { get; set; }
    [JsonConverter(typeof(SizeIntArrayJsonConverter))]
    public required Size BottomLeft { get; set; }
    [JsonConverter(typeof(SizeIntArrayJsonConverter))]
    public required Size TopRight { get; set; }
}

internal sealed class NinePatchSpriteMetadata : SpriteMetadata
{
    public int Left { get; set; }
    public int Right { get; set; }
    public int Top { get; set; }
    public int Bottom { get; set; }
}
