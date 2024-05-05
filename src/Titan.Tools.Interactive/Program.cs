using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

if (args.Length == 0)
{
    Console.WriteLine("Must provide a path to he engine. Use Interactive.ps1 to start the interactive console.");
    return 1;
}


var config = LoadConfig();
if (config != null)
{
    Console.WriteLine("Titan Configuration");
    Console.WriteLine($"\tProject = {config.Project}");
    Console.WriteLine($"\tContent = {config.Content}");
    Console.WriteLine($"\tOutput = {config.Output}");
}
else
{
    Console.WriteLine("No config. Make sure you're running this from the correct directory.");
    return 1;
}
var titanPath = args[0];

Console.WriteLine("Welcome to Titan");
Console.WriteLine($"Your Engine Path is set to: {titanPath}");

var logging = true;
do
{
    Console.WriteLine();
    Console.Write("Please select an option. (Logging is ");
    Console.ForegroundColor = logging ? ConsoleColor.Green : ConsoleColor.Red;
    Console.Write(logging ? "On" : "Off");
    Console.ResetColor();
    Console.WriteLine(")");

    Console.WriteLine($"""
                      
                      [1] -> Process Engine Assets
                      [2] -> Process {config.Name} Assets
                      [3] -> Publish {config.Name}
                      [4] -> Run {config.Name} (Make sure you've published it before)
                      
                      [5] -> Publish Tools
                      
                      [L] -> Toggle logging
                      """);

    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.Escape)
    {
        break;
    }

    switch (key.Key)
    {
        case ConsoleKey.D1 or ConsoleKey.NumPad1:
            ProcessEngineAssets(titanPath, config, logging);
            break;
        case ConsoleKey.D2 or ConsoleKey.NumPad2:
            ProcessGameAssets(titanPath, config, logging);
            break;
        case ConsoleKey.D3 or ConsoleKey.NumPad3:
            PublishGame(titanPath, config, logging);
            break;
        case ConsoleKey.D4 or ConsoleKey.NumPad4:
            RunSandbox(titanPath, logging);
            break;

        case ConsoleKey.D5 or ConsoleKey.NumPad5:
            PublishTools(titanPath, logging);
            break;
        case ConsoleKey.L:
            logging = !logging;
            break;
        default:
            Console.WriteLine("Unrecognized command.");
            break;
    }

} while (true);


Console.WriteLine();
Console.WriteLine("Thanks for using the interactive console!");


return 0;


static bool ProcessEngineAssets(string titanDirectory, TitanConfig config, bool logging)
{
    const string AssetProcessor = "Titan.Tools.AssetProcessor.exe";
    var assetProcessorPath = Path.Combine(titanDirectory, "release", "tools", AssetProcessor);
    var timer = Stopwatch.StartNew();
    Console.WriteLine("Processing engine assets");
    var content = Path.Combine(titanDirectory, "content");
    var binary = Path.Combine(titanDirectory, "assets", "titan.tbin");
    Console.WriteLine($"\tInput = {content}");
    Console.WriteLine($"\tOutput = {binary}");

    var arguments = $"--path {content} --output {binary} --code ./src/Titan/ --name Titan.EngineAssetsRegistry";
    var result = RunProgram(assetProcessorPath, arguments, titanDirectory, redirectOutput: !logging);

    Console.WriteLine($"Completed in {timer.Elapsed.TotalMilliseconds}. Exit Code = {result}");
    return true;
}

static bool ProcessGameAssets(string titanDirectory, TitanConfig config, bool logging)
{
    const string AssetProcessor = "Titan.Tools.AssetProcessor.exe";
    var assetProcessorPath = Path.Combine(titanDirectory, "release", "tools", AssetProcessor);
    var timer = Stopwatch.StartNew();
    Console.WriteLine("Processing game assets");
    var binary = Path.Combine(config.Assets, config.Binary);
    Console.WriteLine($"\tInput = {config.Content}");
    Console.WriteLine($"\tOutput = {binary}");


    var arguments = $"--path {config.Content} --output {binary} --code {config.CodePath} --name {config.AssetRegistryName}";
    var result = RunProgram(assetProcessorPath, arguments, titanDirectory, redirectOutput: !logging);

    Console.WriteLine($"Completed in {timer.Elapsed.TotalMilliseconds}. Exit Code = {result}");
    return true;
}


