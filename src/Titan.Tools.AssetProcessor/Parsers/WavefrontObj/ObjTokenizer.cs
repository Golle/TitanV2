using System.Diagnostics;
using Titan.Core.Logging;

namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;
[DebuggerDisplay("{Line}:{Column}: {ToString(),nq}")]
internal struct Token(int line, int column)
{
    public readonly int Line = line;
    public readonly int Column = column;
    public TokenType Type;
    public string Value;


    public override string ToString()
    {
        if (Value == string.Empty)
        {
            return Type.ToString();
        }

        return $"{Type}:{Value}";
    }
}

internal enum TokenType
{
    Invalid,
    Identifier,
    Float,
    Integer,
    Slash,
    Minus,

    VertexPosition,
    VertexNormal,
    VertexTexture,
    Face,
    Smoothing,
    Group,
    UseMaterial,
    MaterialLib,
    Object,

    Comment,

}

internal static class ObjTokenizer
{
    public static ReadOnlySpan<Token> Tokenize(ReadOnlySpan<char> content)
    {
        List<Token> tokens = new(10_000);
        Cursor cursor = new(content);

        do
        {
            Token token = new(cursor.Line, cursor.Column);

            switch (cursor.Current)
            {
                case '\n':
                case '\r':
                case ' ':
                    // ignore new lines, carrier return and whitespaces.
                    continue;
                case '#':
                    Comment(ref cursor, ref token);
                    continue; // we skip comments

                case '-':
                    token.Type = TokenType.Minus;
                    token.Value = cursor.Current.ToString();
                    break;
                case '/':
                    token.Type = TokenType.Slash;
                    token.Value = cursor.Current.ToString();
                    break;
                case >= '0' and <= '9':
                    Numeric(ref cursor, ref token);
                    break;
                default:
                    Identifier(ref cursor, ref token);
                    break;
            }
            tokens.Add(token);

        } while (cursor.Advance());

        return tokens.ToArray();
    }

    private static void Numeric(ref Cursor cursor, ref Token token)
    {
        static bool IsNumeric(char c) => c is >= '0' and <= '9';
        static bool IsDot(char c) => c is '.';
        static bool IsNumberLiteral(char c) => IsNumeric(c) || IsDot(c);

        Span<char> buffer = stackalloc char[16];
        var count = 0;
        var isFloat = false;
        buffer[count++] = cursor.Current;

        while (IsNumberLiteral(cursor.Peek()))
        {

            cursor.Advance();
            buffer[count++] = cursor.Current;
            if (IsDot(cursor.Current))
            {
                isFloat = true;
            }
        }

        token.Type = isFloat ? TokenType.Float : TokenType.Integer;
        token.Value = new string(buffer[..count]);
    }

    private static void Identifier(ref Cursor cursor, ref Token token)
    {
        static bool IsValidIdentifier(char c) => c != ' ' && c != '\n';

        token.Type = TokenType.Identifier;

        const int MaxIdentifierSize = 256;
        Span<char> buffer = stackalloc char[MaxIdentifierSize];
        var length = 0;
        buffer[length++] = cursor.Current;

        while (IsValidIdentifier(cursor.Peek()))
        {
            cursor.Advance();
            buffer[length++] = cursor.Current;
        }
        (token.Type, token.Value) = IdentifierToTokenTable.Translate(buffer[..length]);
    }

    private static void Comment(ref Cursor cursor, ref Token token)
    {
        token.Type = TokenType.Comment;

        while (cursor.Current is not '\n')
        {
            if (!cursor.Advance())
            {
                // end of file reached.
                return;
            }
        }
    }
}
