using Titan.Application.Events;
using Titan.Audio.Events;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Events;
using Titan.Platform.Win32;
using Titan.Platform.Win32.XAudio2;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Audio.XAudio2;

[UnmanagedResource]
internal unsafe partial struct XAudio2System
{
    private ComPtr<IXAudio2> _audio;

    [System(SystemStage.Init)]
    public static void Init(XAudio2System* system, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<AudioConfig>();
        var createResult = XAudio2Common.XAudio2Create(system->_audio.GetAddressOf(), Flags: 0, XAudio2Processor: XAUDIO2_PROCESSOR.XAUDIO2_DEFAULT_PROCESSOR);
        if (FAILED(createResult))
        {
            Logger.Error<XAudio2System>($"Failed to create the XAudio 2 Device. HRESULT = {createResult}");
            return;
        }
        Logger.Trace<XAudio2System>("Successfully created the XAudio2 Device.");
        Logger.Error<AudioConfig>($"AUDIO: Channels = {config.NumberOfChannels}");

        if (!system->InitAudioVoices(config))
        {
            Logger.Error<XAudio2System>("Failed to create the Audio voices.");
        }
    }


    [System(SystemStage.PreUpdate)]
    public static void Update(EventReader<AudioDeviceChangedEvent> changed)
    {
        if (changed.HasEvents)
        {
            Logger.Trace<XAudio2System>("ly! Audio device changed.");

        }
    }

    private bool InitAudioVoices(AudioConfig config)
    {

        var device = _audio.Get();
        IXAudio2MasteringVoice* masteringVoice;
        var result = device->CreateMasteringVoice(&masteringVoice);

        if (FAILED(result))
        {
            Logger.Error<XAudio2System>("Failed to create the Mastering Voice.");
            return false;
        }

        // figure out how this works
        //device->SetDebugConfiguration();
        device->CreateSourceVoice()
            IXAudio2VoiceCallback
        return true;
    }



    public static void Shutdown(XAudio2System* system)
    {
        system->_audio.Dispose();
    }

}
