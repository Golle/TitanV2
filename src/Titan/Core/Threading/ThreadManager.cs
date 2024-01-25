namespace Titan.Core.Threading;

internal unsafe class ThreadManager<T> : IThreadManager where T : INativeThreadApi
{
    public NativeThreadHandle Create(delegate* unmanaged<void*, int> callback, void* parameters, bool startImmediately) => T.Create(callback, parameters, startImmediately);
    public bool Start(in NativeThreadHandle handle) => T.Start(handle);
    public bool Join(in NativeThreadHandle handle) => T.Join(handle);
    public void Destroy(ref NativeThreadHandle handle) => T.Destroy(ref handle);
    public void Sleep(TimeSpan time) => T.Sleep(time);
    public uint GetCurrentThreadId() => T.GetCurrentThreadId();
}