using System.Numerics;

namespace Titan.Core.Ids;

public interface IIdIncrementer<T> where T : unmanaged, INumber<T>
{
    static abstract T CalculateNext(ref T value);
}
