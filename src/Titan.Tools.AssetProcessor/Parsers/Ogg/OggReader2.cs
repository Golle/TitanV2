using System.Runtime.InteropServices;
using System.Text;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;

namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

internal unsafe class OggReader2
{
    public static void Read(ReadOnlySpan<byte> file, uint bufferSize = 0)
    {
        bufferSize = bufferSize == 0 ? MemoryUtils.MegaBytes(256) : bufferSize;
        var mem = NativeMemory.AllocZeroed(bufferSize);
        if (mem == null)
        {
            throw new OutOfMemoryException($"Failed to allocate a buffer. Allocation size = {bufferSize} bytes.");
        }

        var allocator = new BumpAllocator((byte*)mem, bufferSize);

        //var reader = new TitanBinaryReader(file);
        var reader = new OggPageReader(file);

        Logger.Trace<OggReader2>($"Parsing ogg file. Size = {file.Length} bytes");

        if (!TryParseVorbisHeader(ref reader, out var vorbisHeader))
        {
            Logger.Error<OggReader2>($"Failed to parse the {nameof(VorbisHeader)}");
            return;
        }

        if (!TryParseVorbisMetadata(ref reader, out var vorbisMetadata))
        {
            Logger.Error<OggReader2>("Failed to parse the VorbisMetadata");
            return;
        }

        if (!TryParseVorbisSetup(ref reader, ref allocator, vorbisHeader, out var vorbisSetup))
        {

            Logger.Error<OggReader2>("Failed to parse the VorbisSetup");
            return;
        }

        while (reader.TryReadPayload(out var payloadPage))
        {


        }
        Logger.Trace<OggReader2>($"Finished parsing ogg file.");

    }

    private static bool TryParseVorbisHeader(ref OggPageReader reader, out VorbisHeader header)
    {
        header = default;
        if (!reader.TryReadPayload(out var payload))
        {
            Logger.Error<OggReader2>($"Failed to read the payload for the {nameof(VorbisHeader)}");
            return false;
        }
        var headerReader = new TitanBinaryReader(payload);
        if (!ReadAndValidateVorbisSignature(ref headerReader, 0x01))
        {
            Logger.Error<OggReader2>($"Failed to validate Vorbis Signature.");
            return false;
        }

        header.Version = headerReader.Read<uint>();
        header.Channels = headerReader.Read<byte>();
        header.SampleRate = headerReader.Read<uint>();
        header.BitRateMax = headerReader.Read<uint>();
        header.BitRateNominal = headerReader.Read<uint>();
        header.BitRateMin = headerReader.Read<uint>();
        var blockSize = headerReader.Read<byte>();
        header.BlockSize0 = (byte)(1 << (blockSize & 0xf));
        header.BlockSize1 = (byte)(1 << (blockSize >> 4) & 0xf);
        header.Framing = headerReader.Read<byte>();

        return true;
    }

    private static bool TryParseVorbisMetadata(ref OggPageReader reader, out VorbisCommentMetadata metadata)
    {
        metadata = default;

        if (!reader.TryReadPayload(out var payload))
        {
            Logger.Error<OggReader2>($"Failed to read the payload for the {nameof(VorbisCommentMetadata)}");
            return false;
        }
        var headerReader = new TitanBinaryReader(payload);
        if (!ReadAndValidateVorbisSignature(ref headerReader, 0x03))
        {
            Logger.Error<OggReader2>($"Failed to validate Vorbis Signature.");
            return false;
        }

        var vendorLength = headerReader.Read<uint>();
        metadata.Vendor = headerReader.Read(vendorLength);
        var commentListCount = headerReader.Read<uint>();
        Logger.Trace<OggReader2>($"Vendor: {Encoding.ASCII.GetString(metadata.Vendor)}");
        for (var i = 0; i < commentListCount; ++i)
        {
            //NOTE(Jens): If we want this, we can add a function to the metadata and store this as a readonlyspan. GetComment(int index)
            var length = headerReader.Read<uint>();
            var comment = headerReader.Read(length);
            Logger.Trace<OggReader2>($"Comment[{i}]: {Encoding.ASCII.GetString(comment)}");
        }

        var framingBit = headerReader.Read<byte>();
        if (framingBit != 1)
        {
            Logger.Error<OggReader2>($"Framing bit is invalid. Expected = 1 Got = {framingBit}");
            return false;
        }
        return true;
    }

