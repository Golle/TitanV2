namespace Titan;

public interface IDefault<out T> where T : IConfiguration
{
    static abstract T Default { get; }
}