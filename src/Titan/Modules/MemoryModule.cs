using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Modules;

internal class MemoryModule<TPlatformAllocator> : IModule where TPlatformAllocator : IPlatformAllocator
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        var system = new MemoryManager<TPlatformAllocator>();
        if (!system.Init(config.Memory))
        {
            Logger.Error<MemoryModule<TPlatformAllocator>>($"Failed to init the {nameof(IMemoryManager)}");
            return false;
        }

        builder.AddService<IMemoryManager, MemoryManager<TPlatformAllocator>>(system);
        return true;
    }
}
