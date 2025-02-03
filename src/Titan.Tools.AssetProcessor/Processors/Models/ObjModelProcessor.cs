using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Assets.Types;
using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

namespace Titan.Tools.AssetProcessor.Processors.Models;
internal class ObjModelProcessor : AssetProcessor<ObjModelMetadata>
{
    protected override async Task OnProcess(ObjModelMetadata metadata, IAssetDescriptorContext context)
    {
        try
        {
            //TODO(Jens): Implement submesh/material compression, so that we can draw all submeshes that share a material in a single call.
            var lines = await File.ReadAllTextAsync(metadata.ContentFileFullPath);
            var wavefrontObj = ObjParser.Parse(lines);

            //NOTE(Jens): We've got some assumptions that we'll stick to for now to keep it simple.
            if (wavefrontObj.Objects.Length > 1 || wavefrontObj.Groups.Length > 1)
            {
                throw new NotImplementedException("Multiple groups or objects has not been implemented yet.");
            }

            if (wavefrontObj.MaterialLib == null)
            {
                throw new NotImplementedException("Obj file without a mtl file has not been implemented.");
            }
            var materialMetadata = context
                .GetMetadataByFilename(wavefrontObj.MaterialLib)
                .FirstOrDefault();
            if (materialMetadata == null)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to find material lib file. Path = {metadata.ContentFileRelativePath}, Material Lib = {wavefrontObj.MaterialLib}");
                return;
            }

            // Get the material names and sort them by the same order as the MtlProcessor. So we can get the correct index of the material.
            var materialNames = MtlParser.Parse(await File.ReadAllLinesAsync(materialMetadata.ContentFileFullPath))
                .Select(m => m.Name)
                .ToArray();

            Array.Sort(materialNames, string.CompareOrdinal);
            
            metadata.Dependencies = new List<AssetFileMetadata> { materialMetadata };

            HandleObject(wavefrontObj.Objects[0]);

            void HandleObject(ObjectGroup obj)
            {
                var modelIndices = wavefrontObj.Indicies[obj.IndexOffset..];
                var faceMaterials = wavefrontObj.FaceMaterials[obj.FaceOffset..obj.FaceCount];
                var faceVertices = wavefrontObj.FaceVertices[obj.FaceOffset..obj.FaceCount];

                List<Vertex> vertices = new();
                List<SubMesh> meshes = new();
                List<uint> indices = new();

                var currentMaterial = faceMaterials[0]!;
                var materialIndex = Array.IndexOf(materialNames, currentMaterial);
                if (materialIndex == -1)
                {
                    context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to find material {currentMaterial}. Material file = {materialMetadata.ContentFileRelativePath} Obj file = {metadata.ContentFileRelativePath}");
                    return;
                }
                var indexOffset = 0;
                var vertexOffset = 0;
                for (var i = 0; i < obj.FaceCount; ++i)
                {
                    if (currentMaterial != faceMaterials[i])
                    {
                        currentMaterial = faceMaterials[i]!;
                        meshes.Add(new SubMesh
                        {
                            VertexCount = vertices.Count - vertexOffset,
                            VertexOffset = vertexOffset,
                            IndexCount = vertices.Count - vertexOffset,
                            IndexOffset = vertexOffset,
                            MaterialIndex = materialIndex
                        });
                        vertexOffset = vertices.Count;

                        materialIndex = Array.IndexOf(materialNames, currentMaterial);
                        if (materialIndex == -1)
                        {
                            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to find material {currentMaterial}. Material file = {materialMetadata.ContentFileRelativePath} Obj file = {metadata.ContentFileRelativePath}");
                            return;
                        }
                    }

                    var vertexCount = faceVertices[i];
                    var firstVertex = IndexToVertex(modelIndices[indexOffset], wavefrontObj);
                    for (var j = 1; j < vertexCount - 1; ++j)
                    {
                        var secondVertex = IndexToVertex(modelIndices[indexOffset + j], wavefrontObj);
                        var thirdVertex = IndexToVertex(modelIndices[indexOffset + j + 1], wavefrontObj);

                        //NOTE(Jens): This will add the same vertices several times. This is fine for the first version, but in the future we want to compress this and use indices.

                        vertices.Add(firstVertex);
                        vertices.Add(thirdVertex); //OBJ file has the wrong order (need to verify this). So we swap third and second to make it compatible with D3D12
                        vertices.Add(secondVertex);
                    }

                    indexOffset += vertexCount;
                }

                //NOTE(Jens): If there are no normals, we calculate them.
                if (wavefrontObj.Normals.IsEmpty)
                {
                    if (vertices.Count % 3 != 0)
                    {
                        context.AddDiagnostics(DiagnosticsLevel.Error, "The number of vertices is not a multiple of 3. This is not supported at the moment.");
                        return;
                    }

                    var verticesSpan = CollectionsMarshal.AsSpan(vertices);
                    for (var i = 0; i < verticesSpan.Length; i += 3)
                    {
                        CalculateAndSetFaceNormals(verticesSpan.Slice(i, 3));
                    }
                }
                meshes.Add(new SubMesh
                {
                    VertexCount = vertices.Count - vertexOffset,
                    VertexOffset = vertexOffset,
                    IndexCount = vertices.Count - vertexOffset,
                    IndexOffset = vertexOffset,
                    MaterialIndex = materialIndex
                });

                for (var i = 0u; i < vertices.Count; ++i)
                {
                    indices.Add(i);
                }

                using var stream = new MemoryStream();
                stream.Write(MemoryMarshal.AsBytes<SubMesh>(meshes.ToArray()));
                stream.Write(MemoryMarshal.AsBytes<Vertex>(vertices.ToArray()));
                stream.Write(MemoryMarshal.AsBytes<uint>(indices.ToArray()));

                if (!context.TryAddMesh(new MeshDescriptor
                {
                    IndexCount = (uint)indices.Count,
                    MaterialCount = -1,
                    SubMeshCount = (uint)meshes.Count,
                    VertexCount = (uint)vertices.Count,
                }, stream.ToArray(), metadata))
                {
                    Logger.Error<ObjModelProcessor>("Failed to add mesh.");
                }
            }
        }
        catch (ParserException e)
        {
            Logger.Error<ObjModelProcessor>($"Failed to parse wavefront obj file. Message = {e.Message} Path = {metadata.ContentFileRelativePath}");
            throw;
        }


        static Vertex IndexToVertex(in VertexIndex index, WavefrontObj obj) =>
            new()
            {
                Position = obj.Positions[index.Position - 1],
                Texture = obj.Textures[index.Texture - 1],
                Normal = index.Normal == -1 ? Vector3.Zero : obj.Normals[index.Normal - 1]
            };
    }

    private static void CalculateAndSetFaceNormals(Span<Vertex> vertices)
    {
        Debug.Assert(vertices.Length == 3);

        var edge1 = vertices[1].Position - vertices[0].Position;
        var edge2 = vertices[2].Position - vertices[0].Position;

        var normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
        foreach (ref var vertex in vertices)
        {
            vertex.Normal = normal;
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vertex
{
    public Vector3 Position;
    public Vector2 Texture;
    public Vector3 Normal;
}

public struct SubMesh
{
    public int VertexOffset;
    public int VertexCount;
    public int IndexOffset;
    public int IndexCount;
    public int MaterialIndex;
}
