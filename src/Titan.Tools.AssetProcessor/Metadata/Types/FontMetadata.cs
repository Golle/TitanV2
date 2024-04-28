namespace Titan.Tools.AssetProcessor.Metadata.Types;

internal sealed class FontMetadata : AssetFileMetadata
{
    public const string DefaultText = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789,.;:-+*/?!%&()";

    public string Characters { get; set; } = DefaultText;
    public string? Typeface { get; set; }
}
