using Titan.Core.Maths;

namespace Titan.Windows.Win32;

internal interface IWindow : IService
{
    nint NativeHandle { get; }
    uint Height { get; }
    uint Width { get; }

    /// <summary>
    /// The Update method will return immediately after processing any events on the queue
    /// </summary>
    /// <returns>False when the window has been closed</returns>
    bool Update();

    /// <summary>
    /// Update blocking will wait for message and then process one before returning
    /// </summary>
    /// <returns>False when the window has been closed</returns>
    bool UpdateBlocking();

    bool SetTitle(ReadOnlySpan<char> title);
    Point GetRelativeCursorPosition();
}
