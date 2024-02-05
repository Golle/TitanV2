using System.Diagnostics;
using Titan.Windows.Win32;

namespace Titan.Runners;

internal class WindowedRunner : IRunner
{
    private IWindow? _window;
    private Win32MessagePump? _messagePump; // temp

    public static IRunner Create()
        => new WindowedRunner();

    public void Init(IApp app)
    {
        _window = app.GetService<IWindow>();
        _messagePump = app.GetService<Win32MessagePump>();
    }

    public bool RunOnce()
    {
        Debug.Assert(_window != null && _messagePump != null);

        Thread.Sleep(1);
        _messagePump.Update();
        return _window.Update();
    }
}
