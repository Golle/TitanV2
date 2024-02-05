using System.Text;

namespace Titan.Generators.UnmanagedResources;

internal static class UnmanagedResourceBuilder
{
    public static string Build(in UnmanagedResourceType resource)
    {
        var symbol = resource.Symbol;
        var @namespace = symbol.ContainingNamespace;

        var modifier = symbol.DeclaredAccessibility.AsString();
        return new FormattedBuilder(new StringBuilder())
            .AppendLine("// Auto-Generated")
            .AppendLine()
            .AppendLine($"namespace {@namespace};")
            .AppendLine($"{modifier} partial struct {symbol.Name} : {TitanTypes.IResource}")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"public static uint Id {{ get; }} = {TitanTypes.UnmanagedResourceGenerator}.GetNext();")
            .EndIndentation()
            .AppendLine("}")
            .ToString();
    }
}
