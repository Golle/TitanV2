using Titan.Core.Ids;

namespace Titan.Resources;

public struct UnmanagedResourceId
{
    public static uint GetNext() => IdGenerator<UnmanagedResourceId, uint, SimpleValueIncrement<uint>>.GetNext();
}
