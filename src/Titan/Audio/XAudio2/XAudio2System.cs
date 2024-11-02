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
    private ComPtr<IXAudio2> Audio;
    private IXAudio2MasteringVoice* MasteringVoice;
    internal TitanArray<AudioSink> AudioSinks;

    [System(SystemStage.Init)]
    public static void Init(XAudio2System* system, in CoreAudioSystem coreAudio, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<AudioConfig>();

        var createResult = XAudio2Common.XAudio2Create(system->Audio.GetAddressOf(), Flags: 0, XAudio2Processor: XAUDIO2_PROCESSOR.XAUDIO2_DEFAULT_PROCESSOR);
        if (FAILED(createResult))
        {
            Logger.Error<XAudio2System>($"Failed to create the XAudio 2 Device. HRESULT = {createResult}");
            return;
        }
        Logger.Trace<XAudio2System>("Successfully created the XAudio2 Device.");

        if (!memoryManager.TryAllocArray(out system->AudioSinks, config.Channels))
        {
            Logger.Error<XAudio2System>($"Failed to allocate array for Audio Sinks. Count = {config.Channels} Size = {sizeof(AudioSink) * config.Channels} bytes");
            return;
        }

        if (!InitAudioVoices(system, coreAudio, configurationManager))
        {
            Logger.Error<XAudio2System>("Failed to create the Audio voices.");
        }
    }

    [System(SystemStage.PreUpdate)]
    public static void Update(XAudio2System* system, in CoreAudioSystem coreAudio, IConfigurationManager configurationManager, EventReader<AudioDeviceChangedEvent> changed)
    {
        if (!changed.HasEvents)
        {
            return;
        }

        //NOTE(Jens): Workaround for known issue with event count/HasEvents.
        foreach (var _ in changed)
        {
            Logger.Trace<XAudio2System>("Audio devices changed, recreating. NOTE: Must check if the device we're using have been changed, otherwise we'll not recreate this.");

            if (!InitAudioVoices(system, coreAudio, configurationManager))
            {
                Logger.Error<XAudio2System>("Failed to recreate the XAudio2 voices.");
            }
            break;
        }
    }

    private void ReleaseVoices()
    {
        foreach (ref var audioSink in AudioSinks.AsSpan())
        {
            // we might need to stop audio from playing before releasing
            if (audioSink.SourceVoice != null)
            {
                audioSink.SourceVoice->DestroyVoice();
            }
        }
        if (MasteringVoice != null)
        {
            MasteringVoice->DestroyVoice();
        }
    }
    private static bool InitAudioVoices(XAudio2System* system, in CoreAudioSystem coreAudio, IConfigurationManager configurationManager)
    {
        // make sure we've released everything.
        system->ReleaseVoices();

        using var _ = new MeasureTime<XAudio2System>("Finished init of Voices in {0} ms");
        var config = configurationManager.GetConfigOrDefault<AudioConfig>();
        var format = config.Format;
        var configDevice = config.Device;
        Logger.Trace<XAudio2System>($"Creating XAudio voices. Stored ID = {configDevice?.Id} Name = {configDevice?.Name}");
        var coreDevice = coreAudio.FindDeviceByID(configDevice?.Id);
        if (coreDevice == null)
        {
            if (configDevice != null)
            {
                Logger.Warning<XAudio2System>($"The configured device could not be found, using default device. DeviceID = {configDevice.Id}");
            }
            else
            {
                Logger.Trace<XAudio2System>("No configured device, using default.");
            }
            coreDevice = coreAudio.GetDefaultDevice();
        }

        if (coreDevice != null)
        {
            // must be a null terminated string. This is not very nice, maybe we should have a helper function for this.
            var length = coreDevice->Id.Length;
            Span<char> deviceId = stackalloc char[length + 1];
            coreDevice->Id.CopyTo(deviceId);
            deviceId[length] = '\0';
            fixed (char* ptr = deviceId)
            {
                var hr = system->Audio.Get()->CreateMasteringVoice(&system->MasteringVoice, szDeviceId: ptr);
                if (FAILED(hr))
                {
                    Logger.Error<XAudio2System>($"Failed to create the {nameof(IXAudio2MasteringVoice)}.");
                    return false;
                }
            }
        }
        else
        {
            Logger.Error<XAudio2System>("No audio device found, can't create the MasteringVoice.");
            return false;
        }

        // figure out how this works
        //device->SetDebugConfiguration();

        Logger.Trace<XAudio2System>($"Creating {system->AudioSinks.Length} {nameof(IXAudio2SourceVoice)}s.");

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

        for (var i = 0; i < system->AudioSinks.Length; ++i)
        {
            var sink = system->AudioSinks.GetPointer(i);
            sink->Callbacks = IXAudio2VoiceCallback.Create(sink);
            var voiceResult = system->Audio.Get()->CreateSourceVoice(&sink->SourceVoice, &voiceFormat, Flags: 0, MaxFrequencyRatio: format.MaxFrequencyRatio, &sink->Callbacks, pSendList: null, pEffectChain: null);
            if (FAILED(voiceResult))
            {
                Logger.Error<XAudio2System>($"Failed to create {nameof(IXAudio2SourceVoice)} at index {i}. HRESULT = {voiceResult}");
                return false;
            }
        }
        return true;
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(XAudio2System* system)
    {
        system->ReleaseVoices();
        system->Audio.Dispose();
    }

    internal struct AudioSink : IXAudio2VoiceCallbackFunctions
    {
        public IXAudio2SourceVoice* SourceVoice;
        public IXAudio2VoiceCallback Callbacks;
    }
}
