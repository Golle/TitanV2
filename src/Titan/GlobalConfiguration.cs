using System.Runtime.InteropServices;

namespace Titan;

internal enum Platforms
{
    Windows,
    Linux,
    MacOS
}

internal static class GlobalConfiguration
{
    public static readonly Platforms Platform = GetPlatform();


    private static Platforms GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Platforms.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Platforms.MacOS;
        }
        return Platforms.Linux;
    }
}