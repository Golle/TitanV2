namespace Titan.Tools.AssetProcessor;
internal static class StringHelper
{
    public static string ToPropertyName(string name)
    {
        Span<char> buffer = stackalloc char[name.Length];
        var count = 0;
        if (!char.IsLetter(name[0]))
        {
            buffer[count++] = '_';
        }

        var makeUpper = true;
        foreach (var character in name)
        {
            //NOTE(Jens): add more illegal characters when we need it. 
            if (character is ' ' or '-' or '_')
            {
                makeUpper = true;
                continue;
            }
            buffer[count++] = makeUpper ? char.ToUpper(character) : character;
            makeUpper = false;
        }

        return new string(buffer[..count]);
    }
}
