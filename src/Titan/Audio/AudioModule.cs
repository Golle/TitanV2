using System.Text.Json.Serialization.Metadata;
using Titan.Application;
using Titan.Audio.CoreAudio;
using Titan.Audio.Resources;
using Titan.Audio.XAudio2;
using Titan.Core.Memory;

namespace Titan.Audio;

public record AudioDevice(string Id, string Name);
public record AudioFormat : IConfiguration, IDefault<AudioFormat>
{
    public const float DefaultMaxFrequencyRatio = 4f;
    public const uint DefaultChannels = 2; // stereo
    public const uint DefaultBitsPerSample = 16;
    public const uint DefaultSamplesPerSec = 44100;

    public uint Channels { get; init; }
    public uint BitsPerSample { get; init; }
    public uint SamplesPerSec { get; init; }
    public float MaxFrequencyRatio { get; init; }
    public static AudioFormat Default => new()
    {
        MaxFrequencyRatio = DefaultMaxFrequencyRatio,
        BitsPerSample = DefaultBitsPerSample,
        Channels = DefaultChannels,
        SamplesPerSec = DefaultSamplesPerSec
    };
}

public record AudioConfig : IConfiguration, IDefault<AudioConfig>, IPersistable<AudioConfig>
{
    public const uint DefaultChannels = 32u;
    public const uint DefaultMaxLoadedSounds = 512;
    public static readonly uint DefaultMaxAudioBufferBytes = MemoryUtils.MegaBytes(256);

    /// <summary>
    /// The number of concurrent sounds, defualt <see cref="DefaultChannels"/>
    /// </summary>
    public uint Channels { get; init; }
    public uint MaxAudioBufferBytes { get; init; }
    public uint MaxLoadedSounds { get; init; }
    public required AudioFormat Format { get; init; }
    public AudioDevice? Device { get; init; }
    public static AudioConfig Default => new()
    {
        Channels = DefaultChannels,
        MaxAudioBufferBytes = DefaultMaxAudioBufferBytes,
        MaxLoadedSounds = DefaultMaxLoadedSounds,
        Format = AudioFormat.Default
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
                .AddModule<CoreAudioModule>()
                .AddModule<XAudio2Module>()
                .AddSystemsAndResource<AudioSystem>()
                .AddAssetLoader<AudioLoader>()
                ;
        }
        else
        {
            throw new NotSupportedException($"Platform {GlobalConfiguration.Platform} does not support Audio.");
        }

        return true;
    }
}
