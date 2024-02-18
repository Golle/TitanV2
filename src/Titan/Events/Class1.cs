using Titan.Application;

namespace Titan.Events;
internal sealed class EventsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddService(new EventSystem())
            .AddResource<EventState>();
        return true;
    }

    public static bool Init(IApp app)
    {
        throw new NotImplementedException();
    }

    public static bool Shutdown(IApp app)
    {
        throw new NotImplementedException();
    }
}
