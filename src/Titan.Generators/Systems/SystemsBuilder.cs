using System.Text;
using Microsoft.CodeAnalysis;

namespace Titan.Generators.Systems;

internal enum ModifierType
{
    In,
    Ref,
    Value,
    Pointer
}

internal enum ArgumentKind
{
    Unmanaged,
    Managed,
    EventWriter,
    EventReader,
    EntityManager
}

internal readonly struct ParameterInfo(ModifierType modifier, string type, ArgumentKind argumentKind)
{
    public readonly ModifierType Modifier = modifier;
    public readonly string Type = type;

    public readonly ArgumentKind ArgumentKind = argumentKind;
}
internal static class SystemsBuilder
{
    private const int MaxArguments = 20;
    public static string Build(in SystemType systemType, SourceProductionContext context)
    {
        var builder = new FormattedBuilder(new StringBuilder());
        builder
            .AppendLine("// Auto-Generated")
            .AppendLine();

        AppendNamespace(systemType.Type, builder);

        var modifier = systemType.Type.DeclaredAccessibility.AsString();
        var name = systemType.Type.Name;
        var method = systemType.Method.Name;
        //NOTE(Jens): 2 methods called the same will cause a conflict. Do we care?
        var systemTypeName = $"{name}_{method}";

        builder.AppendLine($"{modifier} unsafe struct {systemTypeName}")
            .AppendLine("{")
            .BeginIndentation();

        //Debugger.Launch();
        // Create members
        var parameters = systemType
            .Method
            .Parameters
            .Select(static p =>
            {
                var displayString = p.Type.ToDisplayString();
                if (displayString.StartsWith(TitanTypes.EventReader))
                {
                    var type = ((INamedTypeSymbol)p.Type).TypeArguments[0].ToDisplayString();
                    return new ParameterInfo(ModifierType.Value, type, ArgumentKind.EventReader);
                }

                if (displayString == TitanTypes.EventWriter)
                {
                    return new ParameterInfo(ModifierType.Value, string.Empty, ArgumentKind.EventWriter);
                }

                if (displayString == TitanTypes.EntityManager)
                {
                    return new ParameterInfo(ModifierType.Value, string.Empty, ArgumentKind.EntityManager);
                }

                if (p.Type is IPointerTypeSymbol pointerType)
                {
                    return new ParameterInfo(ModifierType.Pointer, pointerType.PointedAtType.ToDisplayString(), ArgumentKind.Unmanaged);
                }

                var modifier = p.RefKind switch
                {
                    RefKind.In or RefKind.RefReadOnlyParameter => ModifierType.In,
                    RefKind.Ref => ModifierType.Ref,
                    _ => ModifierType.Value
                };
                return new ParameterInfo(modifier, displayString, p.Type.IsUnmanagedType ? ArgumentKind.Unmanaged : ArgumentKind.Managed);
            })
            .ToArray();

        for (var i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];

            var type = parameter.ArgumentKind switch
            {
                ArgumentKind.EventWriter => TitanTypes.EventWriter,
                ArgumentKind.EventReader => $"{TitanTypes.EventReader}<{parameter.Type}>",
                ArgumentKind.Managed => $"{TitanTypes.ManagedResource}<{parameter.Type}>",
                ArgumentKind.Unmanaged => $"{parameter.Type}*",
                ArgumentKind.EntityManager => TitanTypes.EntityManager,
                _ => throw new NotImplementedException($"The kind {parameter.ArgumentKind} has not been implemented.")
            };
            //var type = parameter.IsUnmanaged
            //    ? $"{parameter.Type}*"
            //    : $"{TitanTypes.ManagedResource}<{parameter.Type}>";
            builder.AppendLine($"private static {type} _p{i};");
        }

        builder.AppendLine();

        // Create init function
        builder.AppendLine($"public static void Init(ref {TitanTypes.SystemInitializer} initializer)")
            .AppendLine("{")
            .BeginIndentation();

        for (var i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];
            switch (parameter.ArgumentKind)
            {
                case ArgumentKind.Unmanaged:
                    var resourceFunction = parameter.Modifier switch
                    {
                        ModifierType.Ref or ModifierType.Pointer => "GetMutableResource",
                        _ => "GetReadOnlyResource"
                    };
                    builder.AppendLine($"_p{i} = initializer.{resourceFunction}<{parameter.Type}>();");
                    break;
                case ArgumentKind.Managed:
                    builder.AppendLine($"_p{i} = initializer.GetService<{parameter.Type}>();");
                    break;
                case ArgumentKind.EventReader:
                    builder.AppendLine($"_p{i} = initializer.CreateEventReader<{parameter.Type}>();");
                    break;
                case ArgumentKind.EventWriter:
                    builder.AppendLine($"_p{i} = initializer.CreateEventWriter();");
                    break;
                case ArgumentKind.EntityManager:
                    builder.AppendLine($"_p{i} = initializer.CreateEntityManager();");
                    break;
            }
        }

        builder
            .EndIndentation()
                .AppendLine("}")
                .AppendLine();


        var arguments = string.Join(", ", parameters.Select(static (p, i) =>
        {
            if (p.ArgumentKind is ArgumentKind.Managed)
            {
                return $"_p{i}.Value";
            }

            var mod = p.Modifier switch
            {
                ModifierType.In => "in *",
                ModifierType.Ref => "ref *",
                _ => string.Empty
            };

            return $"{mod}_p{i}";
        }));


        // Create execute function
        builder
            .AppendLine("public static void Execute(void * context)")
                .BeginIndentation()
                .AppendLine($"=> {systemType.Type}.{systemType.Method.Name}({arguments});")
                .EndIndentation();


        builder
            .EndIndentation()
                .AppendLine("}");

        context.AddSource($"{systemTypeName}.g.cs", builder.ToString());
        return systemTypeName;
    }

    private static void AppendNamespace(ISymbol type, FormattedBuilder builder)
    {
        if (type.ContainingNamespace.IsGlobalNamespace)
        {
            return;
        }
        builder.AppendLine($"namespace {type.ContainingNamespace.ToDisplayString()};");
    }
}
