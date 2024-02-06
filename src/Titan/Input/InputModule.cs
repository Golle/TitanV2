using Titan.Application;

namespace Titan.Input;
internal class InputModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddResource<InputState>()
            .AddSystems<InputSystem>();

        return true;
    }

    public static bool Init(IApp app)
    {
        return true;
    }

    public static bool Shutdown(IApp app)
    {
        return true;
    }
}
