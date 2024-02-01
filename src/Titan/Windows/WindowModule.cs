using System.Text.Json.Serialization.Metadata;
using Titan.Application;
using Titan.Windows.Linux;
using Titan.Windows.Win32;

namespace Titan.Windows;

public record WindowConfig(uint Width, uint Height, bool Windowed, bool Resizable) : IConfiguration, IDefault<WindowConfig>, IPersistable<WindowConfig>
{
    public const uint DefaultHeight = 1080;
    public const uint DefaultWidth = 1920;

    public string? Title { get; init; }

    public int X { get; init; } = -1;
    public int Y { get; init; } = -1;
    public static WindowConfig Default => new(DefaultWidth, DefaultHeight, true, true);
    public static JsonTypeInfo<WindowConfig> TypeInfo => TitanSerializationContext.Default.WindowConfig;
    public static string Filename => "window.conf";
}

internal class WindowModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        if (GlobalConfiguration.Platform == Platforms.Windows)
        {
            builder.AddModule<Win32WindowModule>();
        }
        else if (GlobalConfiguration.Platform == Platforms.Linux)
        {
            builder.AddModule<LinuxWindowModule>();
        }
        else
        {
            throw new PlatformNotSupportedException($"The platform {GlobalConfiguration.Platform} is not supported.");
        }
        return true;
    }

    public static bool Init(IApp app) => true;
    public static bool Shutdown(IApp app) => true;
}
