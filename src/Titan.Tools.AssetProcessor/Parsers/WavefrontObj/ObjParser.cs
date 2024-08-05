using System.Globalization;
using System.Numerics;
using Titan.Core.Logging;

namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

internal struct VertexIndex
{
    public int Position;
    public int Texture;
    public int Normal;
}

internal struct ObjectGroup
{
    public string? Name;
    public int FaceCount;
    public int FaceOffset;
    public int IndexOffset;
}

internal class ObjParser
{
    private ref struct ParserContext()
    {
        public readonly List<Vector3> Positions = [];
        public readonly List<Vector3> Normals = [];
        public readonly List<Vector2> Textures = [];
        public readonly List<int> FaceVertices = [];
        public readonly List<string?> FaceMaterials = [];
        public readonly List<VertexIndex> Indices = [];
        public readonly List<ObjectGroup> Objects = [];
        public readonly List<ObjectGroup> Groups = [];

        public ObjectGroup Object;
        public ObjectGroup Group;
        public string? MaterialLib;
        public string? CurrentMaterial;

        public void FlushObject()
        {
            if (Object.FaceCount > 0)
            {
                Objects.Add(Object);
            }
            Object = default;
        }

        public void FlushGroup()
        {
            if (Group.FaceCount > 0)
            {
                Groups.Add(Group);
            }
            Group = default;
        }
    }


    public static WavefrontObj Parse(ReadOnlySpan<char> content)
        => Parse(ObjTokenizer.Tokenize(content));

    public static WavefrontObj Parse(ReadOnlySpan<Token> tokens)
    {
        TokenCursor cursor = new(tokens);
        ParserContext context = new();
        do
        {
            switch (cursor.Current.Type)
            {
                case TokenType.VertexPosition:
                    context.Positions.Add(ParseVector3(ref cursor));
                    break;

                case TokenType.VertexNormal:
                    context.Normals.Add(ParseVector3(ref cursor));
                    break;

                case TokenType.VertexTexture:
                    context.Textures.Add(ParseVector2(ref cursor));
                    //NOTE(Jens): in some cases the VT has a third component which is 0.00. We skip that.
                    if (cursor.Is(TokenType.Float) && float.Parse(cursor.Current.Value, CultureInfo.InvariantCulture) == 0)
                    {
                        cursor.Advance();
                    }
                    break;

                case TokenType.Face:
                    ParseFace(ref cursor, ref context);
                    break;

                case TokenType.Group:
                    ParseGroup(ref cursor, ref context);
                    break;
                case TokenType.Object:
                    ParseObject(ref cursor, ref context);
                    break;
                case TokenType.MaterialLib:
                    ParseMaterialLib(ref cursor, ref context);
                    break;
                case TokenType.UseMaterial:
                    ParseUseMaterial(ref cursor, ref context);
                    break;
                case TokenType.Smoothing:
                    ParseSmoothing(ref cursor, ref context);
                    break;
                default:
                    Logger.Warning<ObjParser>($"Skipped token {cursor.Current.Type} Value = {cursor.Current.Value}");
                    cursor.Advance();
                    break;
            }

        } while (cursor.IsNot(TokenType.Invalid));


        context.FlushObject();
        context.FlushGroup();

        //Console.WriteLine($"Indices = {context.Indices.Count} Positions = {context.Positions.Count} Normals = {context.Normals.Count} Textures = {context.Textures.Count} ObjectS = {context.Objects.Count} Groups = {context.Groups.Count}");
        return new(
            context.MaterialLib,
            context.Positions.ToArray(),
            context.Normals.ToArray(),
            context.Textures.ToArray(),
            context.FaceVertices.ToArray(),
            context.FaceMaterials.ToArray(),
            context.Indices.ToArray(),
            context.Objects.ToArray(),
            context.Groups.ToArray()
        );
    }

    private static void ParseSmoothing(ref TokenCursor cursor, ref ParserContext context)
    {
        cursor.Advance();
        if (cursor.IsNot(TokenType.Integer) && cursor.IsNot(TokenType.Identifier))
        {
            throw new ParserException($"Expected a {nameof(TokenType.Integer)} token. Found {cursor.Current.Type}", cursor.Current);
        }

        if (cursor.Current.Type is TokenType.Integer)
        {
            var smoothing = ParseInt(ref cursor);
            //Console.WriteLine($"Smoothing is not used. Value = {smoothing}");
        }
        else
        {
            var value = cursor.Current.Value;
            cursor.Advance();
            //Console.WriteLine($"Smoothing is not used. Value = {value}");
        }
    }

    private static void ParseUseMaterial(ref TokenCursor cursor, ref ParserContext context)
    {
        cursor.Advance();
        if (cursor.IsNot(TokenType.Identifier))
        {
            throw new ParserException($"Expected a {nameof(TokenType.Identifier)} token. Found {cursor.Current.Type}", cursor.Current);
        }

        context.CurrentMaterial = cursor.Current.Value;
        cursor.Advance();
    }

