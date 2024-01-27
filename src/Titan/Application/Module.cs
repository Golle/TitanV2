﻿namespace Titan.Application;

internal unsafe struct Module
{
    public required string Name;
    public required Type Type;
    private delegate*<IAppBuilder, AppConfig, bool> _build;
    private delegate*<IApp, bool> _init;
    private delegate*<IApp, bool> _shutdown;

    public bool Build(IAppBuilder builder, AppConfig config) => _build(builder, config);
    public bool Init(IApp app) => _init(app);
    public bool Shutdown(IApp app) => _shutdown(app);

    public static Module CreateFromType<T>() where T : IModule =>
        new()
        {
            Name = typeof(T).Name,
            Type = typeof(T),
            _build = &T.Build,
            _init = &T.Init,
            _shutdown = &T.Shutdown
        };
}