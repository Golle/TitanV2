using System.Collections.Immutable;

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
    EntityManager,
    ReadOnlyComponent,
    MutableComponent,
    EntityCollection,
    AssetsManager,
    AudioManager,
    UIManager,
    MaterialsManager,
    MeshManager
}

internal static class SystemsBuilder
{
    private const string QueryFieldName = "_query";

    public static void Build(FormattedBuilder builder, in SystemType system, string name, Dictionary<string, ulong> componentIds)
    {
        var accessibility = system
            .Type
            .DeclaredAccessibility
            .AsString();

        var parameters = system
            .Parameters;

        var components = parameters
            .Where(static p => p.Kind is ArgumentKind.ReadOnlyComponent or ArgumentKind.MutableComponent)
            .Select(p => (Parameter: p, Id: componentIds[p.Type]))
            .OrderBy(static p => p.Id)
            .ToImmutableArray();

        var isEntitySystem = system
            .Parameters
            .Any(static p => p.Kind is ArgumentKind.EntityCollection or ArgumentKind.MutableComponent or ArgumentKind.ReadOnlyComponent);


        if (isEntitySystem)
        {
            //Debugger.Launch();
        }

        builder
            .AppendAutoGenerated()
            .AppendLine()
            .AppendNoWarnings()
            .AppendLine()
            .AppendNamespace(system.Type.ContainingNamespace)
            .AppendLine()
            .AppendLine($"{accessibility} unsafe struct {name}")
            .AppendOpenBracer()
            ;

        AppendResourcesFields(builder, parameters);

        if (isEntitySystem)
        {
            AppendQueryField(builder);
            AppendSignature(builder, components);
            AppendStaticConstructor(builder, name, components);
            AppendInitMethod(builder, parameters, components);
            AppendEntityExecuteMethod(builder, system, parameters, components);
        }
        else
        {
            AppendInitMethod(builder, parameters, components);
            AppendExecuteMethod(builder, system, parameters);
        }


        AppendQueryMethod(builder, isEntitySystem);

        builder
            .AppendCloseBracer();
    }

    private static void AppendExecuteMethod(FormattedBuilder builder, in SystemType system, ImmutableArray<SystemParameter> parameters)
    {
        builder
            .AppendLine("public static void Execute(void * context/* Context is currently not used, placeholder for future dev.*/)")
            .BeginIndentation();

        var arguments = string.Join(", ", parameters.Select(static (p, i) =>
        {
            if (p.Kind is ArgumentKind.Managed)
            {
                return $"_{i}.Value";
            }

            var mod = p.Modifier switch
            {
                ModifierType.Ref => "ref *",
                ModifierType.In => "in *",
                _ => string.Empty
            };
            return $"{mod}_{i}";
        }));

        builder
            .AppendLine($"=> {system.Type.ToDisplayString()}.{system.Method.Name}({arguments});")
            .EndIndentation()
            .AppendLine();
    }

    private static void AppendEntityExecuteMethod(FormattedBuilder builder, in SystemType system, ImmutableArray<SystemParameter> parameters, ImmutableArray<(SystemParameter Parameter, ulong Id)> components)
    {
        var count = components.Length;

        builder
            .AppendLine("public static void Execute(void * context/* Context is currently not used, placeholder for future dev.*/)")
            .AppendOpenBracer()

            .AppendLine("// Complex logic goes here!")
            ;

        // Query
        // Execute, pass

        builder
            .AppendLine($"{TitanTypes.QueryState} state = default;")
            .AppendLine($"{TitanTypes.Entity}* entities;")
            .AppendLine($"var data = stackalloc void*[{count}];")

            .AppendLine()

            .AppendLine("while(_query.EnumerateData(ref state, &entities, data))")
            .AppendOpenBracer();


        builder.AppendLine("var count = state.Count;");
        for (var i = 0; i < count; ++i)
        {
            builder.AppendLine($"var p{i} = new {TitanTypes.Span}<{components[i].Parameter.Type}>(data[{i}], count);");
        }

        var arguments = string.Join(", ", parameters
            .Select((p, i) =>
            {
                if (p.Kind is ArgumentKind.EntityCollection)
                {
                    return $"new {TitanTypes.ReadOnlySpan}<{TitanTypes.Entity}>(entities, count)";
                }

                if (p.Kind is ArgumentKind.ReadOnlyComponent or ArgumentKind.MutableComponent)
                {
                    for (var index = 0; index < components.Length; ++index)
                    {
                        if (components[index].Parameter.Type == p.Type)
                        {
                            return $"p{index}";
                        }
                    }

                    throw new InvalidOperationException("Failed to find the component index. Should not happen.");
                }

                if (p.Kind is ArgumentKind.Managed)
                {
                    return $"_{i}.Value";
                }

                var mod = p.Modifier switch
                {
                    ModifierType.Ref => "ref *",
                    ModifierType.In => "in *",
                    _ => string.Empty
                };
                return $"{mod}_{i}";
            }));


        builder
            .AppendLine($"{system.Type.ToDisplayString()}.{system.Method.Name}({arguments});")
            .AppendCloseBracer();

        builder
            .AppendCloseBracer()
            .AppendLine();
    }

