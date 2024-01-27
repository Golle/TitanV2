using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Modules;

internal class MemoryModule<TPlatformAllocator> : IModule where TPlatformAllocator : IPlatformAllocator
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        var system = new MemorySystem<TPlatformAllocator>();
        if (!system.Init(config.Memory))
        {
            Logger.Error<MemoryModule<TPlatformAllocator>>($"Failed to init the {nameof(IMemorySystem)}");
            return false;
        }

        builder.AddService<IMemorySystem, MemorySystem<TPlatformAllocator>>(system);
        return true;
    }

    public static bool Init(IApp app)
        => true;

    public static bool Shutdown(IApp app)
    {
        var memorySystem = (MemorySystem<TPlatformAllocator>)app.GetService<IMemorySystem>();
        memorySystem.Shutdown();

        return true;
    }
}