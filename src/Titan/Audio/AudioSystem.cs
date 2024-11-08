using System.Diagnostics;
using Titan.Assets;
using Titan.Audio.Resources;
using Titan.Audio.XAudio2;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Audio;

[UnmanagedResource]
internal unsafe partial struct AudioSystem
{
    private TitanArray<AudioClip> Queue;
    private uint Count;

    [System(SystemStage.Init)]
    public static void Init(AudioSystem* system, IMemoryManager memoryManager)
    {
        const uint MaxQueueSize = 256u;
        if (!memoryManager.TryAllocArray(out system->Queue, MaxQueueSize))
        {
            Logger.Error<AudioSystem>($"Failed to allocate the audio queue. Count = {MaxQueueSize} Size = {sizeof(AudioClip) * MaxQueueSize}");
            return;
        }
    }

    /// <summary>
    /// Enqueue a clip for playback
    /// <remarks>This method can only be called in the Update stage</remarks>
    /// </summary>
    public void Enqueue(AssetHandle<AudioAsset> asset, in PlaybackSettings settings)
    {
        //NOTE(Jens): Should we do this check?
        Debug.Assert(settings.Frequenzy > 0);
        Debug.Assert(settings.Volume is >= 0 and <= 1.0f);

        var index = Interlocked.Increment(ref Count) - 1;
        if (index < Queue.Length)
        {
            Queue[index].Asset = asset;
            Queue[index].Settings = settings;
        }
        else
        {
            Logger.Warning<AudioAsset>($"Audio Queue full. Clip dropped. Id = {asset.Index}");
        }
    }


    [System(SystemStage.PostUpdate)]
    public static void Update(AudioSystem* system, in XAudio2System audioSystem, AssetsManager assetsManager)
    {
        var count = system->Count;
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; ++i)
        {
            ref readonly var clip = ref system->Queue[i];
            ref readonly var audio = ref assetsManager.Get(clip.Asset);
            Logger.Error<AudioSystem>($"Playing sound. Id = {clip.Asset.Index}");
            if (!audioSystem.Play(audio.AudioData, clip.Settings))
            {
                Logger.Error<AudioSystem>($"Failed to play the sound. Id = {clip.Asset.Index}");
            }
        }

        system->Count = 0;
    }


    [System(SystemStage.Shutdown)]
    public static void Shutdown(AudioSystem* system, IMemoryManager memoryManager)
    {
        memoryManager.FreeArray(ref system->Queue);
    }

    private struct AudioClip
    {
        public AssetHandle<AudioAsset> Asset;
        public PlaybackSettings Settings;
    }
}