    private static void AppendResourcesFields(FormattedBuilder builder, ImmutableArray<SystemParameter> parameters)
    {
        for (var i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];

            //builder.AppendLine($"// private static {parameter.Type} _{i};");
            var type = parameter.Kind switch
            {
                ArgumentKind.EventWriter => TitanTypes.EventWriter,
                ArgumentKind.EventReader => $"{TitanTypes.EventReader}<{parameter.Type}>",
                ArgumentKind.Managed => $"{TitanTypes.ManagedResource}<{parameter.Type}>",
                ArgumentKind.Unmanaged => $"{parameter.Type}*",
                ArgumentKind.EntityManager => TitanTypes.EntityManager,
                ArgumentKind.AssetsManager => TitanTypes.AssetsManager,
                ArgumentKind.AudioManager => TitanTypes.AudioManager,
                ArgumentKind.UIManager => TitanTypes.UIManager,
                ArgumentKind.MaterialsManager => TitanTypes.MaterialsManager,
                ArgumentKind.MeshManager => TitanTypes.MeshManager,
                ArgumentKind.EntityCollection or ArgumentKind.MutableComponent or ArgumentKind.ReadOnlyComponent
                    => null,
                _ => throw new NotImplementedException($"The kind {parameter.Kind} has not been implemented.")
            };

            if (type is null)
            {
                continue;
            }

            builder
                .AppendLine($"private static {type} _{i};");
        }

        builder
            .AppendLine();
    }

    private static void AppendInitMethod(FormattedBuilder builder, ImmutableArray<SystemParameter> parameters, ImmutableArray<(SystemParameter Parameter, ulong Id)> components)
    {
        const string ArgumentName = "initializer";
        builder
            .AppendLine($"public static void Init(ref {TitanTypes.SystemInitializer} {ArgumentName})")
            .AppendOpenBracer();

        for (var i = 0; i < parameters.Length; ++i)
        {
            var parameter = parameters[i];
            var line = parameter.Kind switch
            {
                ArgumentKind.Unmanaged when parameter.Modifier is ModifierType.Ref or ModifierType.Pointer
                    => $"{ArgumentName}.GetMutableResource<{parameter.Type}>()",

                ArgumentKind.Unmanaged when parameter.Modifier is ModifierType.In or ModifierType.Value
                    => $"{ArgumentName}.GetReadOnlyResource<{parameter.Type}>()",

                ArgumentKind.EventReader
                    => $"{ArgumentName}.CreateEventReader<{parameter.Type}>()",

                ArgumentKind.EventWriter
                    => $"{ArgumentName}.CreateEventWriter()",

                ArgumentKind.EntityManager
                    => $"{ArgumentName}.CreateEntityManager()",

                ArgumentKind.AssetsManager
                    => $"{ArgumentName}.CreateAssetsManager()",

                ArgumentKind.AudioManager
                    => $"{ArgumentName}.CreateAudioManager()",

                ArgumentKind.UIManager
                    => $"{ArgumentName}.CreateUIManager()",

                ArgumentKind.MaterialsManager
                    => $"{ArgumentName}.CreateMaterialsManager()",

                ArgumentKind.MeshManager
                    => $"{ArgumentName}.CreateMeshManager()",

                ArgumentKind.Managed
                    => $"{ArgumentName}.GetService<{parameter.Type}>()",

                _ => null
            };


            if (line is not null)
            {
                builder.AppendLine($"_{i} = {line};");
            }
        }

        foreach (var (component, _) in components)
        {
            if (component.Kind is ArgumentKind.ReadOnlyComponent)
            {
                builder.AppendLine($"{ArgumentName}.AddReadOnlyComponent({component.Type}.Type);");
            }
            else if (component.Kind is ArgumentKind.MutableComponent)
            {
                builder.AppendLine($"{ArgumentName}.AddMutableComponent({component.Type}.Type);");
            }
        }

        builder
            .AppendCloseBracer()
            .AppendLine();
    }

    private static void AppendStaticConstructor(FormattedBuilder builder, string name, ImmutableArray<(SystemParameter Parameter, ulong Id)> components)
    {
        var componentString = string.Join(", ", components.Select(static c => $"{c.Parameter.Type}.Type"));
        builder
            .AppendLine($"static {name}()")
            .AppendOpenBracer()
            .AppendLine($"_query = new([{componentString}], Signature);")
            .AppendCloseBracer()
            .AppendLine();
    }

    private static void AppendSignature(FormattedBuilder builder, ImmutableArray<(SystemParameter Parameter, ulong Id)> components)
    {
        var signature = 1UL;
        foreach (ref readonly var systemParameter in components.AsSpan())
        {
            signature *= systemParameter.Id;
        }

        builder
            .AppendLine($"public const ulong Signature = {signature}UL;")
            .AppendLine();
    }

    private static void AppendQueryField(FormattedBuilder builder) =>
        builder
            .AppendLine($"private static readonly {TitanTypes.CachedQuery} {QueryFieldName};")
            .AppendLine();


    private static void AppendQueryMethod(FormattedBuilder builder, bool isEntitySystem)
    {
        builder
            .AppendLine($"public static {TitanTypes.CachedQuery}* GetQuery()")
            .BeginIndentation();

        builder
            .AppendLine(isEntitySystem ? $"=> {TitanTypes.MemoryUtils}.AsPointer({QueryFieldName});" : "=> null;");

        builder
            .EndIndentation()
            .AppendLine();
    }
}