    private static void ParseMaterialLib(ref TokenCursor cursor, ref ParserContext context)
    {
        if (context.MaterialLib != null)
        {
            throw new ParserException("Multiple MaterialLib tokens found.", cursor.Current);
        }

        cursor.Advance();
        if (cursor.IsNot(TokenType.Identifier))
        {
            throw new ParserException($"Expected a {nameof(TokenType.Identifier)} token. Found {cursor.Current.Type}", cursor.Current);
        }
        context.MaterialLib = cursor.Current.Value;
        cursor.Advance();
    }

    private static void ParseFace(ref TokenCursor cursor, ref ParserContext context)
    {
        static bool IsLine(ref TokenCursor cursor) => cursor.Peek().Type is not TokenType.Slash;
        //static bool IsVertexWithoutTextures(ref TokenCursor cursor) => cursor.Peek().Type is TokenType.Slash && cursor.Peek(2).Type is TokenType.Slash;
        static bool IsVertexWithTexture(ref TokenCursor cursor) => cursor.Peek().Type is TokenType.Slash && cursor.Peek(3).Type is not TokenType.Slash;

        cursor.Advance();

        var count = 0;
        if (IsLine(ref cursor))
        {
            throw new ParserException("Lines have not been implemented.", cursor.Current);
            //ParseLineFace(ref cursor, ref context);
            //var pos = ParseInt(ref cursor);

        }

        if (IsVertexWithTexture(ref cursor))
        {
            do
            {
                var pos = ParseInt(ref cursor);
                cursor.Advance();
                var tex = ParseInt(ref cursor);
                context.Indices.Add(new VertexIndex
                {
                    Normal = -1,
                    Position = pos,
                    Texture = tex
                });
                count++;
            } while (cursor.Is(TokenType.Integer));
        }
        else
        {

            do
            {
                var pos = ParseInt(ref cursor);
                cursor.Advance();
                var tex = ParseInt(ref cursor);
                cursor.Advance();
                var normal = ParseInt(ref cursor);
                context.Indices.Add(new VertexIndex
                {
                    Normal = normal,
                    Position = pos,
                    Texture = tex
                });
                count++;
            } while (cursor.Is(TokenType.Integer));
        }

        context.FaceVertices.Add(count);
        context.FaceMaterials.Add(context.CurrentMaterial);
        context.Group.FaceCount++;
        context.Object.FaceCount++;
    }

    private static int ParseInt(ref TokenCursor cursor)
    {
        if (cursor.Current.Type is not TokenType.Integer)
        {
            throw new ParserException($"Expected type {nameof(TokenType.Integer)} but found {cursor.Current.Type}", cursor.Current);
        }

        var value = int.Parse(cursor.Current.Value);
        cursor.Advance();
        return value;
    }

    private static void ParseObject(ref TokenCursor cursor, ref ParserContext context)
    {
        cursor.Advance();
        var name = cursor.Current.Value;
        cursor.Advance();

        context.FlushObject();
        context.Object = new()
        {
            FaceCount = 0,
            FaceOffset = context.FaceVertices.Count,
            IndexOffset = context.Indices.Count,
            Name = name
        };
    }

    private static void ParseGroup(ref TokenCursor cursor, ref ParserContext context)
    {
        cursor.Advance();
        var name = cursor.Current.Value;
        cursor.Advance();

        context.FlushGroup();
        context.Group = new()
        {
            FaceCount = 0,
            FaceOffset = context.FaceVertices.Count,
            IndexOffset = context.Indices.Count,
            Name = name
        };
    }

    private static Vector3 ParseVector3(ref TokenCursor cursor)
    {
        cursor.Advance();
        var x = ParseFloat(ref cursor);
        var y = ParseFloat(ref cursor);
        var z = ParseFloat(ref cursor);
        return new(x, y, z);
    }

    private static Vector2 ParseVector2(ref TokenCursor cursor)
    {
        cursor.Advance();
        var x = ParseFloat(ref cursor);
        var y = ParseFloat(ref cursor);
        return new(x, y);
    }

    private static float ParseFloat(ref TokenCursor cursor)
    {
        var sign = 1.0f;
        if (cursor.Current.Type is TokenType.Minus)
        {
            sign = -1.0f;
            cursor.Advance();
        }

        if (cursor.Current.Type is not TokenType.Float)
        {
            throw new ParserException($"Excepted a {nameof(TokenType.Float)} but found a {cursor.Current.Type}", cursor.Current);
        }

        var value = float.Parse(cursor.Current.Value, CultureInfo.InvariantCulture) * sign;
        cursor.Advance();
        return value;
    }
}
