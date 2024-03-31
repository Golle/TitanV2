using Titan.Core.Memory;

namespace Titan.Assets;

public record AssetsConfig(uint FileBufferMaxSize, uint MaxAsyncTasks) : IConfiguration, IDefault<AssetsConfig>
{
    public static readonly uint DefaultFileBufferSize = MemoryUtils.MegaBytes(32);
    public const uint DefaultMaxAsyncTasks = 16;
    public static AssetsConfig Default => new(DefaultFileBufferSize, DefaultMaxAsyncTasks);
}