    private static bool TryParseVorbisSetup(ref OggPageReader reader, ref BumpAllocator allocator, in VorbisHeader header, out VorbisSetup setup)
    {
        setup = default;
        if (!reader.TryReadPayload(out var payload))
        {
            Logger.Error<OggReader2>($"Failed to read the payload for the {nameof(VorbisSetup)}");
            return false;
        }
        var headerReader = new TitanBinaryReader(payload);
        if (!ReadAndValidateVorbisSignature(ref headerReader, 0x05))
        {
            Logger.Error<OggReader2>("Failed to validate Vorbis Signature.");
            return false;
        }

        var bitreader = new TitanBitReader(headerReader.GetRemaining());

        // Codebooks
        var codebookCount = bitreader.ReadBits(8) + 1;
        setup.Codebooks = allocator.AllocateArray<VorbisCodebook>(codebookCount);

        for (var i = 0; i < codebookCount; ++i)
        {
            var pattern = bitreader.ReadBits(24);
            if (pattern != 0x564342)// ASCI for VCB
            {
                Logger.Error<OggReader2>("Failed to validate code book pattern.");
                return false;
            }

            if (!TryParseVorbisCodebook(ref bitreader, ref allocator, out setup.Codebooks[i]))
            {
                Logger.Error<OggReader2>($"failed to parse codebook at index = {i}");
                return false;
            }
        }

        // times are not used by vorbis, just skip
        var times = bitreader.ReadBits(6) + 1;
        bitreader.SkipBits(times * 16);

        // Floor configuration
        var floorCount = bitreader.ReadBits(6) + 1;
        setup.FloorConfig = allocator.AllocateArray<VorbisFloorConfig>(floorCount);
        for (var i = 0; i < floorCount; ++i)
        {
            if (!TryParseFloorConfig(ref bitreader, ref allocator, out setup.FloorConfig[i]))
            {
                Logger.Error<OggReader2>($"Failed to parse {nameof(VorbisFloorConfig)} at index {i}");
                return false;
            }
        }

        // Residue
        var residueCount = bitreader.ReadBits(6) + 1;
        setup.ResidueConfig = allocator.AllocateArray<VorbisResidueConfig>(residueCount);

        for (var i = 0; i < residueCount; ++i)
        {
            if (!TryParseResidueConfig(ref bitreader, ref allocator, setup.Codebooks, out setup.ResidueConfig[i]))
            {
                Logger.Error<OggReader2>($"Failed to parse {nameof(VorbisResidueConfig)} at index {i}");
                return false;
            }
        }

        // Mapping

        var mappingCount = bitreader.ReadBits(6) + 1;
        setup.Mappings = allocator.AllocateArray<VorbisMapping>(mappingCount);
        for (var i = 0; i < mappingCount; ++i)
        {
            if (!TryParseMapping(ref bitreader, ref allocator, header.Channels, residueCount, floorCount, out setup.Mappings[i]))
            {
                Logger.Error<OggReader2>($"Failed to parse mappings at index {i}");
                return false;
            }
        }


        var modes = bitreader.ReadBits(6) + 1;
        setup.ModeConfig = allocator.AllocateArray<VorbisModeConfig>(modes);
        for (var i = 0; i < modes; ++i)
        {

            if (!TryParseModeConfig(ref bitreader, mappingCount, out setup.ModeConfig[i]))
            {
                Logger.Error<OggReader2>($"Failed to parse mode config at index {i}");
                return false;
            }
        }


        return true;
    }

    private static bool TryParseModeConfig(ref TitanBitReader reader, int mappingCount, out VorbisModeConfig config)
    {
        config = default;

        config.BlockFlag = reader.ReadBitAsBool();
        config.WindowType = (ushort)reader.ReadBits(16);
        config.TransformType = (ushort)reader.ReadBits(16);
        config.Mapping = (byte)reader.ReadBits(8);

        if (config.WindowType != 0)
        {
            Logger.Error<OggReader2>($"Invalid WindowType, expected 0. Got {config.WindowType}.");
            return false;
        }

        if (config.TransformType != 0)
        {
            Logger.Error<OggReader2>($"Invalid TransformType, expected 0. Got {config.TransformType}.");
            return false;
        }

        if (config.Mapping >= mappingCount)
        {
            Logger.Error<OggReader2>($"Invalid Mapping, expected less than MappingCount. Mapping Count = {mappingCount}, Mapping = {config.Mapping}");
            return false;
        }

        return true;
    }

