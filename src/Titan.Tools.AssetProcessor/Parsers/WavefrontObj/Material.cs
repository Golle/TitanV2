using Titan.Core.Maths;

namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

internal struct Material
{
    public string? Name;
    public Color Ambient;
    public Color Diffuse;
    public Color Specular;
    public Color Emissive;
    public float Transparency;
    public float Shininess;
    public float Refraction;
    public string? AmbientMap;
    public string? DiffuseMap;
    public string? SpecularMap;
    public string? NormalMap;
    public string? DisplacementMap;
    public string? TransparencyMap;
    public MtlIllumination Illumination;
}
