using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Threading;

namespace Titan.Modules;

internal class ThreadingModule<TNativeThreadType> : IModule where TNativeThreadType : INativeThreadApi
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        var memorySystem = builder.GetService<IMemoryManager>();

        var manager = new ThreadManager<TNativeThreadType>();
        builder.AddService<IThreadManager>(manager);
        var jobSystem = new JobSystem(manager);

        if (!jobSystem.Init(config.JobSystem, memorySystem))
        {
            Logger.Error<ThreadingModule<TNativeThreadType>>($"Failed to init the {nameof(IJobSystem)}.");
            return false;
        }

        builder.AddService<IJobSystem, JobSystem>(jobSystem);
        return true;
    }
}
