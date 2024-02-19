using Titan.Application.Events;
using Titan.Events;
using Titan.Systems;

namespace Titan.Application;
internal partial struct ApplicationLifetimeSystem
{
    [System(SystemStage.PreInit, SystemExecutionType.Inline)]
    public static void Init(ref ApplicationLifetime lifetime)
    {
        lifetime.Active = true;
    }

    [System(SystemStage.Last, SystemExecutionType.Inline)]
    public static void Update(EventReader<EngineShutdownEvent> shutdown, ref ApplicationLifetime lifetime)
    {
        //NOTE(Jens): HasEvents doesn't track per type yet. so we need to do this.. :|

        foreach (var _ in shutdown)
        {
            lifetime.Active = false;
        }
    }
}
