namespace Titan.Core.Threading;

public interface IJobSystem
{
    JobHandle Enqueue(in JobDescriptor descriptor);
    bool IsCompleted(in JobHandle handle);
    JobStates GetState(in JobHandle handle);
    void Reset(ref JobHandle handle);
}