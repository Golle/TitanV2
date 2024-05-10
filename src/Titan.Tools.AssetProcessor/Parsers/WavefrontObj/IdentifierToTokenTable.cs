namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

internal static class IdentifierToTokenTable
{
    private const string VertexPosition = "v";
    private const string VertexNormal = "vn";
    private const string VertexTexture = "vt";
    private const string Face = "f";
    private const string Smoothing = "s";
    private const string MaterialLib = "mtllib";
    private const string UseMaterial= "usemtl";
    private const string Object = "o";
    private const string Group = "g";
    public static (TokenType Type, string Value) Translate(ReadOnlySpan<char> buffer)
    {
        static bool IsMatch(string value, ReadOnlySpan<char> identifier) => value.AsSpan().Equals(identifier, StringComparison.Ordinal);

        if (IsMatch(VertexPosition, buffer))
        {
            return (TokenType.VertexPosition, VertexPosition);
        }

        if (IsMatch(VertexNormal, buffer))
        {
            return (TokenType.VertexNormal, VertexNormal);
        }

        if (IsMatch(VertexTexture, buffer))
        {
            return (TokenType.VertexTexture, VertexTexture);
        }

        if (IsMatch(Face, buffer))
        {
            return (TokenType.Face, Face);
        }

        if (IsMatch(Smoothing, buffer))
        {
            return (TokenType.Smoothing, Smoothing);
        }
        if (IsMatch(MaterialLib, buffer))
        {
            return (TokenType.MaterialLib, MaterialLib);
        }
        if (IsMatch(UseMaterial, buffer))
        {
            return (TokenType.UseMaterial, UseMaterial);
        }
        if (IsMatch(Object, buffer))
        {
            return (TokenType.Object, Object);
        }
        if (IsMatch(Group, buffer))
        {
            return (TokenType.Group, Group);
        }

        return (TokenType.Identifier, buffer.ToString());
    }
}