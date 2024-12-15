#include "Shader.DeferredLighting.hlsli"


float3 CalculateBlinnPhongLighting(LightInstanceData light, float3 position, float3 normal, float3 albedo, float specularStrength)
{
    float3 cameraPosition = GetCameraPosition();

    float3 ambientLight = GetAmbientLight();
    float3 ambient = ambientLight * albedo;
    float3 lightDirection = normalize(light.Position - position);
    float diff = max(dot(normal, lightDirection), 0.0);
    float3 diffuse = diff * light.Color * albedo;

    float3 viewDirection = normalize(cameraPosition - position);
    float3 halfwayDirection = normalize(lightDirection + viewDirection);
    float spec = pow(max(dot(normal, halfwayDirection), 0.0), 16.0);
    float3 specular = spec * light.Color * specularStrength;
    return ambient + diffuse + specular;
}


float4 main(LightsVertexOutput input): SV_TARGET0
{
    Texture2D gPosition = GetInputTexture(PositionIndex);
    Texture2D gAlbedo = GetInputTexture(AlbedoIndex);
    Texture2D gNormal = GetInputTexture(NormalIndex);
    Texture2D gSpecular = GetInputTexture(SpecularIndex);

    float3 position = gPosition.Sample(PointSampler, input.Texture).rgb;
    float3 normal = gNormal.Sample(PointSampler, input.Texture).rgb;
    float4 albedo  = gAlbedo.Sample(PointSampler, input.Texture);
    float3 albedoColor = albedo.rgb;

    LightInstanceData light = GetLight(input.InstanceId);
    float3 lighting = CalculateBlinnPhongLighting(light, position, normal, albedoColor, albedo.a);

    return float4(lighting, 1.0);
}