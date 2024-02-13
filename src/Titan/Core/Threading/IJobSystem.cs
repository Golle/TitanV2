namespace Titan.Core.Threading;

public interface IJobSystem : IService
{
    JobHandle Enqueue(in JobDescriptor descriptor);
    /// <summary>
    /// Check if a job is completed
    /// <remarks>Passing an invalid handle to this function will return true.</remarks>
    /// </summary>
    /// <param name="handle">The handle</param>
    /// <returns>True when the job is completed</returns>
    bool IsCompleted(in JobHandle handle);
    JobStates GetState(in JobHandle handle);
    void Reset(ref JobHandle handle);
}
