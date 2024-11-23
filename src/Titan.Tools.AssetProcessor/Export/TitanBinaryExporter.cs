using Titan.Tools.AssetProcessor.Processors;

namespace Titan.Tools.AssetProcessor.Export;

internal class TitanBinaryExporter(string outputFile) : IExporter
{
    public async Task<bool> Export(IAssetDescriptorContext context)
    {
        var data = context.GetData();
        var directory = Path.GetDirectoryName(outputFile)!;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        {
            await using var file = File.OpenWrite(outputFile);
            file.SetLength(data.Length);
            await file.WriteAsync(data);
            await file.FlushAsync();
        }

        return true;
    }
}
