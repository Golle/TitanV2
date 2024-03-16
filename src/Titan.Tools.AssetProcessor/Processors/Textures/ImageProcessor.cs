using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Textures;
internal class ImageProcessor : AssetProcessor<ImageMetadata>
{
    protected override async Task OnProcess(ImageMetadata metadata, IAssetDescriptorContext context)
    {
        await Task.Run(() =>
        {
            using var imageReader = new WicImageReader();

            var image = imageReader.LoadImage(metadata.ContentFileFullPath);

            if (image != null)
            {
                var descriptor = new Texture2DDescriptor
                {
                    Width = image.Width,
                    Height = image.Height,
                    DXGIFormat = image.Format,
                    BitsPerPixel = (ushort)image.BitsPerPixel,
                    Stride = (ushort)image.Stride
                };
                if (!context.TryAddTexture2D(descriptor, image.Data, metadata))
                {
                    context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the texture to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                }
            }
            else
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process image. Id = {metadata.Id} Name = {metadata.Name} Path = {metadata.ContentFileRelativePath}");
            }
        });
    }
}
