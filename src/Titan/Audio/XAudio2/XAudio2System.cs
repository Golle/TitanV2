using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Audio.XAudio2;

[UnmanagedResource]
internal unsafe partial struct XAudio2System
{
    [System(SystemStage.Init)]
    public static void Init(XAudio2System * system, IConfigurationManager configurationManager)
    {

        var config = configurationManager.GetConfigOrDefault<AudioConfig>();

        Logger.Error<AudioConfig>($"AUDIO: Channels = {config.NumberOfChannels}");


    }


}
