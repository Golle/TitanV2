using Titan;
using Titan.Application;
using Titan.Core.Logging;
using Titan.Events;
using Titan.Rendering;
using Titan.Sandbox;
using Titan.Systems;
using Titan.Windows;

using var _ = Logger.Start<ConsoleLogger>(10_000);


var appConfig = new AppConfig("Titan.Sandbox", "0.0.1")
{
    EnginePath = EngineHelper.GetEngineFolder("Titan.sln"),
    ContentPath = EngineHelper.GetContentPath("Titan.Sandbox.csproj", "Assets")
};


App.Create(appConfig)
    .AddModule<GameModule>()
    .AddPersistedConfig(new WindowConfig(1024, 768, true, true))
    .AddPersistedConfig(new RenderingConfig
    {
        Debug = true
    })
    .Build()
    .Run();

namespace Titan.Sandbox
{
    internal class GameModule : IModule
    {
        public static bool Build(IAppBuilder builder, AppConfig config)
        {
            builder.AddSystems<ATestSystem>();
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


    internal partial class ATestSystem
    {
        private static int counter;

        [System(SystemStage.Update, SystemExecutionType.Inline)]
        public static void Update(EventWriter writer, EventReader<TestEvent> reader, EventReader<SomeOtherEvent> reader1)
        {
            return;
            if (counter++ > 100)
            {
                writer.Send(new TestEvent(1));
                writer.Send(new SomeOtherEvent('A'));
                writer.Send(new TestEvent(2));
                writer.Send(new SomeOtherEvent('C'));
                writer.Send(new TestEvent(3));
                writer.Send(new TestEvent(4));
                counter = 0;
            }

            // Check if there are events
            // NOTE(Jens): This check is currently the total number of events, so not accurate
            if (reader.HasEvents)
            {
                Logger.Info($"HAS EVENTS! Count = {reader.EventCount}");
                foreach (ref readonly var @event in reader)
                {
                    Logger.Info($"Event Type = {@event.GetType().Name} Value = {@event.Value}");
                }
            }

            // Could just loop without check as well.
            foreach (ref readonly var otherEvent in reader1)
            {
                Logger.Info($"Event Type = {otherEvent.GetType().Name} Value = {otherEvent.Value}");
            }
        }
    }

    [Event]
    internal readonly partial struct TestEvent(int value)
    {
        public readonly int Value = value;
    }


    [Event]
    internal readonly partial struct SomeOtherEvent(char value)
    {
        public readonly char Value = value;
    }
}
