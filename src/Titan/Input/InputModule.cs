using Titan.Application;
using Titan.Core.Logging;

namespace Titan.Input;
internal class InputModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddService<IInputHandler, InputHandler>(new InputHandler());
        return true;
    }

    public static bool Init(IApp app)
    {
        var inputSystem = app.GetService<InputHandler>();

        if (!inputSystem.Init())
        {
            Logger.Error<InputModule>($"Failed to init the {nameof(InputHandler)}");
            return false;
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<InputHandler>()
            .Shutdown();
        return true;
    }
}
