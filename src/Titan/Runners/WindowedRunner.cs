using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Systems;
using Titan.Windows.Win32;

namespace Titan.Runners;

internal class WindowedRunner : IRunner
{
    private IWindow? _window;
    private ISystemsScheduler? _scheduler;

    public static IRunner Create()
        => new WindowedRunner();

    public void Init(IApp app)
    {
        _window = app.GetService<IWindow>();
        _scheduler = app.GetService<ISystemsScheduler>();
        Logger.Warning<WindowedRunner>("We currently have a Thread.Sleep(1) in the loop. Remove that when we have rendering.");
    }

    public bool RunOnce()
    {
        Thread.Sleep(1);
        Debug.Assert(_window != null && _scheduler != null);
        
        var updateResult = _window.Update();

        _scheduler.Execute();
        return updateResult;
    }
}
