using System.Text;

namespace Titan.Generators.Events;

internal static class EventBuilder
{
    public static string Build(in EventsType eventType)
    {
        var symbol = eventType.Symbol;
        var @namespace = symbol.ContainingNamespace;

        var record = symbol.IsRecord ? "record " : string.Empty;
        var modifier = symbol.DeclaredAccessibility.AsString();
        return new FormattedBuilder(new StringBuilder())
            .AppendLine("// Auto-Generated")
            .AppendLine()
            .AppendLine($"namespace {@namespace};")
            .AppendLine($"{modifier} partial {record}struct {symbol.Name} : {TitanTypes.IEvent}")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"public static ushort Id {{ get; }} = {TitanTypes.EventsGenerator}.GetNext();")
            .EndIndentation()
            .AppendLine("}")
            .ToString();
    }
}
