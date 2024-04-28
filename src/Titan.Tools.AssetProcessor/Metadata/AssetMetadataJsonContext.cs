using System.Text.Json.Serialization;

namespace Titan.Tools.AssetProcessor.Metadata;

[JsonSerializable(typeof(AssetFileMetadata))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never
)]
internal partial class AssetMetadataJsonContext : JsonSerializerContext;
