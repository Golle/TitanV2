using System.Text.Json.Serialization;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Metadata;

//NOTE(Jens): This must contain all types :O

[JsonPolymorphic]
[JsonDerivedType(typeof(ImageMetadata), nameof(ImageMetadata))]
[JsonDerivedType(typeof(ShaderMetadata), nameof(ShaderMetadata))]
[JsonDerivedType(typeof(ObjModelMetadata), nameof(ObjModelMetadata))]
[JsonDerivedType(typeof(MtlMetadata), nameof(MtlMetadata))]
[JsonDerivedType(typeof(AsepriteMetadata), nameof(AsepriteMetadata))]
[JsonDerivedType(typeof(FontMetadata), nameof(FontMetadata))]
internal abstract class AssetFileMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public long FileSize { get; set; }
    public Guid[] DependsOn { get; set; } = Array.Empty<Guid>();
    public bool Skip { get; set; }

    [JsonIgnore]
    public IReadOnlyList<AssetFileMetadata> Dependencies { get; set; } = Array.Empty<AssetFileMetadata>();

    [JsonIgnore]
    public string ContentFileFullPath { get; set; } = string.Empty;
    [JsonIgnore]
    public string ContentFileRelativePath { get; set; } = string.Empty;
}
