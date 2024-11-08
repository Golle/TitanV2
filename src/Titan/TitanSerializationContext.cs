using System.Text.Json.Serialization;
using Titan.Audio;
using Titan.Rendering;
using Titan.Windows;

namespace Titan;

[JsonSerializable(typeof(RenderingConfig))]
[JsonSerializable(typeof(WindowConfig))]
[JsonSerializable(typeof(AudioConfig))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
internal partial class TitanSerializationContext : JsonSerializerContext;
