namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

[Flags]
internal enum OggHeaderFlags : byte
{
    Continuation = 0x1,
    BeginningOfStream = 0x2,
    EndOfStream = 0x4
}