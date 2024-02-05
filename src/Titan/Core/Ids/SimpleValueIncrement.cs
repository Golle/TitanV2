using System.Numerics;

namespace Titan.Core.Ids;

public readonly struct SimpleValueIncrement<T> : IIdIncrementer<T> where T : unmanaged, INumber<T>
{
    private static readonly object _lock = new();
    public static T CalculateNext(ref T value)
    {
        lock (_lock)
        {
            value++;
            return value;
        }
    }
}