static bool PublishGame(string titanDirectory, TitanConfig config, bool logging)
{
    var arguments = $"publish {config.Project} -c Release -o {config.Output} -r win-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false";

    Console.WriteLine($"Publishing {config.Name}");
    var timer = Stopwatch.StartNew();
    var result = RunProgram("dotnet", arguments, string.Empty, !logging);
    Console.WriteLine($"Completed in {timer.Elapsed.TotalMilliseconds}. Exit Code = {result}");

    if (result == 0)
    {
        Console.WriteLine("Copy asset files");
        var destination = Path.Combine(config.Output, "assets");
        Directory.CreateDirectory(destination);
        File.Copy(Path.Combine(titanDirectory, "assets", "titan.tbin"), Path.Combine(destination, "titan.tbin"), true);
        File.Copy(Path.Combine(config.Assets, config.Binary), Path.Combine(destination, config.Binary), true);
        Console.WriteLine("Finished copying asset files");
    }

    return true;
}

static bool PublishTools(string workingDirectory, bool logging)
{
    const string Arguments = "publish ./src/Titan.Tools.AssetProcessor./Titan.Tools.AssetProcessor.csproj -c Release -o release/tools/ -r win-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false";

    Console.WriteLine("Publishing Tools");
    var timer = Stopwatch.StartNew();
    var result = RunProgram("dotnet", Arguments, workingDirectory, !logging);
    Console.WriteLine($"Completed in {timer.Elapsed.TotalMilliseconds}. Exit Code = {result}");

    return true;
}

static bool RunSandbox(string workingDirectory, bool logging)
{
    var timer = Stopwatch.StartNew();
    var result = RunProgram(Path.Combine("release", "sandbox", "Titan.Sandbox.exe"), string.Empty, workingDirectory, !logging);
    Console.WriteLine($"Finished sandbox after {timer.Elapsed.TotalMilliseconds} ms. Exit Code = {result}");
    return true;
}

static int RunProgram(string command, string arguments, string workingDirectory, bool redirectOutput)
{
    Console.WriteLine($"Executing command {Path.GetFileNameWithoutExtension(command)}");
    const int padding = -20;
    Console.WriteLine($" {"Path:",padding} {command}");
    Console.WriteLine($" {"Arguments:",padding} {arguments}");
    Console.WriteLine($" {"Working Directory:",padding} {workingDirectory}");

    Console.WriteLine();
    var process = Process.Start(new ProcessStartInfo(command, arguments)
    {
        CreateNoWindow = false,
        UseShellExecute = false,
        WorkingDirectory = workingDirectory,
        RedirectStandardError = redirectOutput,
        RedirectStandardOutput = redirectOutput
    });
    if (process == null)
    {
        Console.WriteLine($"Failed to start the process. Path = {command}");
        return -1;
    }
    process.WaitForExit();
    return process.ExitCode;
}


static TitanConfig? LoadConfig()
{
    var current = Directory.GetCurrentDirectory();
    var path = Path.Combine(current, ".titanconfig");
    if (File.Exists(path))
    {
        Console.WriteLine($"Reading config from {path}");
        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize(json, TitanConfigJsonContext.Default.TitanConfig);
        if (config != null)
        {
            var projectFilePath = Path.Combine(current, config.Project);
            var projectPath = Path.GetDirectoryName(projectFilePath)!;
            var outputPath = Path.Combine(current, config.Output);
            return config with
            {
                Name = config.Name ?? Path.GetFileNameWithoutExtension(config.Project),
                Output = outputPath,
                Project = projectFilePath,
                Content = Path.Combine(current, config.Content),
                Assets = Path.Combine(current, config.Assets),
                CodePath = Path.Combine(projectPath, config.CodePath)
            };
        }
    }
    Console.WriteLine("No config found or failed to read it.");
    return null;
}



internal record TitanConfig
{
    public string Project { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Assets { get; init; } = string.Empty;
    public string Output { get; init; } = string.Empty;
    public string? Name { get; init; } = null;
    public string Binary { get; init; } = string.Empty;
    public string CodePath { get; init; } = string.Empty;
    public string AssetRegistryName { get; init; } = string.Empty;

}
[JsonSourceGenerationOptions(JsonSerializerDefaults.General)]
[JsonSerializable(typeof(TitanConfig))]
internal partial class TitanConfigJsonContext : JsonSerializerContext;
