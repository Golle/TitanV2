using Titan.Application;

namespace Titan.Audio.XAudio2;


internal sealed class XAudio2Module : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddSystemsAndResource<XAudio2System>();

        return true;
    }
}
