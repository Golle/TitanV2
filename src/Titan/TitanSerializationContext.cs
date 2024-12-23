using System.Text.Json.Serialization;
using Titan.Audio;
using Titan.ECS.Systems;
using Titan.Rendering;
using Titan.Serialization.Json;
using Titan.Windows;

namespace Titan;

[JsonSerializable(typeof(RenderingConfig))]
[JsonSerializable(typeof(WindowConfig))]
[JsonSerializable(typeof(AudioConfig))]
[JsonSerializable(typeof(CameraStateConfig))]

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true, Converters = [typeof(Vector3Converter)])]
internal partial class TitanSerializationContext : JsonSerializerContext;
