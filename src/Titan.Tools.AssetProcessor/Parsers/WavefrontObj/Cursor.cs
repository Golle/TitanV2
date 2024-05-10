namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

internal ref struct Cursor(ReadOnlySpan<char> value)
{
    public const char Invalid = char.MaxValue;

    private int _position;
    private readonly int _length = value.Length;
    private readonly ReadOnlySpan<char> _value = value;

    public char Current => _position >= _value.Length ? Invalid : _value[_position];

    public int Line { get; private set; } = 1;
    public int Column { get; private set; } = 1;

    public char Peek(uint offset = 1)
    {
        var index = _position + offset;
        if (index >= _value.Length)
        {
            return Invalid;
        }
        return _value[(int)index];
    }
   
    public bool Advance()
    {
        if (_position + 1 >= _length)
        {
            return false;
        }
        var current = Current;
        if (current is '\n')
        {
            Line++;
            Column = 1;
        }
        else
        {
            Column++;
        }
        _position += 1;
        return true;
    }
}
