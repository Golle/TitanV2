using System.CommandLine;
using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Tools.AssetProcessor;
using Titan.Tools.AssetProcessor.Export;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Processors;
using Titan.Tools.AssetProcessor.Processors.Audio;
using Titan.Tools.AssetProcessor.Processors.Fonts;
using Titan.Tools.AssetProcessor.Processors.Models;
using Titan.Tools.AssetProcessor.Processors.Shaders;
using Titan.Tools.AssetProcessor.Processors.Textures;

using var _ = Logger.Start<ConsoleLogger>(10_000u);

if (!StartupHelper.VerifyAssetSerialization())
{
    Logger.Error<Program>("New asset types added, fix the errors to continue..");
    return 2;
}

var pathOption = new Option<string>("--path", "The path to the content folder to index and process") { IsRequired = true };
var outputOption = new Option<string>("--output", "The output path and name for the binary files") { IsRequired = true };
var codeOutputOption = new Option<string>("--code", "The output folder for the C# code generated.") { IsRequired = false };
var nameOption = new Option<string>("--name", "The full name of the type created as the registry(ex MyGameProject.Assets.Registry1)") { IsRequired = false };
var tmpOption = new Option<string>("--tmp", "The temp folder for temporary assets during processing.") { IsRequired = true };
var fileOption = new Option<string?>("--file", "The file, must be in a subdirectory of the --path") { IsRequired = false, };
var command = new RootCommand
{
    pathOption,
    outputOption,
    codeOutputOption,
    nameOption,
    tmpOption,
    fileOption
};

var returnCode = 0;
command.SetHandler(async (path, bin, code, name, tmp, file) =>
{
    if (string.IsNullOrWhiteSpace(file))
    {
        var failed = false;
        if (string.IsNullOrWhiteSpace(code))
        {
            Logger.Error<Program>("--code is required");
            failed = true;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            Logger.Error<Program>("--name is required");
            failed = true;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            failed = true;
        }

        if (failed)
        {
            returnCode = -1;
            return;
        }
    }

    var contentFileReader = new ContentFiles(path, Path.GetDirectoryName(bin)!, new MetadataBuilder());
    var timer = Stopwatch.StartNew();
    var result = await contentFileReader.VerifyMetadataFiles();
    if (!result)
    {
        Logger.Error("Verification failed. Please fix the errors and try again.");
        returnCode = 1;
        return;
    }

    var metadataFiles = await contentFileReader.GetFiles(file);
    if (metadataFiles == null)
    {
        Logger.Error<Program>("Error occurred when reading metadata files.");
        returnCode = 3;
        return;
    }

    if (!Directory.Exists(tmp))
    {
        Directory.CreateDirectory(tmp);
    }
    var context = new SortedAssetDescriptorContext(metadataFiles)
    {
        TempFolderPath = tmp
    };
    var pipeline = new AssetPipeline()
        .With<ImageProcessor>()
        .With<ShaderProcessor>()
        .With<ObjModelProcessor>()
        .With<FontProcessor>()
        .With<AudioProcessor>();

    var pipelineResult = await pipeline.Run(metadataFiles, context);
    if (!pipelineResult)
    {
        Logger.Error<Program>("Error occurred in the asset pipeline");
        returnCode = 4;
        return;
    }

    foreach (var (level, message) in context.Diagnostics)
    {
        switch (level)
        {
            case DiagnosticsLevel.Error: Logger.Error<Program>(message); break;
            case DiagnosticsLevel.Info: Logger.Info<Program>(message); break;
            case DiagnosticsLevel.Warning: Logger.Warning<Program>(message); break;
        }
    }

    if (context.HasErrors)
    {
        Logger.Error<Program>("Pipeline contains errors, please check the log and fix the issues.");
        returnCode = 6;
        return;
    }

    var exportPipeline = new ExportPipeline();
    if (file == null)
    {
        exportPipeline = exportPipeline
            .With(new RegistryExporter(code, name, Path.GetFileName(bin)))
            .With(new BinaryExporter())
            .With(new TitanBinaryExporter(bin));
    }
    else
    {
        //NOTE(Jens): This is single file processing, called when files changes.
        exportPipeline = exportPipeline
            .With(new BinaryExporter());
    }

    var exportResult = await exportPipeline.Export(context);
    if (!exportResult)
    {
        Logger.Error<Program>("Failed to export AssetDescriptors");
        returnCode = 7;
        return;
    }

    timer.Stop();
    Logger.Info<Program>($"Completed in {timer.Elapsed.TotalMilliseconds} ms");

}, pathOption, outputOption, codeOutputOption, nameOption, tmpOption, fileOption);

var res = await command.InvokeAsync(args);
return returnCode != 0 ? returnCode : res;
