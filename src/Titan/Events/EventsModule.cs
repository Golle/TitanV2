using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Events;
internal sealed class EventsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddResource<EventState>()
            .AddSystems<EventSystem>() //TODO(Jens): When adding a System, we can scan for resources and add them automagically.
            .AddService<IEventSystem, EventSystem>(new EventSystem());
        return true;
    }

    public static bool Init(IApp app)
    {
        var eventState = app.GetResourceHandle<EventState>();
        var eventSystem = app.GetService<EventSystem>();
        var memoryManager = app.GetService<IMemoryManager>();

        var config = app.GetConfigOrDefault<EventConfig>();

        if (!eventSystem.Init(memoryManager, config, eventState))
        {
            Logger.Error<EventsModule>($"Failed to init the {nameof(EventSystem)}.");
            return false;
        }
        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<EventSystem>()
            .Shutdown();
        return true;
    }
}
