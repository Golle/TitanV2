using Titan.Application;

namespace Titan.Input;
internal sealed class InputModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddResource<InputState>()
            .AddSystems<InputSystem>();

        return true;
    }
}
