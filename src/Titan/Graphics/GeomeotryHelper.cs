using System.Numerics;

namespace Titan.Graphics;

internal struct Vertex
{
    public Vector3 Position;
    public Vector2 UV;
}

internal ref struct Plane(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<ushort> indices)
{
    public ReadOnlySpan<Vertex> Vertices = vertices;
    public ReadOnlySpan<ushort> Indices = indices;

    public unsafe uint VerticesSize => (uint)(Vertices.Length * sizeof(Vertex));
    public unsafe uint IndicesSize => (uint)(Indices.Length * sizeof(ushort));
    public unsafe uint TotalSize => VerticesSize + IndicesSize;
}

internal ref struct Box(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<ushort> indices)
{
    public ReadOnlySpan<Vertex> Vertices = vertices;
    public ReadOnlySpan<ushort> Indices = indices;

    public unsafe uint VerticesSize => (uint)(Vertices.Length * sizeof(Vertex));
    public unsafe uint IndicesSize => (uint)(Indices.Length * sizeof(ushort));
    public unsafe uint TotalSize => VerticesSize + IndicesSize;
}
internal static class GeomeotryHelper
{

    public static Plane CreatePlane()
    {
        ReadOnlySpan<Vertex> vertices = new Vertex[]
        {
            new() { Position = new(-0.5f, -0.5f,0.0f), UV = new(0.0f, 1.0f) }, // Bottom-left   0
            new() { Position = new(-0.5f, 0.5f,0.0f), UV = new(0.0f, 0.0f) },  // Top-left      1
            new() { Position = new(0.5f, -0.5f,0.0f), UV = new(1.0f, 1.0f) },  // Bottom-right  2
            new() { Position = new(0.5f, 0.5f,0.0f), UV = new(1.0f, 0.0f) },   // Top-right     5
        };
        ReadOnlySpan<ushort> indices = new ushort[] { 0, 1, 2, 2, 1, 3 };
        return new Plane(vertices, indices);
    }
    public static Box CreateBox()
    {
        ReadOnlySpan<Vertex> vertices = new[]
        {
            // Front face
            new Vertex { Position = new(-0.5f, -0.5f, -0.5f), UV = new(0.0f, 1.0f) },
            new Vertex { Position = new(-0.5f,  0.5f, -0.5f), UV = new(0.0f, 0.0f) },
            new Vertex { Position = new( 0.5f,  0.5f, -0.5f), UV = new(1.0f, 0.0f) },
            new Vertex { Position = new( 0.5f, -0.5f, -0.5f), UV = new(1.0f, 1.0f) },

            // Back face
            new Vertex { Position = new(-0.5f, -0.5f, 0.5f), UV = new(1.0f, 1.0f) },
            new Vertex { Position = new( 0.5f, -0.5f, 0.5f), UV = new(0.0f, 1.0f) },
            new Vertex { Position = new( 0.5f,  0.5f, 0.5f), UV = new(0.0f, 0.0f) },
            new Vertex { Position = new(-0.5f,  0.5f, 0.5f), UV = new(1.0f, 0.0f) },

            // Top face
            new Vertex { Position = new(-0.5f, 0.5f, -0.5f), UV = new(0.0f, 1.0f) },
            new Vertex { Position = new(-0.5f, 0.5f,  0.5f), UV = new(0.0f, 0.0f) },
            new Vertex { Position = new( 0.5f, 0.5f,  0.5f), UV = new(1.0f, 0.0f) },
            new Vertex { Position = new( 0.5f, 0.5f, -0.5f), UV = new(1.0f, 1.0f) },

            // Bottom face
            new Vertex { Position = new(-0.5f, -0.5f, -0.5f), UV = new(1.0f, 1.0f) },
            new Vertex { Position = new( 0.5f, -0.5f, -0.5f), UV = new(0.0f, 1.0f) },
            new Vertex { Position = new( 0.5f, -0.5f,  0.5f), UV = new(0.0f, 0.0f) },
            new Vertex { Position = new(-0.5f, -0.5f,  0.5f), UV = new(1.0f, 0.0f) },

            // Right face
            new Vertex { Position = new( 0.5f, -0.5f, -0.5f), UV = new(0.0f, 1.0f) },
            new Vertex { Position = new( 0.5f,  0.5f, -0.5f), UV = new(0.0f, 0.0f) },
            new Vertex { Position = new( 0.5f,  0.5f,  0.5f), UV = new(1.0f, 0.0f) },
            new Vertex { Position = new( 0.5f, -0.5f,  0.5f), UV = new(1.0f, 1.0f) },

            // Left face
            new Vertex { Position = new(-0.5f, -0.5f, -0.5f), UV = new(1.0f, 1.0f) },
            new Vertex { Position = new(-0.5f, -0.5f,  0.5f), UV = new(0.0f, 1.0f) },
            new Vertex { Position = new(-0.5f,  0.5f,  0.5f), UV = new(0.0f, 0.0f) },
            new Vertex { Position = new(-0.5f,  0.5f, -0.5f), UV = new(1.0f, 0.0f) },
        };

        ReadOnlySpan<ushort> indices = new ushort[]
        {
            // Front face
            0, 1, 2,
            0, 2, 3,

            // Back face
            4, 5, 6,
            4, 6, 7,

            // Top face
            8, 9, 10,
            8, 10, 11,

            // Bottom face
            12, 13, 14,
            12, 14, 15,

            // Right face
            16, 17, 18,
            16, 18, 19,

            // Left face
            20, 21, 22,
            20, 22, 23,
        };

        return new(vertices, indices);
    }
}
