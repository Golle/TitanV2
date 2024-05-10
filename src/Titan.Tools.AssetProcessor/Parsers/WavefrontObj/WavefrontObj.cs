using System.Numerics;

namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

internal sealed class WavefrontObj(string? materialLib, Vector3[] positions, Vector3[] normals, Vector2[] textures, int[] faceVertices, string?[] faceMaterials, VertexIndex[] indices, ObjectGroup[] objects, ObjectGroup[] groups)
{
    public string? MaterialLib => materialLib;
    public ReadOnlySpan<Vector3> Positions => positions;
    public ReadOnlySpan<Vector3> Normals => normals;
    public ReadOnlySpan<Vector2> Textures => textures;
    public ReadOnlySpan<VertexIndex> Indicies => indices;
    public ReadOnlySpan<string?> FaceMaterials => faceMaterials;
    public ReadOnlySpan<int> FaceVertices => faceVertices;
    public ReadOnlySpan<ObjectGroup> Groups => groups;
    public ReadOnlySpan<ObjectGroup> Objects => objects;
}
