namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

[Flags]
internal enum OggHeaderFlags : byte
{
    Continuation = 0x1,
    BeginningOfStream = 0x2,
    EndOfStream = 0x4
}