using Microsoft.CodeAnalysis;

namespace Titan.Generators.Systems;

public readonly struct SystemType(INamedTypeSymbol type, IMethodSymbol method, int stage, SyntaxNode node)
{
    public readonly INamedTypeSymbol Type = type;
    public readonly IMethodSymbol Method = method;
    public readonly int Stage = stage;
    public readonly SyntaxNode Node = node;
}