    private static bool TryParseMapping(ref TitanBitReader reader, ref BumpAllocator allocator, byte channels, int residueCount, int floorCount, out VorbisMapping mapping)
    {
        mapping = default;
        var type = reader.ReadBits(16);
        if (type != 0)
        {
            Logger.Error<OggReader2>($"The mapping type is invalid. Expected 0, got {type}");
            return false;
        }

        mapping.Channels = allocator.AllocateArray<VorbisMappingChannel>(channels);
        mapping.Submaps = (byte)(reader.ReadBitAsBool() ? reader.ReadBits(4) + 1 : 1);

        if (reader.ReadBitAsBool())
        {
            mapping.CouplingSteps = reader.ReadBits(8) + 1;
            if (mapping.CouplingSteps > channels)
            {
                Logger.Error<OggReader2>($"Invalid value for {nameof(VorbisMapping.CouplingSteps)}. Expected to be less or equals to the number of channels. Channels = {channels} Coupling Steps = {mapping.CouplingSteps}");
                return false;
            }

            var bitsToRead = ilog(channels - 1);
            for (var i = 0; i < mapping.CouplingSteps; ++i)
            {
                mapping.Channels[i].Magnitude = (byte)reader.ReadBits(bitsToRead);
                mapping.Channels[i].Angle = (byte)reader.ReadBits(bitsToRead);

                if (mapping.Channels[i].Magnitude >= channels)
                {
                    Logger.Error<OggReader2>($"Magnitude is greater than number of channels. Channels = {channels}, Magnitude = {mapping.Channels[i].Magnitude}, Index = {i}");
                    return false;
                }

                if (mapping.Channels[i].Angle >= channels)
                {
                    Logger.Error<OggReader2>($"Angle is greater than number of channels. Channels = {channels}, Angle = {mapping.Channels[i].Angle}, Index = {i}");
                    return false;
                }

                if (mapping.Channels[i].Magnitude == mapping.Channels[i].Angle)
                {
                    Logger.Error<OggReader2>($"Angle and Magnitude are the same. Magnitude = {mapping.Channels[i].Magnitude}, Angle = {mapping.Channels[i].Angle}, Index = {i}");
                    return false;
                }
            }
        }
        else
        {
            mapping.CouplingSteps = 0;
        }

        var reserved = reader.ReadBits(2);
        if (reserved != 0)
        {
            Logger.Error<OggReader2>($"Reserved bits should be 0. Value = {reserved}");
            return false;
        }

        if (mapping.Submaps > 1)
        {
            for (var i = 0; i < channels; ++i)
            {
                mapping.Channels[i].Mux = (byte)reader.ReadBits(4);
                if (mapping.Channels[i].Mux >= mapping.Submaps)
                {
                    Logger.Error<OggReader2>($"Invalid value for Mux. Expected to be less or equal to submaps. Mux = {mapping.Channels[i].Mux}, Submaps = {mapping.Submaps}, Index = {i}");
                    return false;
                }
            }
        }
        else
        {
            for (var i = 0; i < channels; ++i)
            {
                mapping.Channels[i].Mux = 0;
            }
        }

        mapping.SubmapFloors = allocator.AllocateArray<byte>(mapping.Submaps);
        mapping.SubmapResidues = allocator.AllocateArray<byte>(mapping.Submaps);
        for (var i = 0; i < mapping.Submaps; ++i)
        {
            reader.ReadBits(8);
            mapping.SubmapFloors[i] = (byte)reader.ReadBits(8);
            mapping.SubmapResidues[i] = (byte)reader.ReadBits(8);

            if (mapping.SubmapFloors[i] >= floorCount)
            {
                Logger.Error<OggReader2>($"The SubmapFloor is greater or equal to floor count. SubmapFloors = {mapping.SubmapFloors[i]} FloorCount = {floorCount} Index = {i}");
                return false;
            }

            if (mapping.SubmapResidues[i] >= residueCount)
            {
                Logger.Error<OggReader2>($"The SubmapResidue is greater or equal to floor count. SubmapResidue = {mapping.SubmapResidues[i]} FloorCount = {floorCount} Index = {i}");
                return false;
            }
        }
        return true;
    }

