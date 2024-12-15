namespace Titan.Rendering;

public struct Viewport
{
    //NOTE(Jens): Make sure these are the same as the render API being used (only d3d12 at the moment)
    public float TopLeftX;
    public float TopLeftY;
    public float Width;
    public float Height;
    public float MinDepth;
    public float MaxDepth;
}
