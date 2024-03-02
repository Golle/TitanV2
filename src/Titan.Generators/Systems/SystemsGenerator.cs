using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Titan.Generators.Systems;
[Generator]
public class SystemsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            TitanTypes.SystemAttribute,
            static (node, _) => node is MethodDeclarationSyntax syntax && IsJobMethod(syntax),
            static (syntaxContext, _) =>
            {
                var constructorArguments = syntaxContext
                    .Attributes
                    .First(static data => TitanTypes.SystemAttribute.EndsWith(data.AttributeClass!.MetadataName))
                    .ConstructorArguments;

                var systemStage = (int)constructorArguments[0].Value!;
                var executionType = (int)constructorArguments[1].Value!;


                var method = (IMethodSymbol)syntaxContext.TargetSymbol;
                var type = method.ContainingType;
                return new SystemType(type, method, systemStage, executionType, syntaxContext.TargetNode);
            });

        var valueProvider = context
            .CompilationProvider
            .Combine(structDeclarations.Collect());

        context
            .RegisterSourceOutput(valueProvider, static (productionContext, source) => Execute(source.Left, source.Right!, productionContext));
        return;

        static bool IsJobMethod(MethodDeclarationSyntax syntax)
        {
            var parent = syntax.Parent;
            if (parent is StructDeclarationSyntax structDecl)
            {
                return structDecl.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword));
            }

            if (parent is ClassDeclarationSyntax classDecl)
            {
                return classDecl.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword));
            }

            return false;
        }

        static void Execute(Compilation _, ImmutableArray<SystemType> systemTypes, SourceProductionContext context)
        {
            Dictionary<INamedTypeSymbol, List<(string Name, int Stage, int ExecutionType)>> systems = new(systemTypes.Length, SymbolEqualityComparer.Default);
            foreach (var system in systemTypes)
            {
                if (!system.Method.ReturnsVoid)
                {
                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("TI0002", "Wrong return type", "The return type must be void for a System.", "Systems", DiagnosticSeverity.Error, true), system.Node.GetLocation(), system.Method.Locations));
                    continue;
                }

                var name = SystemsBuilder.Build(system, context);
                var containingType = system.Type;
                if (!systems.TryGetValue(containingType, out var list))
                {
                    systems[containingType] = list = new();
                }
                list.Add((name, system.Stage, system.ExecutionType));
            }


            foreach (var system in systems)
            {
                CreateSystem(system.Key, system.Value, context);
            }
        }
    }

    private static void CreateSystem(ITypeSymbol type, IReadOnlyList<(string Name, int Stage, int ExecutionType)> systems, SourceProductionContext context)
    {
        var builder = new FormattedBuilder(new StringBuilder());
        var containingNamespace = type.ContainingNamespace.ToDisplayString();
        var typeName = type.Name;

        var classOrStruct = type.IsValueType ? "struct" : "class";

        var modifier = type.DeclaredAccessibility.AsString();
        builder.AppendLine("// Auto-Generated")
            .AppendLine($"namespace {containingNamespace};")
            .AppendLine($"{modifier} unsafe partial {classOrStruct} {typeName} : {TitanTypes.ISystem}")
            .AppendLine("{")
            .BeginIndentation();

        // Add GetJobs
        {
            builder.AppendLine($"public static int GetSystems(System.Span<{TitanTypes.SystemDescriptor}> descriptors)")
                .AppendLine("{")
                .BeginIndentation()
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
                    .AppendLine($"descriptors[{i}].Execute = &{systemName}.Execute;");
            }

            builder
                .AppendLine($"return {systems.Count};")
                .EndIndentation()
                .AppendLine("}");
        }

        builder
            .EndIndentation()
            .AppendLine("}");

        //NOTE(Jens): This will crash if this is a resource without a namespace. Fix if it ever occurs.
        context.AddSource($"{containingNamespace}.{typeName}.g.cs", builder.ToString());
    }
}
