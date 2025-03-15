using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Titan.Generators.Systems;
internal static class Helpers
{
    public static SystemType ReadSystemType(GeneratorAttributeSyntaxContext context)
    {
        var constructorArguments = context
            .Attributes
            .First(static data => TitanTypes.SystemAttribute.EndsWith(data.AttributeClass!.MetadataName))
            .ConstructorArguments;

        var systemStage = (int)constructorArguments[0].Value!;
        var executionType = (int)constructorArguments[1].Value!;
        var order = (int)constructorArguments[2].Value!;

        var method = (IMethodSymbol)context.TargetSymbol;
        var type = method.ContainingType;


        var parameters = ReadParameters(method);
        return new SystemType(type, method, systemStage, executionType, order, context.TargetNode, parameters);
    }

    public static ComponentType ReadComponentType(GeneratorAttributeSyntaxContext syntaxContext)
    {
        var attribute = syntaxContext.Attributes.First(a => a.AttributeClass?.ToDisplayString() == TitanTypes.ComponentAttribute);
        var isTag = (bool)attribute.ConstructorArguments[0].Value!;

        var type = (INamedTypeSymbol)syntaxContext.TargetSymbol;

        return new ComponentType(type.Name, type.ToDisplayString(), type.ContainingNamespace.ToDisplayString(), type.DeclaredAccessibility.AsString(), isTag);
    }

    public static ImmutableArray<(string Name, ulong Id)> ReadCompiledComponentTypes(Compilation compilation)
    {
        var iComponent = compilation.GetTypeByMetadataName(TitanTypes.IComponent);
        var builder = ImmutableArray.CreateBuilder<(string Name, ulong Id)>();
        GetCompiledComponents(compilation.GlobalNamespace, iComponent!, builder);
        return builder.ToImmutable();

        static void GetCompiledComponents(INamespaceSymbol namespaceSymbol, INamedTypeSymbol interfaceType, ImmutableArray<(string Name, ulong Id)>.Builder builder)
        {
            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (member is INamespaceSymbol nestedNamespace)
                {
                    GetCompiledComponents(nestedNamespace, interfaceType, builder);
                }
                else if (member is INamedTypeSymbol typeSymbol)
                {
                    if (typeSymbol.AllInterfaces.Contains(interfaceType))
                    {
                        if (typeSymbol.GetMembers(ComponentBuilder.IdFieldName).FirstOrDefault() is IFieldSymbol { ConstantValue: uint value })
                        {
                            builder.Add((typeSymbol.ToDisplayString(), value));
                        }
                    }
                    foreach (var nestedType in typeSymbol.GetTypeMembers().OfType<INamespaceSymbol>())
                    {
                        GetCompiledComponents(nestedType, interfaceType, builder);
                    }
                }
            }
        }
    }


    public static ImmutableArray<SystemParameter> ReadParameters(IMethodSymbol method)
    {
        var parameters = method.Parameters;
        var builder = ImmutableArray.CreateBuilder<SystemParameter>(parameters.Length);

        foreach (var parameterSymbol in parameters)
        {
            var type = parameterSymbol.Type;
            var typeName = type is IPointerTypeSymbol pointer
                ? pointer.PointedAtType.ToDisplayString()
                : type.ToDisplayString();

            var isSpan = typeName.StartsWith(TitanTypes.Span);
            var isReadOnlySpan = typeName.StartsWith(TitanTypes.ReadOnlySpan);
            if (isSpan || isReadOnlySpan)
            {
                ArgumentKind kind;

                // component or entity
                var arg = ((INamedTypeSymbol)type).TypeArguments[0];
                if (arg.ToDisplayString() == TitanTypes.Entity)
                {
                    kind = ArgumentKind.EntityCollection;
                }
                else
                {
                    var isComponent = arg.GetAttributes().Any(static a => a.AttributeClass?.ToDisplayString() == TitanTypes.ComponentAttribute);
                    if (!isComponent)
                    {
                        throw new InvalidOperationException($"The type is not a component. {arg.Name}");
                    }

                    kind = isSpan
                        ? ArgumentKind.MutableComponent
                        : ArgumentKind.ReadOnlyComponent;
                }

                builder.Add(new SystemParameter(arg.ToDisplayString(), kind, ModifierType.Value));
            }
            else
            {
                var modifier = parameterSymbol.RefKind switch
                {
                    RefKind.In or RefKind.RefReadOnly => ModifierType.In,
                    RefKind.Ref => ModifierType.Ref,
                    _ when type is IPointerTypeSymbol => ModifierType.Pointer,
                    _ => ModifierType.Value
                };

                if (typeName is TitanTypes.EntityManager)
                {
                    builder.Add(new SystemParameter(TitanTypes.EntityManager, ArgumentKind.EntityManager, ModifierType.Value));
                }
                else if (typeName is TitanTypes.AssetsManager)
                {
                    builder.Add(new SystemParameter(TitanTypes.AssetsManager, ArgumentKind.AssetsManager, ModifierType.Value));
                }
                else if (typeName is TitanTypes.EventWriter)
                {
                    builder.Add(new SystemParameter(TitanTypes.EventWriter, ArgumentKind.EventWriter, ModifierType.Value));
                }
                else if (typeName is TitanTypes.AudioManager)
                {
                    builder.Add(new SystemParameter(TitanTypes.AudioManager, ArgumentKind.AudioManager, ModifierType.Value));
                }
                else if (typeName is TitanTypes.UIManager)
                {
                    builder.Add(new SystemParameter(TitanTypes.UIManager, ArgumentKind.UIManager, ModifierType.Value));
                }
                else if (typeName is TitanTypes.MaterialsManager)
                {
                    builder.Add(new SystemParameter(TitanTypes.MaterialsManager, ArgumentKind.MaterialsManager, ModifierType.Value));
                }
                else if (typeName is TitanTypes.MeshManager)
                {
                    builder.Add(new SystemParameter(TitanTypes.MeshManager, ArgumentKind.MeshManager, ModifierType.Value));
                }
                else if (typeName.StartsWith(TitanTypes.EventReader))
                {
                    var eventType = ((INamedTypeSymbol)type).TypeArguments[0].ToDisplayString();
                    builder.Add(new SystemParameter(eventType, ArgumentKind.EventReader, ModifierType.Value));
                }
                else if (typeName.StartsWith(TitanTypes.ReadOnlyStorage))
                {
                    var componentType = ((INamedTypeSymbol)type).TypeArguments[0].ToDisplayString();
                    builder.Add(new SystemParameter(componentType, ArgumentKind.ReadOnlyStorage, ModifierType.Value));
                }
                else if (typeName.StartsWith(TitanTypes.MutableStorage))
                {
                    var componentType = ((INamedTypeSymbol)type).TypeArguments[0].ToDisplayString();
                    builder.Add(new SystemParameter(componentType, ArgumentKind.MutableStorage, ModifierType.Value));
                }

                else
                {
                    var kind = type.IsUnmanagedType
                        ? ArgumentKind.Unmanaged
                        : ArgumentKind.Managed;

                    builder.Add(new SystemParameter(typeName, kind, modifier));
                }
            }
        }

        return builder.MoveToImmutable();
    }
}
