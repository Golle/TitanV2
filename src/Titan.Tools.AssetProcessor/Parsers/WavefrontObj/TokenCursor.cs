namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

internal ref struct TokenCursor(ReadOnlySpan<Token> tokens)
{
    private static readonly Token Invalid = new() { Type = TokenType.Invalid };
    private readonly ReadOnlySpan<Token> _tokens = tokens;
    private readonly int _length = tokens.Length;
    private int _position;
    public ref readonly Token Current
    {
        get
        {
            if (_position < _length)
            {
                return ref _tokens[_position];
            }
            return ref Invalid;
        }
    }

    public bool Advance()
    {
        _position++;
        return _position < _length;
    }

    public ref readonly Token Peek(int offset = 1)
    {
        var index = _position + offset;
        if (index < _length)
        {
            return ref _tokens[index];
        }

        return ref Invalid;
    }

    public bool Is(TokenType type) => Current.Type == type;
    public bool IsNot(TokenType type) => Current.Type != type;
}
