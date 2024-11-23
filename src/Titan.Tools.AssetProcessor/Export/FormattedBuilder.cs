using System.Text;

namespace Titan.Tools.AssetProcessor.Export;

internal class FormattedBuilder
{
    private readonly StringBuilder _builder = new();
    private int _indentation;

    public FormattedBuilder EndIndentation(int count = 1)
    {
        _indentation -= (count * 4);
        return this;
    }

    public FormattedBuilder BeginScope()
    {
        AppendLine("{");
        BeginIndentation();
        return this;
    }

    public FormattedBuilder EndScope()
    {
        EndIndentation();
        AppendLine("}");
        return this;
    }

    public FormattedBuilder BeginIndentation(int count = 1)
    {
        _indentation += (count * 4);
        return this;
    }

    public FormattedBuilder AppendLine(string? line = null)
    {
        if (_indentation > 0)
        {
            _builder.Append(' ', _indentation);
        }
        _builder.AppendLine(line);
        return this;
    }

    public override string ToString()
        => _builder.ToString();
}
