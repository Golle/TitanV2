using System.Diagnostics;

if (args.Length == 0)
{
    Console.WriteLine("Must provide a path. Use Interactive.ps1 to start the interactive console.");
    return 1;
}

var path = args[0];

Console.WriteLine("Welcome to Titan");
Console.WriteLine($"Your path is set to: {path}");

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
                      [2] -> Process Sandbox Assets
                      [3] -> Publish the Sandbox
                      [4] -> Run Sandbox (Make sure you've published it before)
                      
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
            ProcessEngineAssets(path, logging);
            break;
        case ConsoleKey.D2 or ConsoleKey.NumPad2:
            ProcessSandboxAssets(path, logging);
            break;
        case ConsoleKey.D3 or ConsoleKey.NumPad3:
            PublishSandbox(path, logging);
            break;
        case ConsoleKey.D4 or ConsoleKey.NumPad4:
            RunSandbox(path, logging);
            break;

        case ConsoleKey.D5 or ConsoleKey.NumPad5:
            PublishTools(path, logging);
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


static bool ProcessEngineAssets(string workingDirectory, bool logging)
{
    const string AssetProcessor = "Titan.Tools.AssetProcessor.exe";
    var assetProcessorPath = Path.Combine(workingDirectory, "release", "tools", AssetProcessor);
    var timer = Stopwatch.StartNew();
    Console.WriteLine("Processing engine assets");

    const string Arguments = "--path ./content --output ./assets/titan.tbin --code ./src/Titan/ --name Titan.EngineAssetsRegistry";
    var result = RunProgram(assetProcessorPath, Arguments, workingDirectory, redirectOutput: !logging);

    Console.WriteLine($"Completed in {timer.Elapsed.TotalMilliseconds}. Exit Code = {result}");
    return true;
}

static bool ProcessSandboxAssets(string workingDirectory, bool logging)
{
    const string AssetProcessor = "Titan.Tools.AssetProcessor.exe";
    var assetProcessorPath = Path.Combine(workingDirectory, "release", "tools", AssetProcessor);
    var timer = Stopwatch.StartNew();
    Console.WriteLine("Processing engine assets");

    const string Arguments = "--path ./samples/Titan.Sandbox/content --output ./samples/Titan.Sandbox/assets/sandbox.tbin --code ./samples/Titan.Sandbox/ --name Titan.Sandbox.SandboxRegistry";
    var result = RunProgram(assetProcessorPath, Arguments, workingDirectory, redirectOutput: !logging);

    Console.WriteLine($"Completed in {timer.Elapsed.TotalMilliseconds}. Exit Code = {result}");
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

static bool PublishSandbox(string workingDirectory, bool logging)
{
    const string Arguments = "publish ./samples/Titan.Sandbox/Titan.Sandbox.csproj -c Release -o release/sandbox/ -r win-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false";

    Console.WriteLine("Publishing Sandbox");
    var timer = Stopwatch.StartNew();
    var result = RunProgram("dotnet", Arguments, workingDirectory, !logging);
    Console.WriteLine($"Completed in {timer.Elapsed.TotalMilliseconds}. Exit Code = {result}");

    if (result == 0)
    {
        Console.WriteLine("Copy asset files");
        var destination = Path.Combine(workingDirectory, "release", "sandbox", "assets");
        Directory.CreateDirectory(destination);
        File.Copy(Path.Combine(workingDirectory, "assets", "titan.tbin"), Path.Combine(destination, "titan.tbin"), true);
        File.Copy(Path.Combine(workingDirectory, "samples", "Titan.Sandbox", "assets", "sandbox.tbin"), Path.Combine(destination, "sandbox.tbin"), true);
        Console.WriteLine("Finished copying asset files");
    }

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
