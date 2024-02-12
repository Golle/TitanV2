using Microsoft.CodeAnalysis;

namespace Titan.Generators.Events;
internal struct EventsType(INamedTypeSymbol symbol)
{
    public INamedTypeSymbol Symbol = symbol;
}
