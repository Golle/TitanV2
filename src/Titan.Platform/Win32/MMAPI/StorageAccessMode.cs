namespace Titan.Platform.Win32.MMAPI;

public enum StorageAccessMode : uint
{
    STGM_READ = 0x00000000,
    STGM_WRITE = 0x00000001,
    STGM_READWRITE = 0x00000002
}