    private static bool TryParseVorbisCodebook(ref TitanBitReader reader, ref BumpAllocator allocator, out VorbisCodebook codebook)
    {
        codebook = default;

        var dimensions = reader.ReadBits(16);
        var entries = reader.ReadBits(24);

        if (dimensions <= 0)
        {
            Logger.Error<OggReader2>($"Dimensions have an invalid value. {dimensions}");
            return false;
        }

        if (entries <= 0)
        {
            Logger.Error<OggReader2>($"Entries have an invalid value. {entries}");
            return false;
        }

        var codewordLengths = allocator.AllocateArray<int>(entries);
        var isOrdered = reader.ReadBitAsBool();

        var maxLength = 0;
        if (isOrdered)
        {
            var length = reader.ReadBits(5) + 1;
            for (var i = 0; i < entries;)
            {
                var count = reader.ReadBits(ilog(entries - i));
                while (--count >= 0)
                {
                    codewordLengths[i++] = length;
                }

                ++length;
            }

            maxLength = length;
        }
        else
        {
            var isSparse = reader.ReadBitAsBool();
            for (var i = 0; i < entries; ++i)
            {
                //NOTE(Jens): this logic is a bit weird, i'm not sure if its correct.
                var hasCodeword = !isSparse || reader.ReadBitAsBool();
                codewordLengths[i] = hasCodeword ? reader.ReadBits(5) + 1 : -1;
                maxLength = Math.Max(maxLength, codewordLengths[i]);
            }
        }

        if (!TryParseLookupTable(ref reader, ref allocator, entries, dimensions, out codebook.LookupTable))
        {
            Logger.Error<OggReader2>("Failed to parse the lookup table.");
            return false;
        }
        codebook.CodewordsLengths = codewordLengths;
        codebook.MaxLength = maxLength;

        return true;


    }

    private static bool TryParseFloorConfig(ref TitanBitReader reader, ref BumpAllocator allocator, out VorbisFloorConfig config)
    {
        config = default;
        config.Type = reader.ReadBits(16);
        switch (config.Type)
        {
            case 0:

                return TryParseFloorConfig0(ref reader, ref allocator, out config.Config0);
            case 1:
                return TryParseFloorConfig1(ref reader, ref allocator, out config.Config1);

        }
        Logger.Error<OggReader2>($"The floor config type {config.Type} is not supported.");
        return false;

    }

    private static bool TryParseFloorConfig0(ref TitanBitReader reader, ref BumpAllocator allocator, out VorbisFloorConfig0 config)
    {
        throw new NotImplementedException();
    }

    private static bool TryParseFloorConfig1(ref TitanBitReader reader, ref BumpAllocator allocator, out VorbisFloorConfig1 config)
    {
        config = default;

        var partitions = reader.ReadBits(5);
        config.PartitionClassList = allocator.AllocateArray<byte>(partitions);
        var maxClass = -1;
        for (var i = 0; i < partitions; ++i)
        {
            var value = (byte)reader.ReadBits(4);
            config.PartitionClassList[i] = value;
            maxClass = Math.Max(maxClass, value);
        }

        maxClass++;

        config.SubclassBooks = allocator.AllocateArray<TitanArray<short>>(maxClass);
        config.ClassDimensions = allocator.AllocateArray<byte>(maxClass);
        config.Subclasses = allocator.AllocateArray<byte>(maxClass);
        config.Masterbooks = allocator.AllocateArray<byte>(maxClass);

        for (var i = 0; i < maxClass; ++i)
        {
            config.ClassDimensions[i] = (byte)(reader.ReadBits(3) + 1);
            config.Subclasses[i] = (byte)reader.ReadBits(2);
            config.Masterbooks[i] = (byte)(config.Subclasses[i] > 0 ? reader.ReadBits(8) : 0);

            var count = 1 << config.Subclasses[i];
            config.SubclassBooks[i] = allocator.AllocateArray<short>(count);
            for (var j = 0; j < count; ++j)
            {
                config.SubclassBooks[i][j] = (short)(reader.ReadBits(8) - 1);
            }
        }
        // see https://github.com/nothings/stb/blob/2e2bef463a5b53ddf8bb788e25da6b8506314c08/stb_vorbis.c#L3970C14-L3973C14
        // https://github.com/NVorbis/NVorbis/blob/519d4e2aae7d6a4d5bab552ec5c1e517e9c78855/NVorbis/Floor1.cs


        config.Multiplier = (byte)(reader.ReadBits(2) + 1);
        config.RangeBits = (byte)reader.ReadBits(4);
        var floorValues = CalculateFloorValuesCount(config);
        config.XList = allocator.AllocateArray<int>(floorValues + 2);
        config.XList[0] = 0;
        config.XList[1] = 1 << config.RangeBits;
        for (var i = 0; i < floorValues; ++i)
        {
            config.XList[i + 2] = reader.ReadBits(config.RangeBits);
        }

        return true;

        static int CalculateFloorValuesCount(in VorbisFloorConfig1 config)
        {
            var total = 0;
            foreach (var value in config.PartitionClassList.AsReadOnlySpan())
            {
                total += config.ClassDimensions[value];
            }

            return total;
        }
    }

