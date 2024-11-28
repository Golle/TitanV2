using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32.DXGI;
using Titan.Tools.AssetProcessor.Processors.Textures.Aseprite;

namespace Titan.Tools.AssetProcessor.Processors.Textures;

internal class AsepriteReader
{
    public static unsafe Image? Read(ReadOnlySpan<byte> data)
    {
        var reader = new TitanBinaryReader(data);
        ref readonly var header = ref reader.Read<AsepriteHeader>();

        Logger.Trace<AsepriteReader>($"Width = {header.Width} Height = {header.Height} Depth = {header.ColorDepth} Frames = {header.Frames}");
        if (header.ColorDepth != 32)
        {
            throw new NotSupportedException($"Currently only support aseprite with RGBA, ColorDepth = 32. Got = {header.ColorDepth}");
        }
        TitanList<AsepriteLayerChunk> layerChunks = stackalloc AsepriteLayerChunk[256];
        var pixels = new byte[header.Width * header.Height * header.ColorDepth / 8];
        var mergedPixels = MemoryMarshal.Cast<byte, Pixel>(pixels);

        using var memoryStream = new MemoryStream();
        for (var i = 0; i < header.Frames; ++i)
        {
            ref readonly var frame = ref reader.Read<AsepriteFrame>();

            for (var j = 0; j < frame.NumberOfChunks; ++j)
            {
                var chunkSize = reader.Read<uint>();
                var chunkType = reader.Read<AsepriteChunkType>();
                var chunkReader = new TitanBinaryReader(reader.Read(chunkSize - 6));
                switch (chunkType)
                {
                    case AsepriteChunkType.Layer:
                        layerChunks.Add(chunkReader.Read<AsepriteLayerChunk>());
                        break;
                    case AsepriteChunkType.Cel:
                        {
                            ref readonly var cel = ref chunkReader.Read<AsepriteCelChunk>();
                            var width = chunkReader.Read<ushort>();
                            var height = chunkReader.Read<ushort>();
                            //var size = chunkSize - sizeof(AsepriteCelChunk) - sizeof(ushort) * 5;
                            ref readonly var layer = ref layerChunks[(int)cel.LayerIndex];

                            //NOTE(Jens): if the layer is not visible, we ignore it.
                            if ((layer.Flags & LayerFlags.Visible) != 0)
                            {

                                // Decompres the pixels and then merge it with the rest of the layers
                                memoryStream.Position = 0;
                                Decompress(memoryStream, chunkReader.GetRemaining());
                                var layerPixels = MemoryMarshal.Cast<byte, Pixel>(memoryStream.GetBuffer());
                                for (var y = 0; y < height; y++)
                                {
                                    for (var x = 0; x < width; x++)
                                    {
                                        var srcIndex = y * width + x;
                                        var dstIndex = (cel.Y + y) * header.Width + (cel.X + x);
                                        if (dstIndex >= 0 && dstIndex < pixels.Length)
                                        {
                                            var layerPixel = layerPixels[srcIndex];
                                            Blend(ref mergedPixels[dstIndex], layerPixel);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }

        return new Image
        {
            Width = header.Width,
            Height = header.Height,
            Format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
            BitsPerPixel = header.ColorDepth,
            Data = pixels,
            Stride = (uint)(header.Width * header.ColorDepth / 8),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Blend(ref Pixel bottom, in Pixel top)
        {
            var alpha = top.A / 255.0f;
            bottom.R = (byte)((1.0f - alpha) * bottom.R + alpha * top.R);
            bottom.G = (byte)((1.0f - alpha) * bottom.G + alpha * top.G);
            bottom.B = (byte)((1.0f - alpha) * bottom.B + alpha * top.B);
            bottom.A = (byte)((1.0f - alpha) * bottom.A + alpha * top.A);
        }

        static void Decompress(Stream stream, ReadOnlySpan<byte> data)
        {
            fixed (byte* ptr = data)
            {
                using var zlib = new ZLibStream(new UnmanagedMemoryStream(ptr, data.Length), CompressionMode.Decompress, true);
                zlib.CopyTo(stream);
            }
        }
    }

    private struct Pixel
    {
        public byte R, G, B, A;
    }
}
