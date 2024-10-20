using Titan.Application;

namespace Titan.Audio.CoreAudio;
internal class CoreAudioModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder

            .AddSystemsAndResource<CoreAudioSystem>();
        return true;
    }
}
