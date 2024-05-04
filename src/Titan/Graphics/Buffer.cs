namespace Titan.Graphics;

public struct Buffer
{
    public uint Count;
    public uint Stride;
    public BufferType Type;
    public uint Size => Count * Stride;
}