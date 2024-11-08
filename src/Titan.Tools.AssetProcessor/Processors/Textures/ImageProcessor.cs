using Titan.Assets.Types;
using Titan.Tools.AssetProcessor.Metadata.Types;

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

        if (image != null)
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

            var descriptor = new Texture2DDescriptor
            {
                Width = image.Width,
                Height = image.Height,
                DXGIFormat = image.Format,
                BitsPerPixel = (ushort)image.BitsPerPixel,
                Stride = (ushort)image.Stride
            };
            if (!context.TryAddTexture2D(descriptor, flippedImage, metadata))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the texture to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
            }
        }
        else
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process image. Id = {metadata.Id} Name = {metadata.Name} Path = {metadata.ContentFileRelativePath}");
        }
    }
}
