using System.Text.Json.Serialization;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Metadata;

[JsonSerializable(typeof(AssetFileMetadata))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    Converters = [typeof(JsonStringEnumConverter<ImageType>)]
)]
internal partial class AssetMetadataJsonContext : JsonSerializerContext;
