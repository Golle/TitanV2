using System.Text.Json.Serialization;
using Titan.Graphics.Resources;
using Titan.Tools.AssetProcessor.Processors.Shaders;

namespace Titan.Tools.AssetProcessor.Metadata.Types;

internal sealed class ShaderMetadata : AssetFileMetadata
{
    public const string DefaultEntryPoint = "main";
    public string EntryPoint { get; set; } = DefaultEntryPoint;

    [JsonConverter(typeof(JsonStringEnumConverter<ShaderType>))]
    public ShaderType ShaderType { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter<ShaderVersion>))]
    public ShaderVersion ShaderVersion { get; set; }
}
