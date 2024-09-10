#define WIN32_THREAD_TRACE

using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Platform.Win32;

namespace Titan.Core.Threading.Platform;
internal readonly unsafe struct Win32NativeThreadApi : INativeThreadApi
{
    [Conditional("WIN32_THREAD_TRACE")]
    private static void Trace(string message) => Logger.Trace<Win32NativeThreadApi>(message);

    private const uint CREATE_SUSPENDED = 0x00000004;

    public static NativeThreadHandle Create(delegate* unmanaged<void*, int> callback, void* parameter, bool startImmediately = false)
    {
        uint threadId = 0;
        var handle = Kernel32.CreateThread(null, 0, callback, parameter, startImmediately ? 0 : CREATE_SUSPENDED, &threadId);
        if (!handle.IsValid())
        {
            Logger.Error<Win32NativeThreadApi>("Failed to create a thread.");
            return NativeThreadHandle.Invalid;
        }
        Trace($"NativeThread created. ID = {threadId} Handle = {handle.Value} Source Thread ID = {GetCurrentThreadId()}");
        return new(handle, threadId);
    }

    public static bool Start(in NativeThreadHandle handle)
    {
        Debug.Assert(handle.IsValid);
        Trace($"Starting Native Thread. ID = {handle.ThreadId} Handle = {handle.Handle} Source Thread ID = {GetCurrentThreadId()}");

        var result = Kernel32.ResumeThread(handle.Handle);
        if (result == uint.MaxValue)
        {
            Logger.Error<Win32NativeThreadApi>($"Failed to resume Native Thread. Id = {handle.ThreadId} ID = {handle.ThreadId}");
            return false;
        }
        return true;
    }

    public static bool Join(in NativeThreadHandle handle)
    {
        Trace($"Joining Native Thread. Id = {handle.ThreadId} Handle = {handle.Handle} Source Thread ID = {GetCurrentThreadId()}");
        var result = Kernel32.WaitForSingleObject(handle.Handle, 3000); // wait up to 3 seconds. This should never happen in a normal use case.
        if (result != 0)
        {
            Logger.Warning<Win32NativeThreadApi>($"{nameof(Kernel32.WaitForSingleObject)} returned unexpected code: 0x{result:x8}");
            return false;
        }

        return true;
    }

    public static void Destroy(ref NativeThreadHandle handle)
    {
        Trace($"Destroying Native Thread. ID = {handle.ThreadId} Handle = {handle.Handle} Source Thread ID = {GetCurrentThreadId()}");
        if (handle.IsValid)
        {
            Kernel32.CloseHandle(handle.Handle);
            handle = default;
        }
    }

    public static void Sleep(TimeSpan time)
    {
        Trace($"Sleep Native Thread Id = {GetCurrentThreadId()} Time = {time}");
        Kernel32.Sleep((uint)time.TotalMilliseconds);
    }

    public static uint GetCurrentThreadId()
        => Kernel32.GetCurrentThreadId();
}
