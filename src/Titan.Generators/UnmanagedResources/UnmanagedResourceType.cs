using Microsoft.CodeAnalysis;

namespace Titan.Generators.UnmanagedResources;

internal readonly struct UnmanagedResourceType(INamedTypeSymbol symbol)
{
    public readonly INamedTypeSymbol Symbol = symbol;
}
