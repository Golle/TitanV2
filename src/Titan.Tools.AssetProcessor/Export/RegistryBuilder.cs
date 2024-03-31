using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Assets.Types;
using Titan.Core.Strings;
using Titan.Graphics.Resources;
using Titan.Platform.Win32.DXGI;
using Titan.Tools.AssetProcessor.Metadata;

namespace Titan.Tools.AssetProcessor.Export;
internal class RegistryBuilder(string? @namespace, string name, string binaryFilename)
{
    private readonly FormattedBuilder _builder = new();
    private readonly List<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> _assets = new();
    private const string AssetRegistryIdName = "Id";
    private const string AssetStructName = "__ASSETS__";
    private const string DependenciesStructName = "__ASSETS_DEPS__";

    private const string AssetMemberName = "_assets";
    private const string DependenciesMemberName = "_dependencies";
    public void AddDescriptor(in AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
        => _assets.Add((assetDescriptor, metadata));

    private static string CreateTexture2DDescriptor(in AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
    {
        ref readonly var texture = ref assetDescriptor.Texture2D;
        var content =
            $@"{nameof(AssetDescriptor.Texture2D)} = new()
            {{
                {nameof(Texture2DDescriptor.Width)} = {texture.Width},
                {nameof(Texture2DDescriptor.Height)} = {texture.Height},
                {nameof(Texture2DDescriptor.Stride)} = {texture.Stride},
                {nameof(Texture2DDescriptor.BitsPerPixel)} = {texture.BitsPerPixel},
                {nameof(Texture2DDescriptor.DXGIFormat)} = {typeof(DXGI_FORMAT).FullName}.{texture.DXGIFormat}
            }}";

        return CreateBaseDescriptor(assetDescriptor, content, metadata);
    }

    private static string CreateShaderDescriptor(AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
    {
        ref readonly var shader = ref assetDescriptor.Shader;
        var content =
            $@"{nameof(AssetDescriptor.Shader)} = new()
            {{
                {nameof(ShaderDescriptor.Type)} = {typeof(ShaderType).FullName}.{shader.Type}
            }}";
        return CreateBaseDescriptor(assetDescriptor, content, metadata);
    }

    private static string CreateBaseDescriptor(in AssetDescriptor baseDescriptor, string content, AssetFileMetadata metadata)
    {
        //NOTE(Jens): Consider removing AssetPath, it can be useful in debug/tracing/logging but it will use some additional memory.
        return $@"new {typeof(AssetDescriptor).FullName}
        {{
            {nameof(AssetDescriptor.Id)} = {typeof(AssetId).FullName}.{nameof(AssetId.GetNext)}(),
            {nameof(AssetDescriptor.RegistryId)} = {AssetRegistryIdName},
            {nameof(AssetDescriptor.Type)} = {typeof(AssetType).FullName}.{baseDescriptor.Type},
            {nameof(AssetDescriptor.File)} = 
            {{ 
                {nameof(FileDescriptor.Length)} = {baseDescriptor.File.Length},
                {nameof(FileDescriptor.Offset)} = {baseDescriptor.File.Offset},
#if !RELEASE
            AssetPath = {typeof(StringRef).FullName}.{nameof(StringRef.Create)}(@""{metadata.ContentFileRelativePath}"")
#else
            AssetPath = {typeof(StringRef).FullName}.{nameof(StringRef.Empty)}
#endif
            }},
            {nameof(AssetDescriptor.Dependencies)} = new({baseDescriptor.Dependencies.Index}, {baseDescriptor.Dependencies.Count}),
            {content}
        }}";
    }

    private void PrepareAssetDescriptors(Span<uint> dependencies)
    {
        var depsIndex = 0;
        for (var i = 0; i < _assets.Count; ++i)
        {
            var meta = _assets[i].Metadata;
            if (meta.Dependencies.Count == 0)
            {
                continue;
            }
            ref var desc = ref CollectionsMarshal.AsSpan(_assets)[i].Descriptor;
            var dependenciesCount = meta.DependsOn.Length;
            desc.Dependencies = new((uint)depsIndex, (byte)dependenciesCount);

            AddDependencies(dependencies.Slice(depsIndex, dependenciesCount), _assets, meta);

            depsIndex += dependenciesCount;

            static void AddDependencies(Span<uint> slice, List<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> assets, AssetFileMetadata current)
            {
                var total = 0;
                for (var i = 0; i < assets.Count; ++i)
                {
                    if (current.Dependencies.Contains(assets[i].Metadata))
                    {
                        slice[total++] = (uint)i;
                    }
                }
                Debug.Assert(current.Dependencies.Count == total);
            }
        }
    }

    public string Build()
    {
        var numberOfDependencies = _assets
            .Sum(a => a.Metadata.DependsOn.Length);
        Span<uint> dependencies = stackalloc uint[numberOfDependencies];

        PrepareAssetDescriptors(dependencies);


        // Signature of the Registry
        _builder
            .AppendLine("// Auto-Generated")
            .AppendLine("#pragma warning disable")
            .AppendLine();

        if (@namespace != null)
        {
            _builder.AppendLine($"namespace {@namespace};");
        }

        _builder
            .AppendLine($"public readonly struct {name} : {typeof(IAssetRegistry).FullName}")
            .AppendLine("{")
            .BeginIndentation();

        // The inline struct
        _builder
            .AppendLine($"public static {typeof(RegistryId).FullName} {AssetRegistryIdName} {{ get; }} = {typeof(RegistryId).FullName}.{nameof(RegistryId.GetNext)}();")
            .AppendLine($"private static {AssetStructName} {AssetMemberName};")
            .AppendLine($"private static {DependenciesStructName} {DependenciesMemberName};"); ;


        InsertGetFilePath();
        InsertGetDescriptors();
        InsertGetDependencies();

        // Descriptor references
        {
            for (var i = 0; i < _assets.Count; ++i)
            {
                var (_, metadata) = _assets[i];
                var propertyName = metadata.Name != null
                    ? StringHelper.ToPropertyName(metadata.Name)
                    : $"UnnamedAsset{i}";
                _builder
                    .AppendLine($"public static ref readonly {typeof(AssetDescriptor).FullName} {propertyName} => ref {AssetMemberName}[{i}];");
            }
        }

        // Static constructor with all descriptors and dependencies
        {
            _builder
                .AppendLine($"static {name}()")
                .AppendLine("{")
                .BeginIndentation();

            for (var i = 0; i < _assets.Count; ++i)
            {
                var (descriptor, metadata) = _assets[i];
                var content = descriptor.Type switch
                {
                    AssetType.Texture2D => CreateTexture2DDescriptor(descriptor, metadata),
                    AssetType.Shader => CreateShaderDescriptor(descriptor, metadata),
                    _ => throw new NotImplementedException($"The conversion for {descriptor.Type} has not been implemented yet")
                };

                _builder.AppendLine($"{AssetMemberName}[{i}] = {content};");
            }

            for (var i = 0; i < dependencies.Length; ++i)
            {
                _builder.AppendLine($"{DependenciesMemberName}[{i}] = {dependencies[i]};");
            }

            _builder
                .EndIndentation()
                .AppendLine("}");
        }



        InsertAssetStruct();
        InsertDependenciesStruct(numberOfDependencies);
        return _builder
            .EndIndentation()
            .AppendLine("}")
            .ToString();
    }

    private void InsertAssetStruct() =>
        _builder
            .AppendLine($"[{typeof(InlineArrayAttribute).FullName}({Math.Max(1, _assets.Count)})]") // inline arrays must have atleast 1 element.
            .AppendLine($"private struct {AssetStructName}")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"private {typeof(AssetDescriptor).FullName} _ref;")
            .EndIndentation()
            .AppendLine("}");

    private void InsertDependenciesStruct(int numberOfDependencies) =>
        _builder.AppendLine($"[{typeof(InlineArrayAttribute).FullName}({Math.Max(1, numberOfDependencies)})]")// inline arrays must have atleast 1 element.
            .AppendLine($"private struct {DependenciesStructName}")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"private {typeof(uint).FullName} _ref;")
            .EndIndentation()
            .AppendLine("}");
    private void InsertGetDescriptors() =>
        _builder
            .AppendLine($"public static ReadOnlySpan<{typeof(AssetDescriptor).FullName}> {nameof(IAssetRegistry.GetAssetDescriptors)}()")
            .BeginIndentation()
            .AppendLine($"=> {AssetMemberName};")
            .EndIndentation();

    private void InsertGetDependencies() =>
        _builder
            .AppendLine($"public static ReadOnlySpan<{typeof(uint).FullName}> {nameof(IAssetRegistry.GetDependencies)}(in {typeof(AssetDescriptor).FullName} descriptor)")
            .BeginIndentation()
            .AppendLine($"=> ((ReadOnlySpan<{typeof(uint).FullName}>){DependenciesMemberName}).Slice((int)descriptor.{nameof(AssetDescriptor.Dependencies)}.{nameof(AssetDescriptor.Dependencies.Index)}, (int)descriptor.{nameof(AssetDescriptor.Dependencies)}.{nameof(AssetDescriptor.Dependencies.Count)});")
            .EndIndentation();

    private void InsertGetFilePath() =>
        _builder
            .AppendLine($"public static ReadOnlySpan<char> {nameof(IAssetRegistry.GetFilePath)}()")
            .AppendLine("{")
            .BeginIndentation()
            .AppendLine($"ReadOnlySpan<char> path = \"{binaryFilename}\";")
            .AppendLine("return path;")
            .EndIndentation()
            .AppendLine("}");
}
