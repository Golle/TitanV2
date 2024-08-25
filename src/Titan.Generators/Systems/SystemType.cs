using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Titan.Generators.Systems;

internal readonly struct SystemType(INamedTypeSymbol type, IMethodSymbol method, int stage, int executionType, int order, SyntaxNode node, ImmutableArray<SystemParameter> parameters)
{
    //NOTE(Jens): These might not be cached, maybe extract all info we want.
    public readonly INamedTypeSymbol Type = type;
    public readonly IMethodSymbol Method = method;
    public readonly int Stage = stage;
    public readonly int ExecutionType = executionType;
    public readonly int Order = order;
    public readonly SyntaxNode Node = node;

    public readonly ImmutableArray<SystemParameter> Parameters = parameters;
}

internal readonly struct SystemParameter(string type, ArgumentKind kind, ModifierType modifier)
{
    public readonly string Type = type;
    public readonly ArgumentKind Kind = kind;
    public readonly ModifierType Modifier = modifier;
}
