using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Assets.Types;
using Titan.Core.Strings;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering.Resources;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;

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
    private static string CreateMeshDescriptor(AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
    {
        ref readonly var mesh = ref assetDescriptor.Mesh;
        var content =
            $@"{nameof(AssetDescriptor.Mesh)} = new()
            {{
                {nameof(MeshDescriptor.IndexCount)} = {mesh.IndexCount},
                {nameof(MeshDescriptor.VertexCount)} = {mesh.VertexCount},
                {nameof(MeshDescriptor.SubMeshCount)} = {mesh.SubMeshCount},
                {nameof(MeshDescriptor.MaterialCount)} = {mesh.MaterialCount},
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
    private static string CreateFontDescriptor(AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
    {
        ref readonly var font = ref assetDescriptor.Font;
        var content =
            $@"{nameof(AssetDescriptor.Font)} = new()
            {{
                {nameof(FontDescriptor.DefaultGlyphIndex)} = {font.DefaultGlyphIndex},
                {nameof(FontDescriptor.NumberOfGlyphs)} = {font.NumberOfGlyphs},
                {nameof(FontDescriptor.BytesPerPixel)} = {font.BytesPerPixel},
                {nameof(FontDescriptor.Width)} = {font.Width},
                {nameof(FontDescriptor.Height)} = {font.Height}
            }}";

        return CreateBaseDescriptor(assetDescriptor, content, metadata);
    }
    private static string CreateSpriteDescriptor(AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
    {
        ref readonly var sprite = ref assetDescriptor.Sprite;
        ref readonly var texture = ref sprite.Texture;
        var content =
            $@"{nameof(AssetDescriptor.Sprite)} = new()
            {{
                {nameof(SpriteDescriptor.NumberOfSprites)} = {sprite.NumberOfSprites},
                {nameof(SpriteDescriptor.NumberOfNinePatchSprites)} = {sprite.NumberOfNinePatchSprites},
                {nameof(SpriteDescriptor.Texture)} = new()
                {{
                    {nameof(Texture2DDescriptor.Width)} = {texture.Width},
                    {nameof(Texture2DDescriptor.Height)} = {texture.Height},
                    {nameof(Texture2DDescriptor.Stride)} = {texture.Stride},
                    {nameof(Texture2DDescriptor.BitsPerPixel)} = {texture.BitsPerPixel},
                    {nameof(Texture2DDescriptor.DXGIFormat)} = {typeof(DXGI_FORMAT).FullName}.{texture.DXGIFormat}
                }}
            }}";

        return CreateBaseDescriptor(assetDescriptor, content, metadata);
    }

    private static string CreateAudioDescriptor(AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
    {
        ref readonly var audio = ref assetDescriptor.Audio;
        var content =
            $@"{nameof(AssetDescriptor.Audio)} = new()
            {{
            }}";

        return CreateBaseDescriptor(assetDescriptor, content, metadata);
    }

    private static string CreateMaterialDescriptor(AssetDescriptor assetDescriptor, AssetFileMetadata metadata)
    {
        ref readonly var material = ref assetDescriptor.Material;
        var content =
            $@"{nameof(AssetDescriptor.Material)} = new()
            {{
                {nameof(MaterialDescriptor.MaterialCount)} = {material.MaterialCount}
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
            AssetPath = {typeof(StringRef).FullName}.{nameof(StringRef.Create)}(@""{metadata.ContentFileRelativePath}""),
            BinaryAssetPath = {typeof(StringRef).FullName}.{nameof(StringRef.Create)}(@""{metadata.BinaryFileRelativePath}"")
#else
            AssetPath = {typeof(StringRef).FullName}.{nameof(StringRef.Empty)},
            BinaryAssetPath = {typeof(StringRef).FullName}.{nameof(StringRef.Empty)}
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
            var dependenciesCount = meta.Dependencies.Count;
            desc.Dependencies = new((uint)depsIndex, (byte)dependenciesCount);

            AddDependencies(dependencies.Slice(depsIndex, dependenciesCount), _assets, meta);

            depsIndex += dependenciesCount;

            static void AddDependencies(Span<uint> slice, List<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> assets, AssetFileMetadata current)
            {
                var total = 0;
                // Need to loop through dependencies from the one that has dependencies so they are written in the order they are added.
                foreach (var dependency in current.Dependencies)
                {
                    for (var i = 0; i < assets.Count; ++i)
                    {
                        if (assets[i].Metadata == dependency)
                        {
                            slice[total++] = (uint)i;
                        }
                    }
                }
                Debug.Assert(current.Dependencies.Count == total);
            }
        }
    }

    public string Build()
    {
        var numberOfDependencies = _assets
            .Sum(a => a.Metadata.Dependencies.Count);


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
            .BeginScope();

        // The inline struct
        _builder
            .AppendLine($"public static {typeof(RegistryId).FullName} {AssetRegistryIdName} {{ get; }} = {typeof(RegistryId).FullName}.{nameof(RegistryId.GetNext)}();")
            .AppendLine($"private static readonly {AssetStructName} {AssetMemberName};")
            .AppendLine($"private static readonly {DependenciesStructName} {DependenciesMemberName};");


        InsertGetFilePath();
        InsertGetDescriptors();
        InsertGetDependencies();

        InsertGroupedAssets();

        // Static constructor with all descriptors and dependencies
        {
            _builder
                .AppendLine($"static {name}()")
                .BeginScope();

            for (var i = 0; i < _assets.Count; ++i)
            {
                var (descriptor, metadata) = _assets[i];
                var content = descriptor.Type switch
                {
                    AssetType.Texture => CreateTexture2DDescriptor(descriptor, metadata),
                    AssetType.Shader => CreateShaderDescriptor(descriptor, metadata),
                    AssetType.Mesh => CreateMeshDescriptor(descriptor, metadata),
                    AssetType.Audio => CreateAudioDescriptor(descriptor, metadata),
                    AssetType.Font => CreateFontDescriptor(descriptor, metadata),
                    AssetType.Sprite => CreateSpriteDescriptor(descriptor, metadata),
                    AssetType.Material => CreateMaterialDescriptor(descriptor, metadata),
                    _ => throw new NotImplementedException($"The conversion for {descriptor.Type} has not been implemented yet")
                };

                _builder.AppendLine($"{AssetMemberName}[{i}] = {content};");
            }

            for (var i = 0; i < dependencies.Length; ++i)
            {
                _builder.AppendLine($"{DependenciesMemberName}[{i}] = {dependencies[i]};");
            }

            _builder
                .EndScope();
        }

        InsertAssetStruct();
        InsertDependenciesStruct(numberOfDependencies);
        return _builder
            .EndScope()
            .ToString();
    }

    private void InsertGroupedAssets()
    {
        var assetReferences = new (int Index, AssetDescriptor Descriptor, AssetFileMetadata Metadata)[_assets.Count];

        // Get the index for the assets
        for (var i = 0; i < _assets.Count; ++i)
        {
            var (descriptor, metadata) = _assets[i];
            assetReferences[i] = (i, descriptor, metadata);
        }

        // Group by type
        foreach (var groupedAssets in assetReferences.GroupBy(d => d.Descriptor.Type))
        {
            var groupName = GetAssetGroupName(groupedAssets.Key);
            _builder
                .AppendLine($"public readonly struct {groupName}")
                .BeginScope();

            foreach (var asset in groupedAssets)
            {
                var (index, descriptor, metadata) = asset;

                var propertyName = metadata.Name != null
                    ? StringHelper.ToPropertyName(metadata.Name)
                    : $"UnnamedAsset{asset.Index}";

                if (descriptor.Type == AssetType.Sprite && metadata is ImageMetadata imageMetadata)
                {
                    _builder
                        .AppendLine($"public static class {propertyName}")
                        .BeginScope()
                        .AppendLine($"public static ref readonly {typeof(AssetDescriptor).FullName} Asset => ref {AssetMemberName}[{index}];")
                        .AppendLine()
                        .AppendLine("public static class SpriteIndex")
                        .BeginScope();

                    var textureIndex = 0;
                    foreach (var sprite in imageMetadata.Sprites)
                    {
                        var spriteName = StringHelper.ToPropertyName(sprite.Name ?? $"UnamedSprite{textureIndex}");
                        _builder.AppendLine($"public const byte {spriteName} = {textureIndex};");

                        if (sprite.NinePatch != null)
                        {
                            // add indexes for nine patch textures.
                            for (var i = 0; i < 9; ++i)
                            {
                                _builder.AppendLine($"public const byte {spriteName}_{i} = {++textureIndex};");
                            }
                        }
                        textureIndex++;
                    }

                    _builder
                        .EndScope()
                        .EndScope();
                }
                else if (descriptor.Type == AssetType.Material && metadata is MtlMetadata mtlMetadata)
                {
                    _builder
                        .AppendLine($"public static class {propertyName}")
                        .BeginScope()
                        .AppendLine($"public static ref readonly {typeof(AssetDescriptor).FullName} Asset => ref {AssetMemberName}[{index}];");
                    for (var i = 0; i < mtlMetadata.MaterialNames.Length; ++i)
                    {
                        var materialName = StringHelper.ToPropertyName(mtlMetadata.MaterialNames[i] ?? $"UnnamedMaterial{i}");
                        _builder.AppendLine($"public const byte {materialName} = {i};");
                    }

                    _builder.EndScope();
                }
                else
                {
                    _builder
                        .AppendLine($"public static ref readonly {typeof(AssetDescriptor).FullName} {propertyName} => ref {AssetMemberName}[{index}];");
                }
            }
            _builder
                .EndScope()
                .AppendLine();
        }
    }


    private static string GetAssetGroupName(AssetType type) =>
        type switch
        {
            AssetType.Sprite => "Sprites",
            AssetType.Audio => "Audios",
            AssetType.Font => "Fonts",
            AssetType.Material => "Materials",
            AssetType.Mesh => "Meshes",
            AssetType.Shader => "Shaders",
            AssetType.Texture => "Textures",
            _ => type.ToString()
        };

    private void InsertAssetStruct() =>
        _builder
            .AppendLine($"[{typeof(InlineArrayAttribute).FullName}({Math.Max(1, _assets.Count)})]") // inline arrays must have atleast 1 element.
            .AppendLine($"private struct {AssetStructName}")
            .BeginScope()
            .AppendLine($"private {typeof(AssetDescriptor).FullName} _ref;")
            .EndScope();


    private void InsertDependenciesStruct(int numberOfDependencies) =>
        _builder.AppendLine($"[{typeof(InlineArrayAttribute).FullName}({Math.Max(1, numberOfDependencies)})]")// inline arrays must have atleast 1 element.
            .AppendLine($"private struct {DependenciesStructName}")
            .BeginScope()
            .AppendLine($"private {typeof(uint).FullName} _ref;")
            .EndScope();

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
            .BeginScope()
            .AppendLine($"ReadOnlySpan<char> path = \"{binaryFilename}\";")
            .AppendLine("return path;")
            .EndScope();
}
