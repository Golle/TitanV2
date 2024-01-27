namespace Titan.Core.Threading;

public interface IJobSystem : IService
{
    JobHandle Enqueue(in JobDescriptor descriptor);
    bool IsCompleted(in JobHandle handle);
    JobStates GetState(in JobHandle handle);
    void Reset(ref JobHandle handle);
}