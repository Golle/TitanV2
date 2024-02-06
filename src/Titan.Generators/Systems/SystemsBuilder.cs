using System.Diagnostics;
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

internal readonly struct ParameterInfo(ModifierType modifier, string type, bool isUnmanaged)
{
    public readonly ModifierType Modifier = modifier;
    public readonly string Type = type;
    public readonly bool IsUnmanaged = isUnmanaged;
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

        // Create members
        var parameters = systemType
            .Method
            .Parameters
            .Select(static p =>
            {
                if (p.Type is IPointerTypeSymbol pointerType)
                {
                    return new ParameterInfo(ModifierType.Pointer, pointerType.PointedAtType.ToDisplayString(), true);
                }

                var modifier = p.RefKind switch
                {
                    RefKind.In => ModifierType.In,
                    RefKind.Ref => ModifierType.Ref,
                    _ => ModifierType.Value
                };
                return new ParameterInfo(modifier, p.Type.ToDisplayString(), p.Type.IsUnmanagedType);
            })
            .ToArray();

        for (var i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];

            //builder.AppendLine($"// Unmanaged: {unmanaged} {parameter.Type.Name} {parameter.Name} {string.Join(", ", parameter.CustomModifiers.Select(m => $"{m.Modifier.Name} (Optional: {m.IsOptional})"))}");
            var type = parameter.IsUnmanaged
                ? $"{parameter.Type}*"
                : $"{TitanTypes.ManagedResource}<{parameter.Type}>";
            builder.AppendLine($"private static {type} _p{i};");
        }

        builder.AppendLine();

        // Create init function
        builder.AppendLine($"public static void Init({TitanTypes.SystemInitializer} initializer)")
            .AppendLine("{")
            .BeginIndentation();

        for (var i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];
            if (parameter.IsUnmanaged)
            {
                var resourceFunction = parameter.Modifier switch
                {
                    ModifierType.Ref or ModifierType.Pointer => "GetMutableResource",
                    _ => "GetReadOnlyResource"
                };
                builder.AppendLine($"_p{i} = initializer.{resourceFunction}<{parameter.Type}>();");
            }
            else
            {
                builder.AppendLine($"_p{i} = initializer.GetService<{parameter.Type}>();");
            }
        }

        builder
            .EndIndentation()
            .AppendLine("}")
            .AppendLine();



        var arguments = string.Join(", ", parameters.Select(static (p, i) =>
        {
            if (!p.IsUnmanaged)
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
            .AppendLine("public static void Execute()")
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
