using System.Text.Json.Serialization.Metadata;
using Titan.Application;
using Titan.Audio.XAudio2;

namespace Titan.Audio;

public record AudioConfig : IConfiguration, IDefault<AudioConfig>, IPersistable<AudioConfig>
{
    public const int DefaultNumberOfChannels = 32;
    public int NumberOfChannels { get; init; }

    public static AudioConfig Default => new AudioConfig
    {
        NumberOfChannels = DefaultNumberOfChannels
    };

    public static JsonTypeInfo<AudioConfig> TypeInfo => TitanSerializationContext.Default.AudioConfig;
    public static string Filename => "audio.conf";
}

internal sealed class AudioModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        if (GlobalConfiguration.Platform == Platforms.Windows)
        {
            builder
                .AddModule<XAudio2Module>();
        }
        else
        {
            throw new NotSupportedException($"Platform {GlobalConfiguration.Platform} does not support Audio.");
        }

        return true;
    }
}

