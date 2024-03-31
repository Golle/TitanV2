using System.Text.Json.Serialization;
using Titan.Graphics.Rendering;
using Titan.Windows;

namespace Titan;

[JsonSerializable(typeof(RenderingConfig))]
[JsonSerializable(typeof(WindowConfig))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
internal partial class TitanSerializationContext : JsonSerializerContext;
