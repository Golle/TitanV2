using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Titan.Generators.Events;

[Generator]
public class EventsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            TitanTypes.EventAttribute,
            static (node, _) => node.IsPartial() && (node.IsStruct() || node.IsRecordStruct()),
            static (syntaxContext, _) => new EventsType((INamedTypeSymbol)syntaxContext.TargetSymbol));

        var valueProvider = context
            .CompilationProvider
            .Combine(structDeclarations.Collect());

        context
            .RegisterSourceOutput(valueProvider, static (productionContext, source) => Execute(source.Left, source.Right!, productionContext));

        static void Execute(Compilation _, ImmutableArray<EventsType> events, SourceProductionContext context)
        {
            foreach (var eventsType in events)
            {
                var result = EventBuilder.Build(eventsType);
                context.AddSource($"{eventsType.Symbol.Name}.g.cs", result);
            }
        }
    }
}
