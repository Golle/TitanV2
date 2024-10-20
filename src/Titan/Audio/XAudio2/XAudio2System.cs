using Titan.Audio.CoreAudio;
using Titan.Audio.Events;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
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
    private TitanArray<AudioSink> _audioSinks;

    [System(SystemStage.Init)]
    public static void Init(XAudio2System* system, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<AudioConfig>();
        var createResult = XAudio2Common.XAudio2Create(system->_audio.GetAddressOf(), Flags: 0, XAudio2Processor: XAUDIO2_PROCESSOR.XAUDIO2_DEFAULT_PROCESSOR);
        if (FAILED(createResult))
        {
            Logger.Error<XAudio2System>($"Failed to create the XAudio 2 Device. HRESULT = {createResult}");
            return;
        }
        Logger.Trace<XAudio2System>("Successfully created the XAudio2 Device.");

        if (!memoryManager.TryAllocArray(out system->_audioSinks, config.Channels))
        {
            Logger.Error<XAudio2System>($"Failed to allocate array for Audio Sinks. Count = {config.Channels} Size = {sizeof(AudioSink) * config.Channels} bytes");
            return;
        }

        if (!system->InitAudioVoices(config.Format))
        {
            Logger.Error<XAudio2System>("Failed to create the Audio voices.");
        }
    }

    [System(SystemStage.PreUpdate)]
    public static void Update(in CoreAudioSystem coreAudio, EventReader<AudioDeviceChangedEvent> changed)
    {
        if (!changed.HasEvents)
        {
            return;
        }
        Logger.Trace<XAudio2System>("ly! Audio device changed.");
        if (coreAudio.GetDevices().IsEmpty)
        {
            Logger.Warning<XAudio2System>("There are no Audio Devices. Sound will be disabled.");
        }
        foreach (ref readonly var device in coreAudio.GetDevices())
        {
            Logger.Info<XAudio2System>($"A device. Name = {new string(device.Name)} Id = {new string(device.Id)}");
        }
    }

    private bool InitAudioVoices(in AudioFormat format)
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

        Logger.Trace<XAudio2System>($"Creating {_audioSinks.Length} {nameof(IXAudio2SourceVoice)}s.");

        var blockAlign = (format.Channels * format.BitsPerSample) / 8;
        var averageBytesPerSec = (format.BitsPerSample * format.Channels * format.SamplesPerSec) / 8;
        WAVEFORMATEX voiceFormat = new()
        {
            nBlockAlign = (ushort)blockAlign,
            wFormatTag = XAudio2Constants.WAVE_FORMAT_PCM,
            wBitsPerSample = (ushort)format.BitsPerSample,
            nSamplesPerSec = format.SamplesPerSec,
            nChannels = (ushort)format.Channels,
            cbSize = (ushort)sizeof(WAVEFORMATEX),
            nAvgBytesPerSec = averageBytesPerSec
        };

        for (var i = 0; i < _audioSinks.Length; ++i)
        {
            var sink = _audioSinks.GetPointer(i);
            sink->Callbacks = IXAudio2VoiceCallback.Create(sink);
            var voiceResult = device->CreateSourceVoice(&sink->SourceVoice, &voiceFormat, Flags: 0, MaxFrequencyRatio: 2f, &sink->Callbacks, pSendList: null, pEffectChain: null);
            if (FAILED(voiceResult))
            {
                Logger.Error<XAudio2System>($"Failed to create {nameof(IXAudio2SourceVoice)} at index {i}. HRESULT = {voiceResult}");
                return false;
            }
        }

        return true;
    }



    public static void Shutdown(XAudio2System* system)
    {
        system->_audio.Dispose();
    }

    private struct AudioSink : IXAudio2VoiceCallbackFunctions
    {
        public IXAudio2SourceVoice* SourceVoice;
        public IXAudio2VoiceCallback Callbacks;
    }
}


