namespace Titan.Core.Threading;

public readonly unsafe struct JobDescriptor
{
    internal readonly delegate*<void*, void> Callback;
    internal readonly void* Context;
    internal readonly bool AutoReset;
    private JobDescriptor(delegate*<void*, void> callback, void* context, bool autoReset = true)
    {
        AutoReset = autoReset;
        Callback = callback;
        Context = context;
    }

    public static JobDescriptor Create(delegate*<void*, void> callback, void* context, bool autoReset = true)
        => new(callback, context, autoReset);
    public static JobDescriptor CreateTyped<T>(delegate*<T*, void> callback, T* context, bool autoReset = true) where T : unmanaged
        => Create((delegate*<void*, void>)callback, context, autoReset);
}