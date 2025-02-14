using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32.DXGI;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Textures;
internal class ImageLoader
{
    //private static readonly WicImageReader _imageReader = new();
    private const string DirectXTexFileName = "texconv.exe";

    public static Image? LoadAndCompress(string path, CompressionType compression, string tempPath)
    {
        using var imageReader = new WicImageReader();
        var image = imageReader.LoadImage(path);
        if (image == null)
        {
            Logger.Error<ImageLoader>("Failed to load image");
            return null;
        }

        if (compression == CompressionType.None)
        {
            return image;
        }
#if DEBUG
        var textConvPath = DirectXTexFileName;
#else
        var textConvPath = Path.Combine(AppContext.BaseDirectory, DirectXTexFileName);
#endif

        if (!File.Exists(textConvPath))
        {
            throw new FileNotFoundException($"The {textConvPath} file could not be found.");
        }

        if (string.IsNullOrWhiteSpace(tempPath))
        {
            throw new InvalidOperationException("The temp path can't be null or empty.");
        }

        //NOTE(Jens): Implement more later, keep it simple now.
        const string Format = "BC7_UNORM";

        var args = $"-f {Format} -y -vflip -o \"{tempPath}\" -- \"{path}\"";

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = textConvPath,
            Arguments = args,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory
        });
        if (process == null)
        {
            throw new InvalidOperationException("Process failed.");
        }

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            Logger.Error<ImageLoader>($"Failed to convert the texture. Exit Code = {process.ExitCode}");
            return null;
        }
        var filename = Path.GetFileNameWithoutExtension(path);
        var outputFile = Path.Combine(tempPath, $"{filename}.dds");

        //NOTE(Jens): A lot of extra allocations, maybe we can do it in a better way.
        var bytes = File.ReadAllBytes(outputFile);
        var reader = new TitanBinaryReader(bytes);
        var magic = reader.Read(4);
        //TODO(Jens): add validation? in case the file is corrupt for some reason.
        ref readonly var header = ref reader.Read<DDSHeader>();
        ref readonly var dx10header = ref reader.Read<DDS_HEADER_DXT10>();

        var data = reader.GetRemaining().ToArray();
        File.Delete(outputFile);

        return image with
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM,
            BitsPerPixel = 8,
            Data = data
        };
    }

}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DDSHeader
{
    public uint Size;
    public uint Flags;
    public uint Height;
    public uint Width;
    public uint PitchOrLinearSize;
    public uint Depth;
    public uint MipMapCount;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
    public unsafe fixed uint Reserved[11];
    public DDSPixelFormat PixelFormat;
    public uint Caps;
    public uint Caps2;
    public uint Caps3;
    public uint Caps4;
    public uint Reserved2;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct DDSPixelFormat
{
    public uint Size;
    public uint Flags;
    public uint FourCC;
    public uint RGBBitCount;
    public uint RBitMask;
    public uint GBitMask;
    public uint BBitMask;
    public uint ABitMask;
}
struct DDS_HEADER_DXT10
{
    public DXGI_FORMAT dxgiFormat;
    public uint resourceDimension;
    public uint miscFlag; // See D3D11_RESOURCE_MISC_FLAG
    public uint arraySize;
    public uint reserved;
}
