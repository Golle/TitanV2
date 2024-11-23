using System.Buffers;
using System.Runtime.InteropServices;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Platform.Win32.DXGI;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.UI.Resources;

namespace Titan.Tools.AssetProcessor.Processors.Textures;
internal class ImageProcessor : AssetProcessor<ImageMetadata>
{
    protected override async Task OnProcess(ImageMetadata metadata, IAssetDescriptorContext context)
    {
        await Task.Yield();
        // this makes it fail when done on multiple threads, they kill eachother.

        var image = ImageLoader.LoadAndCompress(metadata.ContentFileFullPath, metadata.Compression, context.TempFolderPath);

        //NOTE(Jens): The images I've been testing with are upside down and flipped. 
        if (image == null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process image. Id = {metadata.Id} Name = {metadata.Name} Path = {metadata.ContentFileRelativePath}");
            return;
        }

        var textureDescriptor = new Texture2DDescriptor
        {
            Width = image.Width,
            Height = image.Height,
            DXGIFormat = image.Format,
            BitsPerPixel = (ushort)image.BitsPerPixel,
            Stride = (ushort)image.Stride
        };


        // if it's a simple texture, no further processing required.
        if (metadata.Type == ImageType.Texture)
        {
            if (!context.TryAddTexture2D(textureDescriptor, image.Data, metadata))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the texture to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
            }
            return;
        }

        if (metadata.Type != ImageType.SpriteSheet)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Invalid ImageType.  Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
            return;
        }

        var spriteDescriptor = new SpriteDescriptor
        {
            Texture = textureDescriptor,
            NumberOfSprites = (byte)metadata.Sprites.Length
        };

        var totalSize = image.Data.Length + metadata.Sprites.Length * Marshal.SizeOf<SpriteInfo>();
        var buffer = ArrayPool<byte>.Shared.Rent(totalSize);
        try
        {
            var writer = new TitanBinaryWriter(buffer);
            foreach (var sprite in metadata.Sprites)
            {
                //var size = sprite.BottomRight - sprite.TopLeft;
                writer.Write(new SpriteInfo
                {
                    MinX = (ushort)sprite.BottomLeft.X,
                    MinY = (ushort)sprite.BottomLeft.Y,
                    MaxX = (ushort)sprite.TopRight.X,
                    MaxY = (ushort)sprite.TopRight.Y
                });
            }
            writer.WriteBytes(image.Data);
            if (!context.TryAddSprite(spriteDescriptor, buffer.AsSpan()[..totalSize], metadata))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the sprite to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static byte[] FlipImage(Image image)
    {
        var flippedImage = new byte[image.Data.Length];

        var stride = (int)image.Stride;
        for (var y = 0; y < image.Height; ++y)
        {
            var sourceIndex = (int)((image.Height - 1 - y) * stride);
            var destinationIndex = y * stride;
            image.Data.AsSpan(sourceIndex, stride)
                .CopyTo(flippedImage.AsSpan(destinationIndex, stride));
        }

        return flippedImage;
    }
}
