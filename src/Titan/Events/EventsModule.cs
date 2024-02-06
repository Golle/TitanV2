using Titan.Application;
using Titan.Core.Logging;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Events;
internal sealed class EventsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddResource<EventState>()
            .AddSystems<EventSystem>(); //TODO(Jens): When adding a System, we can scan for resources and add them automagically.
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


internal unsafe partial struct EventSystem
{
    [System(SystemStage.First)]
    public static void Update(EventState * eventState)
    {

        eventState->A++;

        Logger.Info<EventSystem>($"State: {eventState->A}");

    }

}

[UnmanagedResource]
internal unsafe partial struct EventState
{

    public int A;
}
