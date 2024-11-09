#define NUM_INPUTS 0

#include "Shader.Engine.Defaults.hlsli"
#include "Shader.UI.hlsli"

static const float2 Positions[6] = {
    float2(-1.0f, -1.0f), // Bottom-left
    float2(-1.0f, 1.0f),  // Top-left
    float2(1.0f, -1.0f),  // Bottom-right
    float2(-1.0f, 1.0f),  // Top-left
    float2(1.0f, 1.0f),   // Top-right
    float2(1.0f, -1.0f)   // Bottom-right
};

UIVertexOutput main(uint id : SV_VertexID, in uint InstanceIdx : SV_InstanceID)
{
 	float2 pos = Positions[id];
  	float2 uv = pos * 0.5f + 0.5f;

    UIElement ui = UIElements[InstanceIdx];

    float2 elementCenterPixels = ui.Offset + (ui.Size * 0.5f);
    float2 elementCenterNDC = (elementCenterPixels / float2(GetWindowWidth(), GetWindowHeight())) * 2.0f - 1.0f;

    float2 elementHalfSizeNDC = (ui.Size / float2(GetWindowWidth(), GetWindowHeight()));

    float2 finalPositionNDC = elementCenterNDC + elementHalfSizeNDC * pos;

    // Output structure setup
    UIVertexOutput output;
    output.Position = float4(finalPositionNDC, 0.0f, 1.0f);
    output.Texture = uv;
    output.Color = ui.Color;

    return output;
}