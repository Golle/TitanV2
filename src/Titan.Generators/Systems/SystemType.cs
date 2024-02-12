using Microsoft.CodeAnalysis;

namespace Titan.Generators.Systems;

public readonly struct SystemType(INamedTypeSymbol type, IMethodSymbol method, int stage, int executionType, SyntaxNode node)
{
    public readonly INamedTypeSymbol Type = type;
    public readonly IMethodSymbol Method = method;
    public readonly int Stage = stage;
    public readonly int ExecutionType  = executionType;
    public readonly SyntaxNode Node = node;
}
