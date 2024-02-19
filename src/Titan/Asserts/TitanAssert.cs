using Titan.Core.Logging;
using Titan.Platform.Win32;

namespace Titan.Asserts;

public static unsafe class TitanAssert
{

    private static readonly delegate*<string, void> _messageBox;

    static TitanAssert()
    {
        _messageBox = GlobalConfiguration.Platform switch
        {
            Platforms.Windows => &Win32Message,
            _ => null
        };
    }

    public static void Fatal(string message)
    {
        Logger.Error($"Fatal error occured. Message = {message}");
        if (_messageBox != null)
        {
            _messageBox(message);
        }

        //TODO(Jens): Implement proper shutdown when error occurs.
        Environment.Exit(-1);
    }

    private static void Win32Message(string message)
    {
        fixed (char* pCaption = "FATAL")
        fixed (char* pMessage = message)
        {
            User32.MessageBoxW(default, pMessage, pCaption, 0x00000010);
        }
    }

}
