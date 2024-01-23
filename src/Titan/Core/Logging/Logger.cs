using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Titan.Core.Logging;

public record struct LogMessage(LogLevel Level, string Message, string? Scope);

public static class Logger
{
    private static BackgroundLogger? _backgroundLogger;
    /// <summary>
    /// Start the logger with a Default Console Logger.
    /// </summary>
    public static IDisposable Start() => Start<ConsoleLogger>();
    public static IDisposable Start<TLogger>(uint maxMessages = 0) where TLogger : ILogger, new() => Start(new TLogger(), maxMessages);
    public static IDisposable Start(ILogger logger, uint maxMessages)
    {
        //NOTE(Jens): If logging is disabled, don't start a background thread
#if LOG_TRACE || LOG_DEBUG || LOG_INFO || LOG_WARNING || LOG_ERROR
        System.Diagnostics.Debug.Assert(_backgroundLogger == null);
        _backgroundLogger = new BackgroundLogger(logger, maxMessages);
#endif
        return new DisposableLogger();
    }

    public static void Shutdown()
    {
#if LOG_TRACE || LOG_DEBUG || LOG_INFO || LOG_WARNING || LOG_ERROR
        System.Diagnostics.Debug.Assert(_backgroundLogger != null);
        _backgroundLogger.Shutdown();
#endif
    }

    [Conditional("LOG_DEBUG")]
    public static void Debug(string message) => Log(LogLevel.Debug, message);

    [Conditional("LOG_DEBUG")]
    public static void Debug<T>(string message) => Debug(message, typeof(T));

    [Conditional("LOG_DEBUG")]
    public static void Debug(string message, Type type) => Log(LogLevel.Debug, message, type.Name);

    [Conditional("LOG_TRACE")]
    public static void Trace(string message) => Log(LogLevel.Trace, message);

    [Conditional("LOG_TRACE")]
    public static void Trace<T>(string message) => Trace(message, typeof(T));

    [Conditional("LOG_TRACE")]
    public static void Trace(string message, Type type) => Log(LogLevel.Trace, message, type.Name);

    [Conditional("LOG_INFO")]
    public static void Info<T>(string message) => Info(message, typeof(T));
    [Conditional("LOG_INFO")]
    public static void Info(string message, Type type) => Log(LogLevel.Info, message, type.Name);
    [Conditional("LOG_INFO")]
    public static void Info(string message) => Log(LogLevel.Info, message);

    [Conditional("LOG_ERROR")]
    public static void Error<T>(string message) => Error(message, typeof(T));
    [Conditional("LOG_ERROR")]
    public static void Error(string message, Type type) => Log(LogLevel.Error, message, type.Name);
    [Conditional("LOG_ERROR")]
    public static void Error(string message) => Log(LogLevel.Error, message);

    [Conditional("LOG_WARNING")]
    public static void Warning<T>(string message) => Warning(message, typeof(T));
    [Conditional("LOG_WARNING")]
    public static void Warning(string message, Type type) => Log(LogLevel.Warning, message, type.Name);
    [Conditional("LOG_WARNING")]
    public static void Warning(string message) => Log(LogLevel.Warning, message);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Raw(string message)
    {
#if LOG_TRACE || LOG_DEBUG || LOG_INFO || LOG_WARNING || LOG_ERROR
        Log(0, message);
#endif
    }

    private static void Log(LogLevel level, string message, string? scope = null)
    {
        System.Diagnostics.Debug.Assert(_backgroundLogger != null);
        var result = _backgroundLogger.TryWrite(new LogMessage(level, message, scope));
        if (!result)
        {
            Console.Error.WriteLine("Failed to write to log channel because it's been closed. (Use Assert later.)");
        }
        //System.Diagnostics.Debug.Assert(result, "Failed to write to channel.");
    }

    private readonly struct DisposableLogger : IDisposable
    {
        public void Dispose() => Shutdown();
    }
}

public enum LogLevel
{
    Trace = 1,
    Debug = 2,
    Info = 4,
    Warning = 8,
    Error = 16,
}
