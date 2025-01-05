using Titan.Application.Events;
using Titan.Events;
using Titan.Systems;

namespace Titan.Application;

/// <summary>
/// State is updated in system state First, do not use these values in system that is in state First or Last. 
/// </summary>
internal partial struct EngineState
{
    /// <summary>
    /// The number of frames since the start of the application.
    /// </summary>
    public static ulong FrameCount { get; private set; }
    /// <summary>
    /// The current index of the frame, this uses <see cref="GlobalConfiguration.MaxRenderFrames"/> for a modulo operator.
    /// </summary>
    public static uint FrameIndex { get; private set; }
    /// <summary>
    /// True until a Shutdown event occured.
    /// </summary>
    public static bool Active { get; private set; }

    [System(SystemStage.PreInit, SystemExecutionType.Inline)]
    public static void Init()
    {
        Active = true;
        FrameCount = 0;
        FrameIndex = 0;
    }

    [System(SystemStage.First, SystemExecutionType.Inline)]
    public static void First()
    {
        // increase the frame count at the start of the frame, that means every stage will have the same frame count. 
        FrameCount++;

#if TRIPLE_BUFFERING
        FrameIndex = (FrameIndex + 1) % GlobalConfiguration.MaxRenderFrames;
#else
        FrameIndex = (FrameIndex + 1) & 0x1;
#endif
    }

    [System(SystemStage.Last, SystemExecutionType.Inline)]
    public static void Last(EventReader<EngineShutdownEvent> shutdown)
    {
        //NOTE(Jens): HasEvents doesn't track per type yet. so we need to do this.. :|

        foreach (var _ in shutdown)
        {
            Active = false;
        }
    }
}
