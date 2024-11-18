#define NUM_INPUTS 0

#include "Shader.Engine.Defaults.hlsli"
#include "Shader.UI.hlsli"

static const float2 Positions[4] = {
    float2(-1.0f, -1.0f), // Bottom-left
    float2(-1.0f, 1.0f),  // Top-left
    float2(1.0f, 1.0f),   // Top-right
    float2(1.0f, -1.0f),  // Bottom-right
};

static const float2 UVOffsets[4] = {
    float2(0.0f, 0.0f), // Bottom-left
    float2(0.0f, 1.0f), // Top-left
    float2(1.0f, 1.0f), // Top-right
    float2(1.0f, 0.0f)  // Bottom-right
};

UIVertexOutput main(uint id : SV_VertexID, in uint InstanceIdx : SV_InstanceID)
{
    float2 windowSize = GetWindowSize();
 	float2 pos = Positions[id];
  	// float2 uv = pos * 0.5f + 0.5f;

    UIElement ui = UIElements[InstanceIdx];
    
    float2 uvOffset = UVOffsets[id];
    float2 uv = ui.UVMin + uvOffset * (ui.UVMax - ui.UVMin);


    float2 pivot = ui.Offset + (ui.Size * 0.5f);
    float2 center = (pivot / windowSize) * 2.0f - 1.0f;

    float2 elementHalfSize = (ui.Size / windowSize);

    float2 position = center + elementHalfSize * pos;

    // Output structure setup
    UIVertexOutput output;
    output.Position = float4(position, 0.0f, 1.0f);
    output.Texture = uv;
    output.Color = ui.Color;
    output.TextureId = ui.TextureId;
    output.Type = ui.Type;

    return output;
}