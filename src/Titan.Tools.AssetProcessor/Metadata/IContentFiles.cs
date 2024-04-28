namespace Titan.Tools.AssetProcessor.Metadata;

internal interface IContentFiles
{
    Task<bool> VerifyMetadataFiles();
    Task<AssetFileMetadata[]?> GetFiles();
}
