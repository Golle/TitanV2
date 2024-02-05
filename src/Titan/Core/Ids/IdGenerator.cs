using System.Numerics;

namespace Titan.Core.Ids;
public static class IdGenerator<T, TValueType, TIncrementer>
    where TValueType : unmanaged, INumber<TValueType>
    where TIncrementer : IIdIncrementer<TValueType>
{
    private static TValueType _next;
    public static TValueType GetNext()
        => TIncrementer.CalculateNext(ref _next);
}
