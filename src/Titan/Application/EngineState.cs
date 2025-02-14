using System.Diagnostics;
using Titan.Application.Events;
using Titan.Events;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Application;


public static class GameTime
{
    public static long LastTimestamp { get; internal set; }
    public static float DeltaTimeSeconds => (float)DeltaTime.TotalSeconds;
    public static float DeltaTimeMillis => (float)DeltaTime.TotalMilliseconds;
    public static TimeSpan DeltaTime { get; internal set; }
}

/// <summary>
/// State is updated in system state First, do not use these values in system that is in state First or Last. 
/// </summary>
public partial struct EngineState
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

    public static int WindowWidth { get; private set; }
    public static int WindowHalfWidth { get; private set; }
    public static int WindowHeight { get; private set; }
    public static int WindowHalfHeight { get; private set; }

    [System(SystemStage.PreInit, SystemExecutionType.Inline)]
    internal static void Init()
    {
        Active = true;
        FrameCount = 0;
        FrameIndex = 0;
        GameTime.LastTimestamp = Stopwatch.GetTimestamp();
    }

    [System(SystemStage.First, SystemExecutionType.Inline)]
    internal static void First(in Window window)
    {
        // increase the frame count at the start of the frame, that means every stage will have the same frame count. 
        FrameCount++;

#if TRIPLE_BUFFERING
        FrameIndex = (FrameIndex + 1) % GlobalConfiguration.MaxRenderFrames;
#else
        FrameIndex = (FrameIndex + 1) & 0x1;
#endif
        var current = Stopwatch.GetTimestamp();
        GameTime.DeltaTime = Stopwatch.GetElapsedTime(GameTime.LastTimestamp, current);
        GameTime.LastTimestamp = current;

        WindowHeight = window.Height;
        WindowWidth = window.Width;
        WindowHalfHeight = WindowHeight >> 1;
        WindowHalfWidth = WindowWidth >> 1;

    }

    [System(SystemStage.Last, SystemExecutionType.Inline)]
    internal static void Last(EventReader<EngineShutdownEvent> shutdown)
    {
        //NOTE(Jens): HasEvents doesn't track per type yet. so we need to do this.. :|

        foreach (var _ in shutdown)
        {
            Active = false;
        }
    }
}
