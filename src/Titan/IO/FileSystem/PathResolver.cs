using System.Diagnostics;

namespace Titan.IO.FileSystem;

internal static class PathResolver
{
    //NOTE(Jens): This does only support windows. 
    public static string GetAppDataPath(string name)
    {
        Debug.Assert(GlobalConfiguration.Platform == Platforms.Windows, "Currently only supports Windows");
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Debug.Assert(!string.IsNullOrWhiteSpace(localAppData));
        return Path.Combine(localAppData, name);
    }

    public static string GetTempPath(string name)
    {
        var appData = GetAppDataPath(name);
        return Path.Combine(appData, "Temp");
    }

    public static string GetLogsPath(string name)
    {
        var appData = GetAppDataPath(name);
        return Path.Combine(appData, "Logs");
    }
    public static string GetConfigsPath(string name)
    {
        var appData = GetAppDataPath(name);
        return Path.Combine(appData, "Configs");
    }
}
