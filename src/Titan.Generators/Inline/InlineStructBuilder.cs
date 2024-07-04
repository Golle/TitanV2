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
            .AppendLine($"{modifier} unsafe partial struct {symbol.Name}<T> where T : unmanaged")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"public int Size => Length;")
            .AppendLine($"private const int Length = {length};")
            .AppendLine("private T _ref;")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public {TitanTypes.Span}<T> AsSpan() => {TitanTypes.MemoryMarshal}.CreateSpan(ref _ref, Length);")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public readonly {TitanTypes.ReadOnlySpan}<T> AsReadOnlySpan() => {TitanTypes.MemoryMarshal}.CreateReadOnlySpan(ref {TitanTypes.Unsafe}.AsRef(in _ref), Length);")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public readonly T* AsPointer() => (T*){TitanTypes.Unsafe}.AsPointer(ref {TitanTypes.Unsafe}.AsRef(in this));")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public readonly {TitanTypes.TitanList}<T> AsList() => new((T*){TitanTypes.Unsafe}.AsPointer(ref {TitanTypes.Unsafe}.AsRef(in this)), {length});")
            .AppendLine()
            .AppendLine(InlineMethod)
            .AppendLine($"public readonly T* GetPointer(int index) => (T*){TitanTypes.Unsafe}.AsPointer(ref {TitanTypes.Unsafe}.AsRef(in this)) + index;")
            .AppendLine()
            .AppendLine($"public readonly ref T this[uint index]")
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
