using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using Titan.Core.Logging;

namespace Titan.Tools.AssetProcessor.Metadata;
internal sealed class ContentFiles(string contentFolder, string binaryFolder, MetadataBuilder metadataBuilder) : IContentFiles
{
    private const string MetadataFileExtension = ".kmeta";
    private const string BinaryFileExtension = ".kbin";
    private static readonly string[] IgnoredFileExtensions = [".md", ".hlsli", ".blend", ".blend1"];

    public async Task<bool> VerifyMetadataFiles()
    {
        var result = true;
        var files = EnumerateFiles(contentFolder, "*")
            .Where(f => !IsIgnored(f));

        foreach (var path in files)
        {
            var filename = Path.GetFileName(path);
            var directory = Path.GetDirectoryName(path)!;
            var relativePath = Path.GetRelativePath(contentFolder, path);

            if (filename.EndsWith(MetadataFileExtension))
            {
                var assetFilename = Path.GetFileNameWithoutExtension(filename);
                var assetPath = Path.Combine(directory, assetFilename);
                if (!File.Exists(assetPath))
                {
                    Logger.Error<ContentFiles>($"Found metadata file without a content file. Please delete or rename it. Path = {relativePath}");
                    result = false;
                }
                continue;
            }

            // Create the Metadata files if they don't exist
            Logger.Trace<ContentFiles>($"Found asset file. Path = {relativePath}");
            var metadataFile = Path.Combine(directory, $"{filename}{MetadataFileExtension}");
            if (!File.Exists(metadataFile))
            {
                Logger.Info<ContentFiles>($"Creating Metadata. Path = {relativePath}");
                await using var stream = File.OpenRead(path);
                var metadata = metadataBuilder.CreateFromContent(filename, stream);
                if (metadata == null)
                {
                    Logger.Error<ContentFiles>($"Failed to create metadata for file. Unsupported type. Type = {Path.GetExtension(relativePath)}. Path = {relativePath}");
                    result = false;
                }
                else
                {
                    await WriteMetadata(metadataFile, metadata);
                }
            }
        }

        return result;

        static bool IsIgnored(string file)
            => IgnoredFileExtensions.Any(ext => Path.GetExtension(file).Equals(ext, StringComparison.OrdinalIgnoreCase));

    }

    public async Task<AssetFileMetadata[]?> GetFiles()
    {
        ConcurrentDictionary<Guid, AssetFileMetadata> metadatas = new();
        var files = EnumerateFiles(contentFolder, $"*{MetadataFileExtension}");
        await Parallel.ForEachAsync(files, async (file, _) =>
        {
            var metadata = await ReadMetadata(file);
            if (metadata == null)
            {
                Logger.Error<ContentFiles>($"Failed to deserialize metadata file {Path.GetRelativePath(contentFolder, file)}");
                return;
            }
            var assetFilename = Path.GetFileNameWithoutExtension(file);
            var filename = Path.GetFileNameWithoutExtension(assetFilename);
            var extension = Path.GetExtension(assetFilename).ToLowerInvariant();
            var binaryFileName = $"{filename}{extension}{BinaryFileExtension}";
            var directory = Path.GetDirectoryName(file)!;
            metadata.ContentFileFullPath = Path.Combine(directory, assetFilename);
            metadata.ContentFileRelativePath = Path.GetRelativePath(contentFolder, metadata.ContentFileFullPath);
            metadata.MetadataFileFullPath = file;
            metadata.MetadataFileRelativePath = Path.GetRelativePath(contentFolder, file);
            var relativeFolder = Path.GetDirectoryName(metadata.ContentFileRelativePath)!;

            metadata.BinaryFileRelativePath = Path.Combine(relativeFolder, binaryFileName);
            metadata.BinaryFileFullPath = Path.Combine(binaryFolder, metadata.BinaryFileRelativePath);

            metadata.FileExtension = extension;
            if (!metadatas.TryAdd(metadata.Id, metadata))
            {
                throw new InvalidOperationException("Failed to add metadata. Probably duplicate key.");
            }
        });

        var hasErrors = false;
        foreach (var assetFileMetadata in metadatas.Values)
        {
            if (!ResolveDependencies(assetFileMetadata, metadatas))
            {
                Logger.Error<ContentFiles>($"Failed to resolve dependencies: AssetID = {assetFileMetadata.Id}");
                hasErrors = true;
            }
        }

        return hasErrors ? null : metadatas.Values.ToArray();

        static async Task<AssetFileMetadata?> ReadMetadata(string metadataFile)
        {
            await using var stream = File.Open(metadataFile, FileMode.Open, FileAccess.Read, FileShare.None);
            try
            {
                return await JsonSerializer.DeserializeAsync(stream, AssetMetadataJsonContext.Default.AssetFileMetadata);

            }
            catch (Exception e)
            {
                Logger.Error<ContentFiles>($"Failed to deserialize the content. Message = {e.Message}");
                return null;
            }

        }

        static bool ResolveDependencies(AssetFileMetadata metadata, IDictionary<Guid, AssetFileMetadata> metadatas)
        {
            if (metadata.DependsOn.Length == 0)
            {
                return true;
            }

            var result = true;
            var dependencies = new AssetFileMetadata[metadata.DependsOn.Length];
            for (var i = 0; i < metadata.DependsOn.Length; ++i)
            {
                if (!metadatas.TryGetValue(metadata.DependsOn[i], out var value))
                {
                    Logger.Error<ContentFiles>($"Failed to find dependency. Asset = {metadata.Id}. Dependency = {metadata.DependsOn[i]}");
                    result = false;

                }
                else
                {
                    dependencies[i] = value;
                }
            }

            metadata.Dependencies = dependencies;
            return result;
        }
    }

    private async Task WriteMetadata(string path, AssetFileMetadata metadata)
    {
        await using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, metadata, AssetMetadataJsonContext.Default.AssetFileMetadata);
    }

    private static IEnumerable<string> EnumerateFiles(string basePath, string pattern)
        => Directory.EnumerateFiles(basePath, pattern, SearchOption.AllDirectories);
}
