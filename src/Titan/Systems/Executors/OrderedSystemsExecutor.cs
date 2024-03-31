using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Threading;

namespace Titan.Systems.Executors;

/// <summary>
/// The OrderedSystemsExecutor will scheldule systems in parallel using the JobSystem, and respect the dependencies.
/// </summary>
internal sealed unsafe class OrderedSystemsExecutor : ISystemsExecutor
{
    [SkipLocalsInit]
    public static void Run(IJobSystem jobSystem, TitanArray<SystemNode> nodes)
    {
        var count = nodes.Length;
        if (count == 0)
        {
            return;
        }
        var states = stackalloc SystemState[(int)count];
        var handles = stackalloc JobHandle[(int)count];

        var systemsLeft = count;

        // Initial run for systems without dependencies. 
        for (var index = 0; index < count; ++index)
        {
            
            ref readonly var node = ref nodes[index];
            //TODO(Jens): Add check critera, if a system should run or not. And if it should be treated as an "inline" system.
            var shouldRun = true;
            if (shouldRun)
            {
                if (!node.HasDependencies)
                {
                    if (node.ExecutionType is SystemExecutionType.Inline or SystemExecutionType.InlineCheck) // we can do this with flags instead.
                    {
                        node.Execute();
                        systemsLeft--;
                        states[index] = SystemState.Completed;
                        handles[index] = JobHandle.Invalid;
                    }
                    else
                    {
                        handles[index] = jobSystem.Enqueue(node.JobDescriptor);
                        Debug.Assert(handles[index].IsValid, "This was not expected, the job queue failed.");
                        states[index] = SystemState.Running;
                    }
                }
                else
                {
                    handles[index] = JobHandle.Invalid;
                    states[index] = SystemState.Waiting;
                }
            }
            else
            {
                systemsLeft--;
                states[index] = SystemState.Completed;
                handles[index] = JobHandle.Invalid;
            }
        }


        while (systemsLeft > 0)
        {
            //NOTE(Jens): Instead of looping through all systems we can have a "list" of systems where we just swap the system pointers when they are completed, reducing the number of iterations we have to do.
            //NOTE(Jens): For example system at index 8 completes, there are currently 10 systems running, decrease it to 9 and swap system 8 with system 10.
            for (var index = 0; index < count; ++index)
            {
                ref var jobHandle = ref handles[index];
                if (jobHandle.IsValid && jobSystem.IsCompleted(jobHandle))
                {
                    jobSystem.Reset(ref jobHandle);
                    states[index] = SystemState.Completed;
                    systemsLeft--;
                    continue;
                }

                if (states[index] != SystemState.Waiting)
                {
                    continue;
                }

                ref readonly var system = ref nodes[index];
                if (!IsReady(system, states))
                {
                    continue;
                }

                if (system.ExecutionType is SystemExecutionType.Inline or SystemExecutionType.InlineCheck)
                {
                    system.Execute();
                    states[index] = SystemState.Completed;
                    systemsLeft--;
                }
                else
                {
                    handles[index] = jobSystem.Enqueue(system.JobDescriptor);
                    states[index] = SystemState.Running;
                }
            }
            //NOTE(Jens) We need a better way to check for tasks that are completed. this will slow down the entire process. It's possible to just spin as well, but it will consume a single core completely.
            //NOTE(Jens): Keep this until we've figured out a better way.
            //Thread.Sleep(1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static bool IsReady(in SystemNode node, SystemState* systemStates)
    {
        if (node.Dependencies.IsEmpty)
        {
            return true;
        }

        //TODO(Jens): Check the code gen for this, and see if a regular for loop is faster.
        foreach (var index in node.Dependencies.AsReadOnlySpan())
        {
            if (systemStates[index] != SystemState.Completed)
            {
                return false;
            }
        }
        return true;
    }

}
