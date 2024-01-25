namespace Titan.Core.Threading;

internal unsafe interface INativeThreadApi
{
    static abstract NativeThreadHandle Create(delegate* unmanaged<void*, int> callback, void* parameter, bool startImmediately = false);
    static abstract bool Start(in NativeThreadHandle handle);
    static abstract bool Join(in NativeThreadHandle handle);
    static abstract void Destroy(ref NativeThreadHandle handle);
    static abstract void Sleep(TimeSpan time);
    static abstract uint GetCurrentThreadId();
}
