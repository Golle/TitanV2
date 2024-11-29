using System.Text.Json.Serialization;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.Tools.AssetProcessor.Processors.Audio;

namespace Titan.Tools.AssetProcessor.Metadata;

//NOTE(Jens): This must contain all types :O

[JsonPolymorphic]
[JsonDerivedType(typeof(ImageMetadata), nameof(ImageMetadata))]
[JsonDerivedType(typeof(ShaderMetadata), nameof(ShaderMetadata))]
[JsonDerivedType(typeof(ShaderInfoMetadata), nameof(ShaderInfoMetadata))]
[JsonDerivedType(typeof(ObjModelMetadata), nameof(ObjModelMetadata))]
[JsonDerivedType(typeof(MtlMetadata), nameof(MtlMetadata))]
[JsonDerivedType(typeof(FontMetadata), nameof(FontMetadata))]
[JsonDerivedType(typeof(AudioMetadata), nameof(AudioMetadata))]
internal abstract class AssetFileMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public long FileSize { get; set; }
    public Guid[] DependsOn { get; set; } = [];
    public bool Skip { get; set; }

    /// <summary>
    /// License is used for tracking resources that have a license attaches to it<br/>
    /// <remarks>This field is not used by engine or the asset tool at the moment</remarks>
    /// </summary>
    public string? License { get; set; }
    /// <summary>
    /// Include the creator of the asset file.<br/>
    /// <remarks>This field is not used by engine or the asset tool at the moment</remarks>
    /// </summary>
    public string? Creator { get; set; }
    /// <summary>
    /// Link to the asset file, if applicable.<br/>
    /// <remarks>This field is not used by engine or the asset tool at the moment</remarks>
    /// </summary>
    public string? Link { get; set; }

    [JsonIgnore]
    public IReadOnlyList<AssetFileMetadata> Dependencies { get; set; } = [];

    [JsonIgnore]
    public string ContentFileFullPath { get; set; } = string.Empty;
    [JsonIgnore]
    public string ContentFileRelativePath { get; set; } = string.Empty;
    [JsonIgnore]
    public string BinaryFileFullPath { get; set; } = string.Empty;
    [JsonIgnore]
    public string BinaryFileRelativePath { get; set; } = string.Empty;
    [JsonIgnore]
    public string FileExtension { get; set; } = string.Empty;
}
