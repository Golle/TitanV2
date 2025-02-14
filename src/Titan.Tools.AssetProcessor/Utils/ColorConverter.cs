using System.Text.Json;
using System.Text.Json.Serialization;
using Titan.Core.Maths;

namespace Titan.Tools.AssetProcessor.Utils;

public sealed class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Start of Array expected, got {reader.TokenType}");
        }
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Number expected, got {reader.TokenType}");
        }
        var red = reader.GetByte();
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Number expected, got {reader.TokenType}");
        }
        var green = reader.GetByte();
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Number expected, got {reader.TokenType}");
        }
        var blue = reader.GetByte();
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Number expected, got {reader.TokenType}");
        }
        var alpha = reader.GetByte();
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException($"End of Array expected, got {reader.TokenType}");
        }

        return new(red, green, blue, alpha);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(Math.Clamp(value.R * byte.MaxValue, 0, byte.MaxValue));
        writer.WriteNumberValue(Math.Clamp(value.G * byte.MaxValue, 0, byte.MaxValue));
        writer.WriteNumberValue(Math.Clamp(value.B * byte.MaxValue, 0, byte.MaxValue));
        writer.WriteNumberValue(Math.Clamp(value.A * byte.MaxValue, 0, byte.MaxValue));
        writer.WriteEndArray();
    }
}
