using Titan.Core.Logging;

namespace Titan;
public static class EngineHelper
{
    //NOTE(Jens): Change this when we're using Titan again.
    public const string TitanFolderName = "TitanV2";
    /// <summary>
    /// Helper to get the path to the engine during development. In release builds this till return null.
    /// <remarks>The engine folder is expected to be located relative to current project. For example c:/git/game and c:/git/titan</remarks>
    /// </summary>
    /// <param name="solutionFileName">The name of the currents projects solution file, or anything that is in the root.</param>
    /// <returns></returns>
    public static string? GetEngineFolder(string solutionFileName)
    {
#if DEBUG
        var currentDirectory = GlobalConfiguration.BasePath;
        var path = FindPath(currentDirectory, solutionFileName, 10);
        if (path == null)
        {
            Logger.Error($"Failed to find the file specified. {solutionFileName}", typeof(EngineHelper));
            return null;
        }

        var engineFolder = Path.Combine(Directory.GetParent(path)!.FullName, TitanFolderName, "Assets");
        if (Directory.Exists(engineFolder))
        {
            return engineFolder;
        }

        Logger.Error($"Failed to find the engine folder at {engineFolder}", typeof(EngineHelper));
#endif
        return null;
    }

    /// <summary>
    /// Searches up the folder tree until a file is found.
    /// </summary>
    /// <param name="filename">The file to look for</param>
    /// <param name="folderName">The folder name that the content is in</param>
    /// <returns>null on failure</returns>
    public static string? GetContentPath(string filename, string folderName)
    {
#if DEBUG
        var currentDirectory = GlobalConfiguration.BasePath;
        var path = FindPath(currentDirectory, filename, 10);

        if (path == null)
        {
            Logger.Error($"Failed to find the file specified. {filename}", typeof(EngineHelper));
            return null;
        }

        var contentPath = Path.Combine(path, folderName);
        if (Directory.Exists(contentPath))
        {
            return contentPath;
        }
        Logger.Error($"Failed to find the content folder at {contentPath}", typeof(EngineHelper));

        return null;
#endif
        return Path.Combine(GlobalConfiguration.BasePath, folderName);
    }

    private static string? FindPath(string current, string filename, int count)
    {
        if (count <= 0)
        {
            // not searching more.
            return null;
        }

        var file = Path.Combine(current, filename);
        if (File.Exists(file))
        {
            return current;
        }

        return FindPath(Directory.GetParent(current)!.FullName, filename, count - 1);
    }
}
