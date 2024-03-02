using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Titan.Generators.Inline;

internal static class InlineStructBuilder
{
    private static readonly string InlineMethod = $"[{TitanTypes.MethodImplAttribute}({TitanTypes.MethodImplOptions}.{nameof(MethodImplOptions.AggressiveInlining)})]";

    public static void Build(StringBuilder stringBuilder, INamedTypeSymbol symbol, int length)
    {
        var builder = new FormattedBuilder(stringBuilder);
        var modifier = symbol.DeclaredAccessibility.AsString();
        var @namespace = symbol.ContainingNamespace.ToDisplayString();
        builder.AppendLine($"namespace {@namespace}")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"{modifier} partial struct {symbol.Name}<T> where T : unmanaged")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"private const int Length = {length};")
            .AppendLine("private T _ref;")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public {TitanTypes.Span}<T> AsSpan() => {TitanTypes.MemoryMarshal}.CreateSpan(ref _ref, Length);")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public {TitanTypes.ReadOnlySpan}<T> AsReadOnlySpan() => {TitanTypes.MemoryMarshal}.CreateReadOnlySpan(ref _ref, Length);")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public readonly unsafe T* AsPointer() => (T*){TitanTypes.Unsafe}.AsPointer(ref {TitanTypes.Unsafe}.AsRef(in this));")
            .AppendLine()
            .AppendLine($"public readonly unsafe ref T this[uint index]")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine(InlineMethod)
            .AppendLine("get => ref *(AsPointer() + index);")
            .EndIndentation()
            .AppendLine("}")
            .AppendLine()

            .EndIndentation()
            .AppendLine("}")
            .EndIndentation()
            .AppendLine("}");

    }
}
