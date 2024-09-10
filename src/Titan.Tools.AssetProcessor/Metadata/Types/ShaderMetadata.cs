using System.Text.Json.Serialization;
using Titan.Rendering.Resources;
using Titan.Tools.AssetProcessor.Processors.Shaders;

namespace Titan.Tools.AssetProcessor.Metadata.Types;

internal sealed class ShaderMetadata : AssetFileMetadata
{
    public const string DefaultEntryPoint = "main";
    public const ShaderVersion DefaultShaderVersion = ShaderVersion.Version_6_5;
    public string EntryPoint { get; set; } = DefaultEntryPoint;

    [JsonConverter(typeof(JsonStringEnumConverter<ShaderType>))]
    public ShaderType ShaderType { get; set; } = ShaderType.Vertex;

    [JsonConverter(typeof(JsonStringEnumConverter<ShaderVersion>))]
    public ShaderVersion ShaderVersion { get; set; } = DefaultShaderVersion;
}
