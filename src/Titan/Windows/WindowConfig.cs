using System.Text.Json.Serialization.Metadata;

namespace Titan.Windows;

public record WindowConfig(uint Width, uint Height, bool Windowed) : IConfiguration, IDefault<WindowConfig>, IPersistable<WindowConfig>
{
    public const uint DefaultHeight = 1080;
    public const uint DefaultWidth = 1920;

    public string? Title { get; init; }

    /// <summary>
    /// Placeholder for Reziable windows.
    /// <remarks>This is not supported at the moment.</remarks>
    /// </summary>
    public bool Resizable { get; init; }
    public int X { get; init; } = -1;
    public int Y { get; init; } = -1;
    public static WindowConfig Default => new(DefaultWidth, DefaultHeight, true);
    public static JsonTypeInfo<WindowConfig> TypeInfo => TitanSerializationContext.Default.WindowConfig;
    public static string Filename => "window.conf";
}
