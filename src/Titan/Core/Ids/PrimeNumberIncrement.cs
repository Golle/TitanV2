namespace Titan.Core.Ids;

public readonly struct PrimeNumberIncrement : IIdIncrementer<ulong>
{
    private static readonly object _lock = new();

    public static ulong CalculateNext(ref ulong value)
    {
        lock (_lock)
        {
            var val = value+1;
            while (!IsPrime(val))
            {
                val++;
            }

            value = val;
            
            return val;
        }
    }

    private static bool IsPrime(ulong value)
    {
        if (value <= 1)
        {
            return false;
        }

        var i = 2ul;
        while (i * i <= value)
        {
            if (value % i == 0)
            {
                return false;
            }
            i++;
        }
        return true;
    }
}
