using System.Buffers;
using System.Runtime.InteropServices;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Core.Memory;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.UI.Resources;

namespace Titan.Tools.AssetProcessor.Processors.Textures;
internal class ImageProcessor : AssetProcessor<ImageMetadata>
{
    protected override async Task OnProcess(ImageMetadata metadata, IAssetDescriptorContext context)
    {
        await Task.Yield();
        // this makes it fail when done on multiple threads, they kill eachother.

        if (metadata.IsAseprite && metadata.Compression != CompressionType.None)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, "Compression is currently not supported for Aseprite images.");
            return;
        }

        var image = metadata.IsAseprite
            ? AsepriteReader.Read(await File.ReadAllBytesAsync(metadata.ContentFileFullPath))
            : ImageLoader.LoadAndCompress(metadata.ContentFileFullPath, metadata.Compression, context.TempFolderPath);



        //NOTE(Jens): The images I've been testing with are upside down and flipped. 
        if (image == null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process image. Id = {metadata.Id} Name = {metadata.Name} Path = {metadata.ContentFileRelativePath}");
            return;
        }

        if (image.Width < 64 || image.Height < 64)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, "The size of the image must be at least 64x64 pixels. this is due to a limitation on the GPU where a rowpitch must be 256 bytes aligned. (When we use atlas packing we don't have to worry about it, but not each texture is a single resource)");
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
        };

        var estimatedSize = image.Size * 2; // change this if we run into issues.
        var buffer = ArrayPool<byte>.Shared.Rent(estimatedSize);
        try
        {
            var writer = new TitanBinaryWriter(buffer);
            foreach (var sprite in metadata.Sprites)
            {
                var isNinePatch = sprite.NinePatch is not null;
                writer.WriteBoolAsByte(isNinePatch);
                writer.Write(new SpriteInfo
                {
                    MinX = (ushort)sprite.BottomLeft.X,
                    MinY = (ushort)(sprite.BottomLeft.Y + 1),
                    MaxX = (ushort)(sprite.TopRight.X + 1),
                    MaxY = (ushort)sprite.TopRight.Y
                });
                if (isNinePatch)
                {
                    var ninePatch = sprite.NinePatch!;
                    writer.Write(new NinePatchSpriteInfo
                    {
                        Bottom = (byte)ninePatch.Bottom,
                        Left = (byte)ninePatch.Left,
                        Right = (byte)ninePatch.Right,
                        Top = (byte)ninePatch.Top
                    });
                    spriteDescriptor.NumberOfNinePatchSprites++;
                }
                spriteDescriptor.NumberOfSprites++;
            }
            // write image bytes
            writer.WriteBytes(image.Data);
            if (!context.TryAddSprite(spriteDescriptor, writer.GetData(), metadata))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the sprite to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

}
