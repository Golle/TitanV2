using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Titan.Serialization.Json;

public sealed class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        var x = (float)reader.GetDouble();
        reader.Read();
        var y = (float)reader.GetDouble();
        reader.Read();
        var z = (float)reader.GetDouble();
        reader.Read();
        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}
