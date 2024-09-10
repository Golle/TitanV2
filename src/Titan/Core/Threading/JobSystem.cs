using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;

namespace Titan.Core.Threading;

public record JobSystemConfig(uint NumberOfWorkers, uint MaxQueuedJobs) : IConfiguration, IDefault<JobSystemConfig>
{
    public static uint DefaultNumberOfWorkers = (uint)(Environment.ProcessorCount - 1);
    public static uint DefaultMaxQueuedJobs = 1024;
    public static JobSystemConfig Default => new(DefaultNumberOfWorkers, DefaultMaxQueuedJobs);
}

internal sealed unsafe class JobSystem(IThreadManager threadManager) : IJobSystem
{
    private const int HandleOffset = 1002;
    private uint MaxQueuedJobs;
    private uint JobQueueMask;

    private static readonly TimeSpan DefaultWaitTime = TimeSpan.FromMilliseconds(100);

    //TODO(Jens): Jobs can have a fixed sized "buffer" where we can either store a pointer to some data sent by the user or copy entire value types. Would simplify the usage.
    private TitanArray<Job> _jobs;
    private TitanArray<Worker> _workers;

    private volatile uint _nextJob;
    private volatile uint _nextJobSlot;

    private GCHandle _handle;
    private SemaphoreSlim _notifier = null!;
    private IMemoryManager? _memoryManager;

    public bool Init(in JobSystemConfig config, IMemoryManager memoryManager)
    {
        if (!MathUtils.IsPowerOf2(config.MaxQueuedJobs))
        {
            Logger.Error<JobSystem>($"The {nameof(MaxQueuedJobs)} config value must be a power of 2. Value = {config.MaxQueuedJobs}");
            return false;
        }
        if (config.NumberOfWorkers == 0)
        {
            Logger.Error<JobSystem>("The number of workers must be greater than 0.");
            return false;
        }

        Logger.Trace<JobSystem>($"Init Job System. Number of Workers = {config.NumberOfWorkers} Max Queued Jobs = {config.MaxQueuedJobs}");

        if (config.NumberOfWorkers > Environment.ProcessorCount)
        {
            Logger.Warning<JobSystem>($"You're about to create more workers than there are CPU cores. Worker count = {config.NumberOfWorkers} CPU Cores = {Environment.ProcessorCount}");
        }

        if (!memoryManager.TryAllocArray(out _workers, config.NumberOfWorkers))
        {
            Logger.Error<JobSystem>($"Failed to allocate memory for the Workers. Count = {config.NumberOfWorkers} Size = {config.NumberOfWorkers * sizeof(Worker)}");
            return false;
        }

        if (!memoryManager.TryAllocArray(out _jobs, config.MaxQueuedJobs))
        {
            Logger.Error<JobSystem>($"Failed to allocate memory for the Jobs. Count = {config.MaxQueuedJobs} Size = {config.NumberOfWorkers * sizeof(Job)}");
            return false;
        }

        MaxQueuedJobs = config.MaxQueuedJobs;
        JobQueueMask = MaxQueuedJobs - 1;
        _notifier = new(0, (int)MaxQueuedJobs);

        _handle = GCHandle.Alloc(this);

        for (var i = 0; i < _workers.Length; ++i)
        {
            var worker = _workers.GetPointer(i);
            worker->JobSystem = _handle;
            worker->NativeThread = threadManager.Create(&RunWorker, worker, false);
            worker->Active = true;
        }

        foreach (ref var worker in _workers.AsSpan())
        {
            threadManager.Start(worker.NativeThread);
        }
        _memoryManager = memoryManager;

        return true;
    }


    public void Shutdown()
    {
        // Set the active flag to false
        foreach (ref var worker in _workers.AsSpan())
        {
            worker.Active = false;
        }

        // Notify all sleeping workers
        _notifier.Release((int)_workers.Length);

        // Join the threads and destroy them
        foreach (ref var worker in _workers.AsSpan())
        {
            threadManager.Join(worker.NativeThread);
            threadManager.Destroy(ref worker.NativeThread);
        }

        AssertAllAvailable();

        // Cleanup memory usage
        _memoryManager!.FreeArray(ref _workers);
        _memoryManager!.FreeArray(ref _jobs);
    }

    [Conditional("DEBUG")]
    private void AssertAllAvailable()
    {
        //NOTE(Jens): When the job system is done no jobs should be in any other state than available.
        foreach (ref readonly var job in _jobs.AsReadOnlySpan())
        {
            Debug.Assert(job.State == JobStatesConstants.Available, "Found jobs that are not in Available state. This indicates something has not been done properly.");
        }
    }

