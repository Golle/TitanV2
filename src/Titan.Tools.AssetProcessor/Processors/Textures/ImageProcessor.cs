using System.Buffers;
using System.Runtime.InteropServices;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.UI;

namespace Titan.Tools.AssetProcessor.Processors.Textures;
internal class ImageProcessor : AssetProcessor<ImageMetadata>
{
    //NOTE(Jens): This will leak resources, figure out a better way to create the image reader.
    private readonly WicImageReader _imageReader = new();
    protected override async Task OnProcess(ImageMetadata metadata, IAssetDescriptorContext context)
    {
        await Task.Yield();
        // this makes it fail when done on multiple threads, they kill eachother.
        //using var imageReader = new WicImageReader();

        var image = _imageReader.LoadImage(metadata.ContentFileFullPath);

        //NOTE(Jens): The images I've been testing with are upside down and flipped. 
        if (image == null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process image. Id = {metadata.Id} Name = {metadata.Name} Path = {metadata.ContentFileRelativePath}");
            return;
        }
        var flippedImage = FlipImage(image);

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
            if (!context.TryAddTexture2D(textureDescriptor, flippedImage, metadata))
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



        var totalSize = flippedImage.Length + metadata.Sprites.Length * Marshal.SizeOf<SpriteInfo>();
        var buffer = ArrayPool<byte>.Shared.Rent(totalSize);
        try
        {
            var writer = new TitanBinaryWriter(buffer);
            foreach (var sprite in metadata.Sprites)
            {
                writer.Write(new SpriteInfo
                {
                    Width = (ushort)sprite.Size.Width,
                    Height = (ushort)sprite.Size.Height,
                    X = (ushort)sprite.Position.X,
                    Y = (ushort)sprite.Position.Y
                });
            }
            writer.WriteBytes(flippedImage);
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
