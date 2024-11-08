using System.Diagnostics;
using Titan.Assets;
using Titan.Audio.Resources;
using Titan.Core;

namespace Titan.Audio;

public struct Audio;
public record struct PlaybackSettings(float Volume = 1.0f, float Frequenzy = 1.0f, bool Loop = false);
public readonly unsafe struct AudioManager
{
    private readonly AudioSystem* _system;
    internal AudioManager(AudioSystem* system) => _system = system;

    public void Stop(Handle<Audio> handle)
    {
        //TODO(Jens): Implement this when we have a way to persist sounds, for now keep it simple with play once
    }

    public void Pause(Handle<Audio> handle)
    {
        //TODO(Jens): Implement this when we have a way to persist sounds, for now keep it simple with play once
    }

    /// <summary>
    /// Set the volume of the clip playing
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    public void SetVolume(Handle<Audio> handle, float value)
    {
        Debug.Assert(value is <= 1.0f and >= -1.0f);
        //TODO(Jens): Implement this when we have a way to persist sounds, for now keep it simple with play once
    }

    /// <summary>
    /// Set the master volume
    /// </summary>
    /// <param name="value"></param>
    public void SetMasterVolume(float value)
    {
        Debug.Assert(value is <= 1.0f and >= -1.0f);
        //TODO(Jens): Implement this when we have a way to persist sounds, for now keep it simple with play once
    }

    public void PlayOnce(AssetHandle<AudioAsset> audio)
        => PlayOnce(audio, new PlaybackSettings(Loop: false));

    public void PlayOnce(AssetHandle<AudioAsset> audio, in PlaybackSettings playbackSettings)
        => _system->Enqueue(audio, playbackSettings);
}
