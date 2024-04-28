using Titan.Tools.AssetProcessor.Processors;

namespace Titan.Tools.AssetProcessor.Export;

internal class ExportPipeline(bool canRunInParallel = true)
{
    private readonly List<IExporter> _exporters = new();

    public ExportPipeline With(IExporter exporter)
    {
        _exporters.Add(exporter);
        return this;
    }

    public async Task<bool> Export(IAssetDescriptorContext context)
    {
        var result = true;

        if (canRunInParallel)
        {
            await Parallel.ForEachAsync(_exporters, async (exporter, token) =>
            {
                var exportResult = await exporter.Export(context);
                if (!exportResult)
                {
                    result = false;
                }
            });
        }
        else
        {
            foreach (var exporter in _exporters)
            {
                var exportResult = await exporter.Export(context);
                if (!exportResult)
                {
                    result = false;
                }
            }
        }

        return result;
    }

}
