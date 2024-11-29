using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Processors;

namespace Titan.Tools.AssetProcessor.Export;
internal sealed class BinaryExporter : IExporter
{
    public async Task<bool> Export(IAssetDescriptorContext context)
    {
        //TODO(Jens): Make this parallel if we need to
        var assets = context.GetAssets();
        var data = context.GetData();
        for (var i = 0; i < assets.Length; i++)
        {
            var (descriptor, metadata) = assets.Span[i];
            Logger.Trace<BinaryExporter>($"Writing binary asset. ID = {metadata.Id} Path = {metadata.BinaryFileFullPath}");

            var directory = Path.GetDirectoryName(metadata.BinaryFileFullPath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var slice = data.Slice((int)descriptor.File.Offset, (int)descriptor.File.Length);
            await File.WriteAllBytesAsync(metadata.BinaryFileFullPath, slice);
        }

        return true;
    }
}

