namespace Titan.Core.Threading;

public enum JobStates : byte
{
    Available = JobStatesConstants.Available,
    Claimed = JobStatesConstants.Claimed,
    Waiting = JobStatesConstants.Waiting,
    Executing = JobStatesConstants.Executing,
    Completed = JobStatesConstants.Completed,
    Error = JobStatesConstants.Error
}

internal static class JobStatesConstants
{
    public const byte Available = 0;
    public const byte Claimed = 1;
    public const byte Waiting = 2;
    public const byte Executing = 3;
    public const byte Completed = 4;
    public const byte Error = 5;
}