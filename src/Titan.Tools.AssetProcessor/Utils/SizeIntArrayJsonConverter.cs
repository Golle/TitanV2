using System.Text.Json;
using System.Text.Json.Serialization;
using Titan.Core.Maths;

namespace Titan.Tools.AssetProcessor.Utils;

internal sealed class SizeIntArrayJsonConverter : JsonConverter<Size>
{
    public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        var width = reader.GetInt32();
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException($"Number expected, got {reader.TokenType}");
        }

        var height = reader.GetInt32();
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException($"End of Array expected, got {reader.TokenType}");
        }

        return new Size(width, height);
    }

    public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Width);
        writer.WriteNumberValue(value.Height);
        writer.WriteEndArray();
    }
}