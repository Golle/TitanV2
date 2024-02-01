// Comment out this line to disable tracing
#define TRACE_FILE_API

using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using static Titan.Platform.Win32.GenericRights;

namespace Titan.Core.IO.Platform;

internal unsafe struct Win32FileApi : INativeFileApi
{
    public static NativeFileHandle Open(ReadOnlySpan<char> path, FileAccess access, bool createIfNotExist)
    {
        Trace($"Open file. Path = {path} Access = {access} CreateIfNotExit = {createIfNotExist}");
        var desiredAccess = access switch
        {
            FileAccess.Read => GENERIC_READ,
            FileAccess.Write => GENERIC_WRITE,
            FileAccess.ReadWrite => GENERIC_WRITE | GENERIC_READ,
            _ => GENERIC_ALL
        };
        fixed (char* pPath = path)
        {
            var creationDisposition = createIfNotExist
                ? CreationDisposition.OPEN_ALWAYS
                : CreationDisposition.OPEN_EXISTING;

            var handle = Kernel32.CreateFileW(pPath, (uint)desiredAccess, 0, null, (uint)creationDisposition, (uint)FileAttribute.FILE_ATTRIBUTE_NORMAL, default);
            if (handle.IsValid())
            {
                return new(handle);
            }
        }
        return NativeFileHandle.Invalid;
    }

    public static int Read(in NativeFileHandle handle, Span<byte> buffer, ulong offset)
    {
        fixed (byte* pBuffer = buffer)
        {
            return Read(handle, pBuffer, (nuint)buffer.Length, offset);
        }
    }

    public static int Read(in NativeFileHandle handle, void* buffer, nuint bufferSize, ulong offset)
    {
        Trace($"Read {bufferSize} bytes from file {handle} at offset {offset}");
        //NOTE(Jens): We need a way to handle big files/reads. Not a problem at the moment. 
        Debug.Assert(bufferSize < int.MaxValue);
        Debug.Assert(offset < uint.MaxValue, $"Offsets greater than {uint.MaxValue} is not supported yet.");
        uint bytesRead;
        OVERLAPPED overlapped = new()
        {
            Offset = (uint)offset
        };
        if (Kernel32.ReadFile(handle.Handle, buffer, (uint)bufferSize, &bytesRead, &overlapped))
        {
            return (int)bytesRead;
        }
        return -1;
    }

    public static int Write(in NativeFileHandle handle, ReadOnlySpan<byte> buffer)
    {
        Trace($"Write {buffer.Length} bytes to file {handle}");

        fixed (byte* pBuffer = buffer)
        {
            //NOTE(Jens): Add Overlapped when we want to write to an offset.
            uint bytesWritten;
            if (Kernel32.WriteFile(handle.Handle, pBuffer, (uint)buffer.Length, &bytesWritten, null))
            {
                return (int)bytesWritten;
            }
        }
        Logger.Error<Win32FileApi>($"Failed to write to handle {handle}");
        return -1;
    }

    public static void Close(ref NativeFileHandle handle)
    {
        Trace($"Closing file: {handle}");
        Kernel32.CloseHandle(new HANDLE { Value = handle.Handle });
    }

    public static long GetLength(in NativeFileHandle handle)
    {
        Trace($"GetLength of file: {handle}");
        LARGE_INTEGER fileSize;
        if (Kernel32.GetFileSizeEx(handle.Handle, &fileSize))
        {
            Trace($"File {handle} length: {fileSize.QuadPart} bytes.");
            return (long)fileSize.QuadPart;
        }
        return -1;
    }

    public static void Truncate(in NativeFileHandle handle)
    {
        Trace($"Truncate file: {handle}");
        Kernel32.SetFilePointerEx(handle.Handle, default, null, FileMoveMethod.FILE_BEGIN);
        Kernel32.SetEndOfFile(handle.Handle);
    }


    [Conditional("TRACE_FILE_API")]
    private static void Trace(string message) => Logger.Trace<Win32FileApi>(message);
}
