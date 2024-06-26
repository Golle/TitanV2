using System.Text;

namespace Titan.Generators.Systems;

internal struct ComponentType(string name, string fullName, string @namespace, string accessability)
{
    public readonly string Name = name;
    public readonly string FullName = fullName;
    public readonly string Namespace = @namespace;
    public readonly string Accessability = accessability;
}

internal static class ComponentBuilder
{
    public const string IdFieldName = "Id";
    public static string Build(in ComponentType component, ulong id)
    {
        var builder = new FormattedBuilder(new StringBuilder());
        builder
            .AppendAutoGenerated()
            .AppendLine()
            .AppendLine($"namespace {component.Namespace};")
            .AppendLine()
            .AppendLine($"{component.Accessability} unsafe partial struct {component.Name} : {TitanTypes.IComponent}")
            .AppendOpenBracer()
            .AppendLine($"public static {TitanTypes.ComponentType} Type {{ get; }} = new ({IdFieldName}, (uint)sizeof({component.Name}));")
            .AppendLine($"public const uint {IdFieldName} = {id};")
            .AppendCloseBracer();

        return builder.ToString();
    }
}
