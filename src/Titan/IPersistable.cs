using System.Text.Json.Serialization.Metadata;

namespace Titan;

public interface IPersistable<T> where T : IConfiguration
{
    static abstract JsonTypeInfo<T> TypeInfo { get; }
    static abstract string Filename { get; }
}