    public JobHandle Enqueue(in JobDescriptor descriptor)
    {
        var jobSlot = GetNextFreeJobSlot();
        if (jobSlot == -1)
        {
            return JobHandle.Invalid;
        }
        ref var job = ref _jobs[jobSlot];
        job.Version++;
        job.State = JobStatesConstants.Waiting;
        job.Descriptor = descriptor;
        _notifier.Release(1); // add support for multiple jobs at some point

        return new(jobSlot + HandleOffset, job.Version);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsCompleted(in JobHandle handle)
    {
        if (!handle.IsValid)
        {
            return true;
        }
        //NOTE(Jens): Might be more efficient to store the pointer to the job.
        var index = handle.ValueWithoutVersion - HandleOffset;
        Debug.Assert(index < MaxQueuedJobs);
        var job = _jobs.GetPointer(index);
        if (job->Version != handle.Version)
        {
            //NOTE(Jens): Return true if it's the wrong version. Just assume it's completed.
            return true;
        }

        return job->State == JobStatesConstants.Completed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset(ref JobHandle handle)
    {
        if (!handle.IsValid)
        {
            return;
        }
        var index = handle.ValueWithoutVersion - HandleOffset;
        Debug.Assert(index < MaxQueuedJobs);
        var job = _jobs.GetPointer(index);
        if (job->Version == handle.Version && job->State == JobStatesConstants.Completed)
        {
            job->State = JobStatesConstants.Available;
        }
        handle = JobHandle.Invalid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JobStates GetState(in JobHandle handle)
    {
        Debug.Assert(handle.IsValid);
        var index = handle.ValueWithoutVersion - HandleOffset;
        Debug.Assert(index < MaxQueuedJobs);
        var job = _jobs.GetPointer(index);
        return (JobStates)job->State;
    }

    [UnmanagedCallersOnly]
    private static int RunWorker(void* context)
    {
        var worker = (Worker*)context;
        Debug.Assert(worker->JobSystem.IsAllocated && worker->JobSystem.Target != null);
        var jobSystem = (JobSystem)worker->JobSystem.Target!;
        var notifier = jobSystem._notifier;
        ref var active = ref worker->Active;

        while (active)
        {
            if (!notifier.Wait(DefaultWaitTime))
            {
                continue;
            }

            if (!active)
            {
                //NOTE(Jens): We're not handling shutdown when there are jobs still queued. We need to see if that can ever be a problem.
                //NOTE(Jens): Thread pool shutdown should be one of the last things that happens, so it should not occur. Maybe add an assert?        

                break;
            }

            var job = jobSystem.GetNextJob();
            if (job != null)
            {
#if DEBUG
                try
                {
                    job->Descriptor.Callback(job->Descriptor.Context);
                }
                catch (Exception e)
                {
                    Logger.Error<JobSystem>($"{e.GetType().Name} was thrown in a Job. This will crash the process in release builds. Exceptions have to be handled by the function that is queued.");
                    Logger.Error<JobSystem>(e.Message);
                    Logger.Error<JobSystem>(e.StackTrace!);
                }

#else
                job->Descriptor.Callback(job->Descriptor.Context);
#endif
                job->State = job->Descriptor.AutoReset ? JobStatesConstants.Available : JobStatesConstants.Completed;
            }
        }

        Debug.Assert(jobSystem._nextJob == jobSystem._nextJobSlot, "There are jobs in the queue, this is not expected. Figure out why this is happening.");
        return 0;
    }

    private Job* GetNextJob()
    {
        var maxIterations = MaxQueuedJobs;
        var jobQueueMask = JobQueueMask;
        while (maxIterations-- > 0)
        {
            var current = _nextJob;
            var index = Interlocked.CompareExchange(ref _nextJob, (current + 1) & jobQueueMask, current);
            if (index != current)
            {
                //Logger.Trace<JobSystem>($"Conflict!");
                continue;
            }

            var job = _jobs.GetPointer(index);
            var previousState = Interlocked.CompareExchange(ref job->State, JobStatesConstants.Executing, JobStatesConstants.Waiting);

            if (previousState != JobStatesConstants.Waiting)
            {
                continue;
            }
            return job;
        }
        Logger.Error<JobSystem>($"Failed to get a job after {MaxQueuedJobs} iterations.");
        return null;
    }

    private int GetNextFreeJobSlot()
    {
        var maxIterations = MaxQueuedJobs;
        var jobQueueMask = JobQueueMask;
        while (maxIterations-- > 0)
        {
            var current = _nextJobSlot;
            var index = Interlocked.CompareExchange(ref _nextJobSlot, (current + 1) & jobQueueMask, current);
            if (index != current)
            {
                //Logger.Trace<JobSystem>($"Conflict!");
                continue;
            }

            var job = _jobs.GetPointer(index);
            var previousState = Interlocked.CompareExchange(ref job->State, JobStatesConstants.Claimed, JobStatesConstants.Available);

            if (previousState != JobStatesConstants.Available)
            {
                //Logger.Trace<JobSystem>("Job is in wrong state.");
                continue;
            }
            return (int)index;
        }
        Logger.Error<JobSystem>($"Failed to get a job after {MaxQueuedJobs} iterations.");
        return -1;
    }

    private struct Job
    {
        public int State;
        public byte Version;
        public JobDescriptor Descriptor;
    }

    private struct Worker
    {
        public NativeThreadHandle NativeThread;
        public GCHandle JobSystem;
        public bool Active;
    }
}
