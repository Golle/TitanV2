namespace Titan.Platform.Win32.D3D12;

public unsafe interface INativeGuid
{
    static abstract Guid* Guid { get; }
}
