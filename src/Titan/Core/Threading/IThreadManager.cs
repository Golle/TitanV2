namespace Titan.Core.Threading;

public unsafe interface IThreadManager
{
    NativeThreadHandle Create(delegate* unmanaged<void*, int> callback, void* parameters, bool startImmediately);
    bool Start(in NativeThreadHandle handle);
    bool Join(in NativeThreadHandle handle);
    void Destroy(ref NativeThreadHandle handle);
    void Sleep(TimeSpan time);
    uint GetCurrentThreadId();
}