    private static bool TryParseLookupTable(ref TitanBitReader reader, ref BumpAllocator allocator, int entries, int dimensions, out VorbisLookupTable table)
    {
        table = default;
        table.Type = reader.ReadBits(4);
        if (table.Type is 1 or 2)
        {
            table.MinValue = BitConverter.Int32BitsToSingle(reader.ReadBits(32));
            table.MaxValue = BitConverter.Int32BitsToSingle(reader.ReadBits(32));
            var valueBits = reader.ReadBits(4) + 1;

            //NOTE(Jens):  not sure what it's used for.
            table.SequenceP = reader.ReadBitAsBool();

            var lookupValues = CalculateLookupValues(table.Type, entries, dimensions);
            table.LookupValues = allocator.AllocateArray<uint>(lookupValues);

            // 6b. Read lookup table values
            for (var i = 0; i < lookupValues; i++)
            {
                table.LookupValues[i] = (uint)reader.ReadBits(valueBits);
            }
        }

        return true;

        static int CalculateLookupValues(int lookupType, int entries, int dimensions)
        {
            if (lookupType != 1)
            {
                return entries * dimensions;
            }
            //NOTE(Jens): I'm not sure why this is needed, investigate later
            var r = (int)Math.Floor(Math.Exp(Math.Log(entries) / dimensions));
            if (Math.Floor(Math.Pow(r + 1, dimensions)) <= entries)
            {
                ++r;
            }
            return r;
        }
    }

    private static bool TryParseResidueConfig(ref TitanBitReader reader, ref BumpAllocator allocator, ReadOnlySpan<VorbisCodebook> codebooks, out VorbisResidueConfig config)
    {
        config = default;

        config.Type = (short)reader.ReadBits(16);
        if (config.Type > 2)
        {
            Logger.Error<OggReader2>($"Invalid value for Type. Expected Type < 2, Value = {config.Type}");
            return false;
        }
        config.Begin = reader.ReadBits(24);
        config.End = reader.ReadBits(24);
        if (config.Begin > config.End)
        {
            Logger.Error<OggReader2>($"Invalid value for Beign and End. Begin > End. Begin = {config.Begin} End = {config.End}");
            return false;
        }

        config.PartitionSize = reader.ReadBits(24) + 1;
        config.Classifications = (byte)(reader.ReadBits(6) + 1);
        config.Classbook = (byte)reader.ReadBits(8);
        if (config.Classbook > codebooks.Length)
        {
            Logger.Error<OggReader2>($"Invalid configuration for Classbooks, the index is greater than the number of Codebooks in setup. Value = {config.Classbook}, number of codebooks = {codebooks.Length}");
            return false;
        }


        Span<byte> cascade = stackalloc byte[64];
        for (var i = 0; i < config.Classifications; ++i)
        {
            byte highBits = 0;
            var lowBits = (byte)reader.ReadBits(3);
            if (reader.ReadBitAsBool())
            {
                highBits = (byte)reader.ReadBits(5);
            }

            cascade[i] = (byte)(highBits * 8 + lowBits);
        }

        config.ResidueBooks = allocator.AllocateArray<Inline8<short>>(config.Classifications);
        for (var i = 0; i < config.Classifications; ++i)
        {
            for (var k = 0; k < 8; ++k)
            {
                if ((cascade[i] & (1 << k)) != 0)
                {
                    config.ResidueBooks[i][k] = (short)reader.ReadBits(8);
                    if (config.ResidueBooks[i][k] >= codebooks.Length)
                    {
                        Logger.Error<OggReader2>($"The index for the ResidueBook is greater than the number of codebooks. Index = {config.ResidueBooks[i][k]}, Codebook Count = {codebooks.Length}");
                        return false;
                    }
                }
                else
                {
                    config.ResidueBooks[i][k] = -1;
                }
            }
        }

        return true;
    }

    private static bool ReadAndValidateVorbisSignature(ref TitanBinaryReader reader, byte packetType)
    {
        var type = reader.Read<byte>();
        if (type != packetType)
        {
            Logger.Error<OggReader2>($"The packet type is wrong. Expected = {packetType} Got = {type}");
        }
        var vorbis = reader.Read(6);
        if (!vorbis.SequenceEqual("vorbis"u8))
        {
            Logger.Error<OggReader2>($"The vorbis string did not match. Got = '{Encoding.ASCII.GetString(vorbis)}'");
            return false;
        }
        return true;
    }

    private static int ilog(int value)
    {
        //NOTE(Jens): I'm not sure why this is needed, investigate later
        var bits = 0;
        while (value > 0)
        {
            bits++;
            value >>= 1;
        }
        return bits;
    }
}
