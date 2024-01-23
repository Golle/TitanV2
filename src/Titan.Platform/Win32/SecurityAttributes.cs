namespace Titan.Platform.Win32;

public unsafe struct SecurityAttributes
{
    public uint nLength;
    public void* lpSecurityDescriptor;
    public int bInheritHandle;
}
