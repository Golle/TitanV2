namespace Titan.Tools.AssetProcessor.Metadata.Types;

internal sealed class FontMetadata : AssetFileMetadata
{
    public const string DefaultText = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789,.;:-+*/?!%&() ";

    public string Characters { get; set; } = DefaultText;
    public char DefaultGlyph { get; set; } = '?';
    public string? Typeface { get; set; }
    public int FontSize { get; set; } = 50;
}
