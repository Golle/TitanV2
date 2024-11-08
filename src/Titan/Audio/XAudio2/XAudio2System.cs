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

/*
 * TODO
 * A lot of things have not been implemented yet
 * 1. Recover from lost devices
 * 2. Stored state, for example when you have long running music clips or some other sound that you want to repeat
 * 3. Failed sinks
 * 4. Immediate play, without putting a message on the queue
 * 5. Mono + Stero? currently only supports stereo, maybe we want some channels dedicated for UI/Mono audio.
 *      Mono is half the size of Stereo
 */

[UnmanagedResource]
internal unsafe partial struct XAudio2System
{
    private ComPtr<IXAudio2> Audio;
    private IXAudio2MasteringVoice* MasteringVoice;
    internal TitanArray<AudioSink> AudioSinks;
    private int NextIndex;

    [System(SystemStage.Init)]
    public static void Init(XAudio2System* system, in CoreAudioSystem coreAudio, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        system->NextIndex = 0;

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
    public static void PreUpdate(XAudio2System* system, in CoreAudioSystem coreAudio, IConfigurationManager configurationManager, EventReader<AudioDeviceChangedEvent> changed)
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

    [System]
    public static void Update(XAudio2System* system)
    {
        foreach (ref var sink in system->AudioSinks.AsSpan())
        {
            if (sink.State == AudioPlaybackState.Completed)
            {
                sink.State = AudioPlaybackState.Available;
            }
            else if (sink.State == AudioPlaybackState.Error)
            {
                //TODO(Jens): Do we need to recreate the voice?
                Logger.Error<AudioSystem>($"An AudionSink had an error. HRESULT = {sink.LastError}");
                sink.State = AudioPlaybackState.Available;
            }
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
            sink->State = AudioPlaybackState.Available;
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
        public AudioPlaybackState State;
        public IXAudio2SourceVoice* SourceVoice;
        public IXAudio2VoiceCallback Callbacks;

        public HRESULT LastError;

        public void Play(TitanBuffer buffer, in PlaybackSettings settings)
        {
            State = AudioPlaybackState.Playing;
            XAUDIO2_BUFFER xAudioBuffer = new()
            {
                AudioBytes = buffer.Size,
                pAudioData = buffer.AsPointer(),
                pContext = null,
                LoopCount = settings.Loop ? XAudio2Constants.XAUDIO2_MAX_LOOP_COUNT : 0u
            };
            SourceVoice->SetFrequencyRatio(settings.Frequenzy);
            SourceVoice->SetVolume(settings.Volume);
            SourceVoice->SubmitSourceBuffer(&xAudioBuffer, null);
            SourceVoice->Start();
        }

        public void OnBufferEnd(void* pBufferContext)
        {
            State = AudioPlaybackState.Completed;
        }

        public void OnVoiceError(void* pBufferContext, HRESULT error)
        {
            State = AudioPlaybackState.Error;
            LastError = error;
        }
    }

    public readonly bool Play(TitanBuffer buffer, in PlaybackSettings settings)
    {
        var index = GetAvailableSinkIndex();
        if (index == -1)
        {
            Logger.Warning<XAudio2System>("Failed to get an available sink.");
            return false;
        }

        var sink = AudioSinks.GetPointer(index);
        sink->Play(buffer, settings);
        return true;
    }


    private readonly int GetAvailableSinkIndex()
    {
        //NOTE(Jens): Not thread safe implementation. if we ever want to call this from multiple threads, this needs to change.
        ref var nextIndex = ref *MemoryUtils.AsPointer(NextIndex);
        var count = (int)AudioSinks.Length;
        for (var i = 0; i < count; ++i)
        {
            nextIndex = (nextIndex + 1) % count;
            if (AudioSinks[nextIndex].State == AudioPlaybackState.Available)
            {
                return nextIndex;
            }
        }
        return -1;
    }

    internal enum AudioPlaybackState
    {
        Available,
        // Acquired, // use this if we need to make it thread safe
        Playing,
        Paused, // nyi
        Error,
        Completed,
        NotCreated  // nyi
    }
}
