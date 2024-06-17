using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Titan.Generators.Systems;

[Generator]
internal class SystemsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var systemsValueProvider = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                TitanTypes.SystemAttribute,
                static (node, _) => node.IsMethod() && node.Parent!.IsPartial(),
                static (syntaxContext, _) => Helpers.ReadSystemType(syntaxContext));

        var componentsValueProvider = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                TitanTypes.ComponentAttribute,
                static (node, _) => node.IsStruct() && node.IsPartial(),
                static (syntaxContext, _) => Helpers.ReadComponentType(syntaxContext));


        var compiledComponents = context
            .CompilationProvider
            .Select(static (compilation, _) => Helpers.ReadCompiledComponentTypes(compilation));

        // Combine the results into a single tuple
        var result = compiledComponents
            .Combine(componentsValueProvider.Collect())
            .Combine(systemsValueProvider.Collect())
            .Select((tuple, _) => (CompiledComponents: tuple.Left.Left, Components: tuple.Left.Right, Systems: tuple.Right))
            ;

        context.RegisterSourceOutput(result, static (productionContext, tuple) =>
        {
            // Create a dictionary from the components for easy lookup of ID.
            var ids = tuple
                .CompiledComponents
                .ToDictionary(static valueTuple => valueTuple.Name, static valueTuple => valueTuple.Id);

            // We use ID 1 for Entities, this will not affect the signature calculation
            ids.Add(TitanTypes.Entity, 1);

            // Add the components sources and record their IDs
            ConstructComponents(ids, tuple.Components, productionContext);

            Dictionary<INamedTypeSymbol, List<(string Name, int Stage, int ExecutionType)>> systems = new(tuple.Systems.Length, SymbolEqualityComparer.Default);

            var builder = new FormattedBuilder(new StringBuilder());
            foreach (var system in tuple.Systems)
            {
                if (!system.Method.ReturnsVoid)
                {
                    productionContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("TI0002", "Wrong return type", "The return type must be void for a System.", "Systems", DiagnosticSeverity.Error, true), system.Node.GetLocation(), system.Method.Locations));
                    continue;
                }

                builder.Reset();
                var name = $"{system.Type.Name}_{system.Method.Name}_NEXT";
                SystemsBuilder.Build(builder, system, name, ids);
                productionContext.AddSource($"{name}.g.cs", builder.ToString());

                if (!systems.TryGetValue(system.Type, out var list))
                {
                    systems[system.Type] = list = new();
                }
                list.Add((name, system.Stage, system.ExecutionType));
            }

            foreach (var system in systems)
            {
                WriteSystemDescriptors(system.Key, system.Value, productionContext);
            }
        });
    }



    private static void ConstructComponents(Dictionary<string, ulong> ids, ImmutableArray<ComponentType> components, SourceProductionContext context)
    {
        // Get the higest ID, so we can continue to increase it if we have new components.
        var id = ids.Count > 0 ? ids.Values.Max() : 0;

        foreach (var component in components)
        {
            var nextId = PrimeNumberIncrement.CalculateNext(ref id);
            // Add the new component the Dictionary
            ids.Add(component.FullName, nextId);
            var source = ComponentBuilder.Build(component, nextId);
            context.AddSource($"{component.Name}.g.cs", source);
        }
    }

    private static void WriteSystemDescriptors(ITypeSymbol type, IReadOnlyList<(string Name, int Stage, int ExecutionType)> systems, SourceProductionContext context)
    {
        var builder = new FormattedBuilder(new StringBuilder());
        var containingNamespace = type.ContainingNamespace.ToDisplayString();
        var typeName = type.Name;

        var classOrStruct = type.IsValueType ? "struct" : "class";

        var modifier = type.DeclaredAccessibility.AsString();
        builder.AppendAutoGenerated()
            .AppendLine()
            .AppendLine($"namespace {containingNamespace};")
            .AppendLine()
            .AppendLine($"{modifier} unsafe partial {classOrStruct} {typeName} : {TitanTypes.ISystem}")
            .AppendOpenBracer();

        // Add GetJobs
        {
            builder
                .AppendLine($"public static int GetSystems({TitanTypes.Span}<{TitanTypes.SystemDescriptor}> descriptors)")
                .AppendOpenBracer()
                .AppendLine($"{TitanTypes.Debug}.Assert(descriptors.Length >= {systems.Count});")
                ;

            for (var i = 0; i < systems.Count; ++i)
            {
                var (systemName, stage, executionType) = systems[i];
                builder
                    .AppendLine($"descriptors[{i}].Stage = ({TitanTypes.SystemStage}){stage};")
                    .AppendLine($"descriptors[{i}].ExecutionType = ({TitanTypes.SystemExecutionType}){executionType};")
                    .AppendLine($"descriptors[{i}].Name = {TitanTypes.StringRef}.Create(\"{systemName}\");")
                    .AppendLine($"descriptors[{i}].Init = &{systemName}.Init;")
                    .AppendLine($"descriptors[{i}].Execute = &{systemName}.Execute;")
                    .AppendLine($"descriptors[{i}].GetQuery = &{systemName}.GetQuery;")
                    .AppendLine()
                    ;
            }

            builder
                .AppendLine($"return {systems.Count};")
                .AppendCloseBracer()
                ;
        }

        builder
            .AppendCloseBracer();

        //NOTE(Jens): This will crash if this is a resource without a namespace. Fix if it ever occurs.
        context.AddSource($"{containingNamespace}.{typeName}.g.cs", builder.ToString());
    }
}
