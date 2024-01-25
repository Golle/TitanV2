namespace Titan.Core.Threading;

public readonly struct NativeThreadHandle(nuint handle, uint threadId)
{
    public readonly nuint Handle = handle;
    public readonly uint ThreadId = threadId;

    public bool IsValid => Handle != 0 && ThreadId != 0;

    public static readonly NativeThreadHandle Invalid = default;

}