using System.Globalization;
using Titan.Core.Logging;
using Titan.Core.Maths;

namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

/// <summary>
/// Extremely slow parser :O but will do for now.
/// </summary>
internal static class MtlParser
{
    public static Material[] Parse(ReadOnlySpan<string> materialFile)
    {
        var materials = new Material[32];
        var first = true;
        var materialCount = 0;


        foreach (var line in materialFile)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("newmtl"))
            {
                //NOTE(Jens): Some materials does not have a name, to support this we need to keep track if it's the first or not.
                if (!first)
                {
                    materialCount++;
                }
                first = false;

                var index = line.IndexOf(' ');
                if (index != -1)
                {
                    materials[materialCount].Name = line[(index + 1)..];
                }
                continue;
            }

            ref var material = ref materials[materialCount];
            if (line.StartsWith("Ka"))
            {
                material.Ambient = ParseColor(line);
            }
            else if (line.StartsWith("Kd"))
            {
                material.Diffuse = ParseColor(line);
            }
            else if (line.StartsWith("Ks"))
            {
                material.Specular = ParseColor(line);
            }
            else if (line.StartsWith("Ke"))
            {
                material.Emissive = ParseColor(line);
            }
            else if (line.StartsWith('d') || line.StartsWith("Tr"))
            {
                material.Transparency = ParseFloat(GetValues(line));
            }
            else if (line.StartsWith("Ns"))
            {
                material.Shininess = ParseFloat(GetValues(line));
            }
            else if (line.StartsWith("Ni"))
            {
                material.Refraction = ParseFloat(GetValues(line));
            }
            else if (line.StartsWith("illum"))
            {
                material.Illumination = (MtlIllumination)int.Parse(GetValues(line));
            }
            else if (line.StartsWith("map_Ka"))
            {
                material.AmbientMap = GetValues(line).ToString();
            }
            else if (line.StartsWith("map_Ks"))
            {
                material.SpecularMap = GetValues(line).ToString();
            }
            else if (line.StartsWith("map_Bump"))
            {
                material.NormalMap = GetValues(line).ToString();
            }
            else if (line.StartsWith("map_Kd"))
            {
                material.DiffuseMap = GetValues(line).ToString();
            }
            else if (line.StartsWith("map_Disp"))
            {
                material.DisplacementMap = GetValues(line).ToString();
            }
            else if (line.StartsWith("map_d"))
            {
                material.TransparencyMap = GetValues(line).ToString();
            }
            else
            {
                Logger.Error($"Identifier is not supported. line = {line}", typeof(MtlParser));

            }
        }

        return materials[..(materialCount + 1)];

    }

    private static Color ParseColor(string line)
    {
        var values = line.Split(' ');

        var r = ParseFloat(values[1]);
        var g = ParseFloat(values[2]);
        var b = ParseFloat(values[3]);
        return new Color(r, g, b);
    }

    private static float ParseFloat(ReadOnlySpan<char> value) => float.Parse(value, CultureInfo.InvariantCulture);

    private static ReadOnlySpan<char> GetValues(string line) => line.AsSpan(line.IndexOf(' ') + 1);
}
