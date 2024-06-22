using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Titan.Generators.Inline;
[Generator]
public sealed class InlineGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            "System.Runtime.CompilerServices.InlineArrayAttribute",
            static (node, _) => node.IsPartial() && node is StructDeclarationSyntax structDecl && structDecl.Identifier.ValueText.StartsWith("Inline"),
            static (syntaxContext, _) =>
            {
                var length = (int)syntaxContext
                    .Attributes[0]
                    .ConstructorArguments[0]
                    .Value!;

                return (Symbol: (INamedTypeSymbol)syntaxContext.TargetSymbol, Length: length);
            });


        context
            .RegisterSourceOutput(structDeclarations.Collect(), static (productionContext, source) => Execute(source, productionContext));

        static void Execute(ImmutableArray<(INamedTypeSymbol Symbol, int Length)> arrays, SourceProductionContext context)
        {
            var builder = new StringBuilder();

            foreach (var array in arrays)
            {
                InlineStructBuilder.Build(builder, array.Symbol, array.Length);
                builder.AppendLine();
            }

            context.AddSource("InlineArrays.g.cs", builder.ToString());
        }
    }
}
