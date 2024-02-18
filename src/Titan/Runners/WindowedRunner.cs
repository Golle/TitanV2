using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Systems;
using Titan.Windows.Win32;

namespace Titan.Runners;

internal class WindowedRunner : IRunner
{
    private IWindow? _window;
    private SystemsScheduler? _scheduler;

    private ulong _frameCount;
    private Stopwatch _timer;
    public static IRunner Create()
        => new WindowedRunner();

    public void Init(IApp app)
    {
        _window = app.GetService<IWindow>();
        _scheduler = app.GetService<SystemsScheduler>();
        Logger.Warning<WindowedRunner>("We currently have a Thread.Sleep(1) in the loop. Remove that when we have rendering.");
        _timer = Stopwatch.StartNew();
    }

    public bool RunOnce()
    {
        Thread.Sleep(100);
        Debug.Assert(_window != null && _scheduler != null);

        var updateResult = !_window.Update();
        _scheduler.Execute();
        _frameCount++;
        if (_timer.Elapsed.TotalSeconds > 5)
        {
            var fps = _frameCount / _timer.Elapsed.TotalSeconds;
            Logger.Info<WindowedRunner>($"FPS = {fps}");

            _frameCount = 0;
            _timer.Restart();
        }

        return updateResult;
    }
}
