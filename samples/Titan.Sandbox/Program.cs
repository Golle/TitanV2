using System.Runtime.InteropServices;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Platform;
using Titan.Core.Threading;
using Titan.Core.Threading.Platform;

using var _ = Logger.Start<ConsoleLogger>(10_000);

Logger.Error<Program>($"Hello, World! From {typeof(Program).Assembly.GetName().Name}");

unsafe
{
    var threadManager = new ThreadManager<Win32NativeThreadApi>();

    var memorySystem = new MemorySystem<Win32PlatformAllocator>();
    memorySystem.Init(new MemoryConfig(MemoryUtils.GigaBytes(1), MemoryUtils.MegaBytes(128)));

    var jobSystem = new JobSystem(threadManager);
    jobSystem.Init(new JobSystemConfig((uint)(Environment.ProcessorCount - 1u), 1024), memorySystem);

    var a = new TestStr
    {
        A = 100
    };

    var descriptor = JobDescriptor.CreateTyped(&Test.TheFunc, &a, false);
    for (var i = 0; i < 20; ++i)
    {
        var handle = jobSystem.Enqueue(descriptor);
        jobSystem.GetState(handle);
        Thread.Sleep(10);
    }
    Thread.Sleep(1000);
    jobSystem.Shutdown();
}

Thread.Sleep(1000);

public unsafe class Test
{
    public static void TheFunc(TestStr* parameter)
    {
        Logger.Info<Test>($"Hello from win32 thread. TestStr = {parameter->A}");
    }

}

public struct TestStr
{
    public int A;
}