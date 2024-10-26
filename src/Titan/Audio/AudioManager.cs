using Titan.Audio.XAudio2;

namespace Titan.Audio;

//NOTE(Jens): We need a public interface, maybe another structure under XAUdio2 system that handles convertion etc?
public readonly unsafe struct AudioManager
{
    internal AudioManager(XAudio2System * system)
    {

    }

}
