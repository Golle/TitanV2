using Titan.Core;
using Titan.Core.Logging;

namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

internal ref struct OggPageReader(ReadOnlySpan<byte> data)
{
    private TitanBinaryReader _reader = new(data);
    private OggPage _page = default;
    private byte _segment;
    private int _payloadOffset;

    public bool TryReadPayload(out ReadOnlySpan<byte> data)
    {
        data = ReadOnlySpan<byte>.Empty;
        if (_segment >= _page.PageSegments)
        {
            if (_page.HeaderType == OggHeaderFlags.EndOfStream)
            {
                //Logger.Trace("End of stream reached.", typeof(OggPageReader));
                return false;
            }
            if (!TryParseOggPage())
            {
                Logger.Error("Failed to parse the Ogg Page.", typeof(OggPageReader));
                return false;
            }
            _segment = 0;
            _payloadOffset = 0;
        }

        var payloadSize = 0;
        // move segment count forward, so we can split the packets.
        while (_segment < _page.PageSegments)
        {
            var segmentSize = _page.SegmentTable[_segment++];
            payloadSize += segmentSize;
            if (segmentSize < 255)
            {
                // end of current packet reached. an OggHeader can have multiple packets in the same header.
                break;
            }
        }

        data = _page.Payload.Slice(_payloadOffset, payloadSize);
        _payloadOffset += payloadSize;
        return true;
    }

    private bool TryParseOggPage()
    {
        var capturePattern = _reader.Read(4);
        if (!capturePattern.SequenceEqual("OggS"u8))
        {
            Logger.Error("Invalid capture pattern.", typeof(OggPageReader));
            return false;
        }

        _page.Version = _reader.Read<byte>();
        _page.HeaderType = _reader.Read<OggHeaderFlags>();
        _page.GranulePosition = _reader.Read<ulong>();
        _page.BitstreamSerialNumber = _reader.Read<uint>();
        _page.PageSequenceNumber = _reader.Read<uint>();
        _page.Checksum = _reader.Read<uint>();
        _page.PageSegments = _reader.Read<byte>();
        _page.SegmentTable = _reader.Read(_page.PageSegments);
        var payloadSize = 0;
        foreach (var segmentSize in _page.SegmentTable)
        {
            payloadSize += segmentSize;
        }

        if (_page.SegmentTable[^1] == 255)
        {
            // keep this so we can detect if we have files like this, and need to supportit.
            Logger.Error<OggReader2>("Crap!");
        }
        _page.Payload = _reader.Read(payloadSize);
        return true;
    }
}
